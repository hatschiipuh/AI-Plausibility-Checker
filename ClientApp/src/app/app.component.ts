import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import {
  BusinessServiceClient,
  CreateValidationRequest,
  GetOrCreateUserRequest,
  UserDto,
  ValidateStepRequest,
  ValidationDetailsDto,
  ValidationDto,
  ValidationStepDto
} from './api/business-service-client';
import {
  InquiryDialogComponent,
  InquiryDialogResult
} from './inquiry-dialog/inquiry-dialog.component';

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
    MatListModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  private readonly client = inject(BusinessServiceClient);
  private readonly dialog = inject(MatDialog);

  currentUser?: UserDto;
  currentValidation?: ValidationDto;
  validationDetails?: ValidationDetailsDto;
  steps: ValidationStepDto[] = [];
  email = '';
  displayName = '';
  question = '';
  initialAnswer = '';
  firstInquiry = '';
  firstUserAnswer = '';
  lastScore?: number;
  lastExplanation?: string;
  nextInquiry?: string | null;
  isCompleted = false;

  onGetOrCreateUser(): void {
    const request = new GetOrCreateUserRequest({
      email: this.email.trim(),
      displayName: this.displayName.trim()
    });

    if (!request.email || !request.displayName) {
      return;
    }

    this.client.getOrCreateUser(request).subscribe((user) => {
      this.currentUser = user;
    });
  }

  onCreateValidation(): void {
    if (!this.currentUser?.id) {
      return;
    }

    const request = new CreateValidationRequest({
      userId: this.currentUser.id,
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
        this.loadValidationDetails(validation.id);
      }
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

    const request = new ValidateStepRequest({
      inquiry,
      userAnswer
    });

    this.client.validateStep(this.currentValidation.id, request).subscribe((response) => {
      this.lastScore = response.score;
      this.lastExplanation = response.aiExplanation;
      this.nextInquiry = response.nextInquiry;
      this.isCompleted = response.isCompleted ?? false;
      if (this.currentValidation?.id) {
        this.loadValidationDetails(this.currentValidation.id);
      }

      if (response.nextInquiry && !response.isCompleted) {
        this.openInquiryDialog(response.nextInquiry);
      }
    });
  }

  loadValidationDetails(validationId: string): void {
    this.client.getValidationDetails(validationId).subscribe((details) => {
      this.validationDetails = details;
      this.steps = details.steps ?? [];
      this.isCompleted = details.isCompleted ?? false;
    });
  }

  openInquiryDialog(inquiry: string): void {
    if (!this.currentValidation?.id) {
      return;
    }

    const dialogRef = this.dialog.open<InquiryDialogComponent, { inquiry: string }, InquiryDialogResult>(
      InquiryDialogComponent,
      {
        width: '640px',
        disableClose: true,
        data: { inquiry }
      }
    );

    dialogRef.afterClosed().subscribe((result) => {
      if (!result?.userAnswer || !this.currentValidation?.id) {
        return;
      }

      const request = new ValidateStepRequest({
        inquiry,
        userAnswer: result.userAnswer
      });

      this.client.validateStep(this.currentValidation.id, request).subscribe((response) => {
        this.lastScore = response.score;
        this.lastExplanation = response.aiExplanation;
        this.nextInquiry = response.nextInquiry;
        this.isCompleted = response.isCompleted ?? false;
        if (this.currentValidation?.id) {
          this.loadValidationDetails(this.currentValidation.id);
        }

        if (response.nextInquiry && !response.isCompleted) {
          this.openInquiryDialog(response.nextInquiry);
        }
      });
    });
  }
}
