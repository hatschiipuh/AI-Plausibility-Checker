import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { AuthService } from '../auth.service';
import { LoginRequest, RegisterRequest } from '../api/business-service-client';
import { AuthDialogComponent } from './auth-dialog.component';

describe('AuthDialogComponent', () => {
  let fixture: ComponentFixture<AuthDialogComponent>;
  let component: AuthDialogComponent;
  let authService: {
    isLoading: ReturnType<typeof signal<boolean>>;
    errorMessage: ReturnType<typeof signal<string | undefined>>;
    login: ReturnType<typeof vi.fn>;
    register: ReturnType<typeof vi.fn>;
    clearError: ReturnType<typeof vi.fn>;
  };
  let dialogRefClose: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    authService = {
      isLoading: signal(false),
      errorMessage: signal<string | undefined>(undefined),
      login: vi.fn(),
      register: vi.fn(),
      clearError: vi.fn(),
    };
    dialogRefClose = vi.fn();

    await TestBed.configureTestingModule({
      imports: [AuthDialogComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: MatDialogRef, useValue: { close: dialogRefClose } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('marks the login form as touched and does not call the auth service when invalid', () => {
    component.onLoginSubmit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.loginForm.controls.email.touched).toBe(true);
    expect(component.loginForm.controls.password.touched).toBe(true);
  });

  it('logs in with the entered credentials and closes the dialog on success', () => {
    component.loginForm.setValue({ email: 'user@example.com', password: 'secret123' });

    authService.login.mockImplementation((_request: LoginRequest, onSuccess: () => void) => {
      onSuccess();
    });

    component.onLoginSubmit();

    expect(authService.login).toHaveBeenCalledTimes(1);
    const requestArg = authService.login.mock.calls[0][0] as LoginRequest;
    expect(requestArg.email).toBe('user@example.com');
    expect(requestArg.password).toBe('secret123');
    expect(dialogRefClose).toHaveBeenCalledWith(true);
  });

  it('does not submit the login form while a request is already loading', () => {
    component.loginForm.setValue({ email: 'user@example.com', password: 'secret123' });
    authService.isLoading.set(true);

    component.onLoginSubmit();

    expect(authService.login).not.toHaveBeenCalled();
  });

  it('marks the register form as touched and does not call the auth service when invalid', () => {
    component.onRegisterSubmit();

    expect(authService.register).not.toHaveBeenCalled();
    expect(component.registerForm.controls.displayName.touched).toBe(true);
    expect(component.registerForm.controls.email.touched).toBe(true);
    expect(component.registerForm.controls.password.touched).toBe(true);
  });

  it('rejects a password shorter than 8 characters on registration', () => {
    component.registerForm.setValue({
      displayName: 'Test User',
      email: 'user@example.com',
      password: 'short',
    });

    component.onRegisterSubmit();

    expect(authService.register).not.toHaveBeenCalled();
    expect(component.registerForm.controls.password.invalid).toBe(true);
  });

  it('registers with the trimmed display name and email and closes the dialog on success', () => {
    component.registerForm.setValue({
      displayName: ' Test User ',
      email: 'user@example.com',
      password: 'secret123',
    });

    authService.register.mockImplementation((_request: RegisterRequest, onSuccess: () => void) => {
      onSuccess();
    });

    component.onRegisterSubmit();

    expect(authService.register).toHaveBeenCalledTimes(1);
    const requestArg = authService.register.mock.calls[0][0] as RegisterRequest;
    expect(requestArg.displayName).toBe('Test User');
    expect(requestArg.email).toBe('user@example.com');
    expect(dialogRefClose).toHaveBeenCalledWith(true);
  });

  it('clears a previous error message when switching tabs', () => {
    component.onTabChange();

    expect(authService.clearError).toHaveBeenCalledTimes(1);
  });

  it('closes the dialog with false when cancelled', () => {
    component.onCancel();

    expect(dialogRefClose).toHaveBeenCalledWith(false);
  });
});
