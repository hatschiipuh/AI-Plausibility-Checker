import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  effect,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { defer, finalize } from 'rxjs';
import {
  BusinessServiceClient,
  CreateValidationRequest,
  SwaggerException,
  ValidateStepRequest,
  Validation,
  ValidationDetails,
  ValidationStep,
} from './api/business-service-client';
import {
  InquiryDialogComponent,
  InquiryDialogResult,
} from './inquiry-dialog/inquiry-dialog.component';
import { AuthService } from './auth.service';
import { AccountMenuComponent } from './account-menu/account-menu.component';
import { VALIDATION_INPUT_LIMITS } from './validation-input-limits';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    MatToolbarModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressBarModule,
    MatListModule,
    MatIconModule,
    AccountMenuComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  protected readonly authService = inject(AuthService);
  protected readonly inputLimits = VALIDATION_INPUT_LIMITS;

  private readonly client = inject(BusinessServiceClient);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private isInquiryDialogOpen = false;

  currentValidation?: Validation;
  validationDetails?: ValidationDetails;
  steps: ValidationStep[] = [];
  question = '';
  initialAnswer = '';
  lastScore?: number;
  lastExplanation?: string;
  nextInquiry?: string | null;
  isCompleted = false;
  protected errorMessage?: string;
  protected isCreatingValidation = false;
  protected isValidating = false;
  protected isLoadingDetails = false;

  private retryAction?:
    | { kind: 'step'; validationId: string; inquiry: string; userAnswer: string }
    | { kind: 'details'; validationId: string };

  protected get isBusy(): boolean {
    return this.isCreatingValidation || this.isValidating || this.isLoadingDetails;
  }

  protected get canRetryStep(): boolean {
    return this.retryAction?.kind === 'step' && !this.isValidating;
  }

  protected get canRetryDetails(): boolean {
    return this.retryAction?.kind === 'details' && !this.isLoadingDetails;
  }

  constructor() {
    this.authService.restoreSession();

    effect(() => {
      if (!this.authService.isAuthenticated()) {
        this.clearValidationState();
        this.changeDetectorRef.markForCheck();
      }
    });
  }

  onCreateValidation(): void {
    if (!this.authService.currentUser()?.id || this.isBusy) {
      return;
    }

    const request = new CreateValidationRequest({
      question: this.question.trim(),
      initialAnswer: this.initialAnswer.trim(),
    });

    if (!request.question || !request.initialAnswer) {
      return;
    }

    this.errorMessage = undefined;
    this.retryAction = undefined;
    this.isCreatingValidation = true;
    this.changeDetectorRef.markForCheck();

    defer(() => this.client.createValidation(request))
      .pipe(
        finalize(() => {
          this.isCreatingValidation = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (validation) => {
          if (!validation.id) {
            this.errorMessage =
              'Die Validation konnte nicht gestartet werden, weil die Serverantwort keine ID enthält.';
            this.changeDetectorRef.markForCheck();
            return;
          }

          this.currentValidation = validation;
          this.validationDetails = undefined;
          this.steps = [];
          this.lastScore = undefined;
          this.lastExplanation = undefined;
          this.nextInquiry = null;
          this.isCompleted = validation.isCompleted ?? false;
          this.validateStep(validation.id, request.question, request.initialAnswer);
          this.changeDetectorRef.markForCheck();
        },
        error: (error: unknown) => {
          this.errorMessage = this.getRequestErrorMessage(
            error,
            'Die Validation konnte nicht angelegt werden. Bitte prüfe deine Verbindung und versuche es erneut.',
          );
          this.changeDetectorRef.markForCheck();
        },
      });
  }

  private validateStep(validationId: string, inquiry: string, userAnswer: string): void {
    if (this.isValidating) {
      return;
    }

    const request = new ValidateStepRequest({
      inquiry,
      userAnswer,
    });

    const retryAction = { kind: 'step' as const, validationId, inquiry, userAnswer };
    this.retryAction = undefined;
    this.errorMessage = undefined;
    this.isValidating = true;
    this.changeDetectorRef.markForCheck();

    defer(() => this.client.validateStep(validationId, request))
      .pipe(
        finalize(() => {
          this.isValidating = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          this.handleValidateStepResponse(response);
        },
        error: (error: unknown) => {
          this.errorMessage = this.getRequestErrorMessage(
            error,
            'Die Antwort konnte nicht bewertet werden. Deine Eingabe bleibt erhalten; du kannst es erneut versuchen.',
          );
          this.retryAction = retryAction;
          this.changeDetectorRef.markForCheck();
        },
      });
  }

  protected retryPendingStep(): void {
    const action = this.retryAction;
    if (action?.kind !== 'step' || this.isValidating) {
      return;
    }

    this.validateStep(action.validationId, action.inquiry, action.userAnswer);
  }

  private handleValidateStepResponse(response: {
    score?: number;
    aiExplanation?: string | undefined;
    nextInquiry?: string | undefined;
    isCompleted?: boolean;
  }): void {
    this.lastScore = response.score;
    this.lastExplanation = response.aiExplanation;
    this.nextInquiry = response.nextInquiry;
    this.isCompleted = response.isCompleted ?? false;
    this.changeDetectorRef.markForCheck();

    if (this.currentValidation?.id) {
      this.loadValidationDetails(this.currentValidation.id);
    }

    if (response.nextInquiry && !response.isCompleted) {
      this.openInquiryDialog(response.nextInquiry);
    }
  }

  loadValidationDetails(validationId: string): void {
    if (this.isLoadingDetails) {
      return;
    }

    const retryAction = { kind: 'details' as const, validationId };
    this.retryAction = undefined;
    this.errorMessage = undefined;
    this.isLoadingDetails = true;
    this.changeDetectorRef.markForCheck();

    defer(() => this.client.getValidationDetails(validationId))
      .pipe(
        finalize(() => {
          this.isLoadingDetails = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (details) => {
          this.validationDetails = details;
          this.steps = details.steps ?? [];
          this.isCompleted = details.isCompleted ?? false;
          this.changeDetectorRef.markForCheck();

          const openInquiry = this.getOpenInquiry(this.steps);
          if (openInquiry && !this.isCompleted && !this.isInquiryDialogOpen) {
            this.openInquiryDialog(openInquiry);
          }
        },
        error: (error: unknown) => {
          this.errorMessage = this.getRequestErrorMessage(
            error,
            'Der Verlauf konnte nicht geladen werden. Bitte versuche es erneut.',
          );
          this.retryAction = retryAction;
          this.changeDetectorRef.markForCheck();
        },
      });
  }

  protected retryLoadValidationDetails(): void {
    const action = this.retryAction;
    if (action?.kind === 'details' && !this.isLoadingDetails) {
      this.loadValidationDetails(action.validationId);
    }
  }

  private getRequestErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof SwaggerException) {
      if (error.status === 400) {
        return 'Der Server hat die Anfrage abgelehnt. Bitte prüfe deine Eingaben.';
      }

      if (error.status === 401) {
        return 'Deine Anmeldung ist abgelaufen. Bitte melde dich erneut an.';
      }

      if (error.status === 502) {
        return 'Der Scoring-Dienst ist derzeit nicht erreichbar. Deine Eingabe bleibt erhalten; du kannst es erneut versuchen.';
      }
    }

    return fallback;
  }

  private getOpenInquiry(steps: ValidationStep[]): string | undefined {
    const latestUnansweredStep = [...steps]
      .reverse()
      .find((step) => !!step.inquiry?.trim() && !step.userAnswer?.trim());

    return latestUnansweredStep?.inquiry?.trim();
  }

  openInquiryDialog(inquiry: string): void {
    if (!this.currentValidation?.id) {
      return;
    }

    if (this.isInquiryDialogOpen) {
      return;
    }

    this.isInquiryDialogOpen = true;

    const dialogRef = this.dialog.open<
      InquiryDialogComponent,
      { inquiry: string },
      InquiryDialogResult
    >(InquiryDialogComponent, {
      width: '640px',
      disableClose: true,
      data: { inquiry },
    });

    dialogRef.afterClosed().subscribe((result) => {
      this.isInquiryDialogOpen = false;
      this.changeDetectorRef.markForCheck();

      if (!result?.userAnswer || !this.currentValidation?.id) {
        return;
      }

      this.validateStep(this.currentValidation.id, inquiry, result.userAnswer);
    });
  }

  private clearValidationState(): void {
    this.currentValidation = undefined;
    this.validationDetails = undefined;
    this.steps = [];
    this.question = '';
    this.initialAnswer = '';
    this.lastScore = undefined;
    this.lastExplanation = undefined;
    this.nextInquiry = null;
    this.isCompleted = false;
    this.errorMessage = undefined;
    this.retryAction = undefined;
  }
}
