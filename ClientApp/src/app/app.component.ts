import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, inject } from '@angular/core';
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
import {
  BusinessServiceClient,
  CreateValidationRequest,
  ValidateStepRequest,
  Validation,
  ValidationDetails,
  ValidationStep
} from './api/business-service-client';
import {
  InquiryDialogComponent,
  InquiryDialogResult
} from './inquiry-dialog/inquiry-dialog.component';
import { AuthService } from './auth.service';
import { AccountMenuComponent } from './account-menu/account-menu.component';

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
    AccountMenuComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  protected readonly authService = inject(AuthService);

  private readonly client = inject(BusinessServiceClient);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly dialog = inject(MatDialog);
  private isInquiryDialogOpen = false;

  currentValidation?: Validation;
  validationDetails?: ValidationDetails;
  steps: ValidationStep[] = [];
  question = '';
  initialAnswer = '';
  firstInquiry = '';
  firstUserAnswer = '';
  lastScore?: number;
  lastExplanation?: string;
  nextInquiry?: string | null;
  isCompleted = false;

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
    if (!this.authService.currentUser()?.id) {
      return;
    }

    const request = new CreateValidationRequest({
      question: this.question.trim(),
      initialAnswer: this.initialAnswer.trim()
    });

    if (!request.question || !request.initialAnswer) {
      return;
    }

    this.client.createValidation(request).subscribe((validation) => {
      this.currentValidation = validation;
      this.validationDetails = undefined;
      this.steps = [];
      this.lastScore = undefined;
      this.lastExplanation = undefined;
      this.nextInquiry = null;
      this.isCompleted = validation.isCompleted ?? false;
      this.firstInquiry = '';
      this.firstUserAnswer = '';
      if (validation.id) {
        this.validateStep(validation.id, this.question.trim(), this.initialAnswer.trim());
      }

      this.changeDetectorRef.markForCheck();
    });
  }

  onFirstValidate(): void {
    if (!this.currentValidation?.id) {
      return;
    }

    const inquiry = this.firstInquiry.trim();
    const userAnswer = this.firstUserAnswer.trim();
    if (!inquiry || !userAnswer) {
      return;
    }

    this.validateStep(this.currentValidation.id, inquiry, userAnswer);
  }

  private validateStep(validationId: string, inquiry: string, userAnswer: string): void {
    const request = new ValidateStepRequest({
      inquiry,
      userAnswer
    });

    this.client.validateStep(validationId, request).subscribe((response) => {
      this.handleValidateStepResponse(response);
    });
  }

  private handleValidateStepResponse(response: { score?: number; aiExplanation?: string | undefined; nextInquiry?: string | undefined; isCompleted?: boolean }): void {
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
    this.client.getValidationDetails(validationId).subscribe((details) => {
      this.validationDetails = details;
      this.steps = details.steps ?? [];
      this.isCompleted = details.isCompleted ?? false;
      this.changeDetectorRef.markForCheck();

      const openInquiry = this.getOpenInquiry(this.steps);
      if (openInquiry && !this.isCompleted && !this.isInquiryDialogOpen) {
        this.openInquiryDialog(openInquiry);
      }
    });
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

    const dialogRef = this.dialog.open<InquiryDialogComponent, { inquiry: string }, InquiryDialogResult>(
      InquiryDialogComponent,
      {
        width: '640px',
        disableClose: true,
        data: { inquiry }
      }
    );

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
    this.firstInquiry = '';
    this.firstUserAnswer = '';
    this.lastScore = undefined;
    this.lastExplanation = undefined;
    this.nextInquiry = null;
    this.isCompleted = false;
  }
}
