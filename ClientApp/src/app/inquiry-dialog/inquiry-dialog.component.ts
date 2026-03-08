import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface InquiryDialogData {
  inquiry: string;
}

export interface InquiryDialogResult {
  userAnswer: string;
}

@Component({
  selector: 'app-inquiry-dialog',
  imports: [MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, FormsModule],
  templateUrl: './inquiry-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'inquiry-dialog-host'
  }
})
export class InquiryDialogComponent {
  readonly data = inject<InquiryDialogData>(MAT_DIALOG_DATA);

  private readonly dialogRef = inject(MatDialogRef<InquiryDialogComponent, InquiryDialogResult>);

  userAnswer = '';

  onSubmit(): void {
    const trimmedAnswer = this.userAnswer.trim();
    if (!trimmedAnswer) {
      return;
    }

    this.dialogRef.close({ userAnswer: trimmedAnswer });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}