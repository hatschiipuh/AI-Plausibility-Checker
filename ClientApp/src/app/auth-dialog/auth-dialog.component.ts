import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { AuthService } from '../auth.service';
import { LoginRequest, RegisterRequest } from '../api/business-service-client';

@Component({
  selector: 'app-auth-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule
  ],
  templateUrl: './auth-dialog.component.html',
  styleUrl: './auth-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuthDialogComponent {
  private readonly authService = inject(AuthService);
  private readonly dialogRef = inject(MatDialogRef<AuthDialogComponent, boolean>);
  private readonly formBuilder = inject(FormBuilder);

  readonly isLoading = this.authService.isLoading;
  readonly errorMessage = this.authService.errorMessage;

  readonly loginForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  readonly registerForm = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  onLoginSubmit(): void {
    if (this.loginForm.invalid || this.isLoading()) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password } = this.loginForm.getRawValue();
    const request = new LoginRequest({ email: email.trim(), password });

    this.authService.login(request, () => {
      this.dialogRef.close(true);
    });
  }

  onRegisterSubmit(): void {
    if (this.registerForm.invalid || this.isLoading()) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const { displayName, email, password } = this.registerForm.getRawValue();
    const request = new RegisterRequest({
      displayName: displayName.trim(),
      email: email.trim(),
      password
    });

    this.authService.register(request, () => {
      this.dialogRef.close(true);
    });
  }

  onTabChange(): void {
    this.authService.clearError();
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
