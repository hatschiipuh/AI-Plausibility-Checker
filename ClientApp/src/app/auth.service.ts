import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AuthResponse,
  BusinessServiceClient,
  LoginRequest,
  RegisterRequest,
  User
} from './api/business-service-client';

const accessTokenStorageKey = 'accessToken';

/**
 * Centralizes authentication state (current user, access token persistence)
 * and the register/login/logout/delete-account flows.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly client = inject(BusinessServiceClient);

  private readonly currentUserSignal = signal<User | undefined>(undefined);
  private readonly errorMessageSignal = signal<string | undefined>(undefined);
  private readonly isLoadingSignal = signal(false);

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.currentUserSignal());
  readonly errorMessage = this.errorMessageSignal.asReadonly();
  readonly isLoading = this.isLoadingSignal.asReadonly();

  restoreSession(): void {
    const accessToken = localStorage.getItem(accessTokenStorageKey);
    if (!accessToken) {
      return;
    }

    this.client.meGET().subscribe({
      next: (user) => {
        this.currentUserSignal.set(user);
        this.errorMessageSignal.set(undefined);
      },
      error: () => {
        localStorage.removeItem(accessTokenStorageKey);
        this.currentUserSignal.set(undefined);
      }
    });
  }

  register(request: RegisterRequest, onSuccess: () => void): void {
    this.errorMessageSignal.set(undefined);
    this.isLoadingSignal.set(true);

    this.client.register(request).subscribe({
      next: (response) => {
        this.applyAuthResponse(response);
        this.isLoadingSignal.set(false);
        onSuccess();
      },
      error: (error: unknown) => {
        this.handleAuthError(error);
        this.isLoadingSignal.set(false);
      }
    });
  }

  login(request: LoginRequest, onSuccess: () => void): void {
    this.errorMessageSignal.set(undefined);
    this.isLoadingSignal.set(true);

    this.client.login(request).subscribe({
      next: (response) => {
        this.applyAuthResponse(response);
        this.isLoadingSignal.set(false);
        onSuccess();
      },
      error: (error: unknown) => {
        this.handleAuthError(error);
        this.isLoadingSignal.set(false);
      }
    });
  }

  logout(): void {
    localStorage.removeItem(accessTokenStorageKey);
    this.currentUserSignal.set(undefined);
    this.errorMessageSignal.set(undefined);
  }

  deleteAccount(onSuccess: () => void): void {
    this.isLoadingSignal.set(true);

    this.client.meDELETE().subscribe({
      next: () => {
        localStorage.removeItem(accessTokenStorageKey);
        this.currentUserSignal.set(undefined);
        this.errorMessageSignal.set(undefined);
        this.isLoadingSignal.set(false);
        onSuccess();
      },
      error: (error: unknown) => {
        this.handleAuthError(error);
        this.isLoadingSignal.set(false);
      }
    });
  }

  clearError(): void {
    this.errorMessageSignal.set(undefined);
  }

  private applyAuthResponse(response: AuthResponse): void {
    if (!response.accessToken || !response.user) {
      this.errorMessageSignal.set('Auth response is incomplete.');
      return;
    }

    localStorage.setItem(accessTokenStorageKey, response.accessToken);
    this.currentUserSignal.set(response.user);
    this.errorMessageSignal.set(undefined);
  }

  private handleAuthError(error: unknown): void {
    if (error instanceof HttpErrorResponse) {
      this.errorMessageSignal.set(
        typeof error.error === 'string' ? error.error : error.message
      );
    } else {
      this.errorMessageSignal.set('Authentication failed.');
    }
  }
}
