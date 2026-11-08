import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import {
  AuthResponse,
  BusinessServiceClient,
  LoginRequest,
  RegisterRequest,
  User,
} from './api/business-service-client';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let register: Mock<BusinessServiceClient['register']>;
  let login: Mock<BusinessServiceClient['login']>;
  let meGET: Mock<BusinessServiceClient['meGET']>;
  let meDELETE: Mock<BusinessServiceClient['meDELETE']>;

  const accessTokenStorageKey = 'accessToken';
  const user = new User({ id: 'user-id', email: 'user@example.com', displayName: 'Test User' });

  beforeEach(() => {
    register = vi.fn<BusinessServiceClient['register']>();
    login = vi.fn<BusinessServiceClient['login']>();
    meGET = vi.fn<BusinessServiceClient['meGET']>();
    meDELETE = vi.fn<BusinessServiceClient['meDELETE']>();

    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: BusinessServiceClient, useValue: { register, login, meGET, meDELETE } },
      ],
    });

    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('restores the session from a stored access token', () => {
    localStorage.setItem(accessTokenStorageKey, 'stored-token');
    meGET.mockReturnValue(of(user));

    service.restoreSession();

    expect(service.currentUser()).toEqual(user);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('does not call the API when no access token is stored', () => {
    service.restoreSession();

    expect(meGET).not.toHaveBeenCalled();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('clears the stored access token when restoring the session fails', () => {
    localStorage.setItem(accessTokenStorageKey, 'stored-token');
    meGET.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 401 })));

    service.restoreSession();

    expect(localStorage.getItem(accessTokenStorageKey)).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('stores the access token and current user on successful login', () => {
    const response = new AuthResponse({ accessToken: 'new-token', user });
    login.mockReturnValue(of(response));
    const onSuccess = vi.fn();

    service.login(new LoginRequest({ email: user.email, password: 'secret123' }), onSuccess);

    expect(localStorage.getItem(accessTokenStorageKey)).toBe('new-token');
    expect(service.currentUser()).toEqual(user);
    expect(onSuccess).toHaveBeenCalledTimes(1);
  });

  it('sets an error message and does not call onSuccess when login fails', () => {
    login.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401, error: 'Invalid email or password.' })),
    );
    const onSuccess = vi.fn();

    service.login(new LoginRequest({ email: user.email, password: 'wrong' }), onSuccess);

    expect(service.errorMessage()).toBe('Invalid email or password.');
    expect(service.isAuthenticated()).toBe(false);
    expect(onSuccess).not.toHaveBeenCalled();
  });

  it('stores the access token and current user on successful registration', () => {
    const response = new AuthResponse({ accessToken: 'new-token', user });
    register.mockReturnValue(of(response));
    const onSuccess = vi.fn();

    service.register(
      new RegisterRequest({ email: user.email, displayName: user.displayName, password: 'secret123' }),
      onSuccess,
    );

    expect(localStorage.getItem(accessTokenStorageKey)).toBe('new-token');
    expect(service.currentUser()).toEqual(user);
    expect(onSuccess).toHaveBeenCalledTimes(1);
  });

  it('reports an incomplete auth response instead of applying it', () => {
    register.mockReturnValue(of(new AuthResponse({ accessToken: undefined, user: undefined })));
    const onSuccess = vi.fn();

    service.register(
      new RegisterRequest({ email: user.email, displayName: user.displayName, password: 'secret123' }),
      onSuccess,
    );

    expect(service.errorMessage()).toBe('Auth response is incomplete.');
    expect(service.isAuthenticated()).toBe(false);
    expect(onSuccess).toHaveBeenCalledTimes(1);
  });

  it('clears the access token and current user on logout', () => {
    localStorage.setItem(accessTokenStorageKey, 'stored-token');
    meGET.mockReturnValue(of(user));
    service.restoreSession();

    service.logout();

    expect(localStorage.getItem(accessTokenStorageKey)).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('clears the current user on successful account deletion', () => {
    localStorage.setItem(accessTokenStorageKey, 'stored-token');
    meGET.mockReturnValue(of(user));
    service.restoreSession();
    meDELETE.mockReturnValue(of(undefined));
    const onSuccess = vi.fn();

    service.deleteAccount(onSuccess);

    expect(localStorage.getItem(accessTokenStorageKey)).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(onSuccess).toHaveBeenCalledTimes(1);
  });

  it('keeps the current user and reports the server message when account deletion fails', () => {
    localStorage.setItem(accessTokenStorageKey, 'stored-token');
    meGET.mockReturnValue(of(user));
    service.restoreSession();
    meDELETE.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: 'Internal server error.' })),
    );
    const onSuccess = vi.fn();

    service.deleteAccount(onSuccess);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.errorMessage()).toBe('Internal server error.');
    expect(onSuccess).not.toHaveBeenCalled();
  });

  it('reports a generic error message for non-HTTP failures', () => {
    login.mockReturnValue(throwError(() => new Error('boom')));

    service.login(new LoginRequest({ email: user.email, password: 'secret123' }), () => {});

    expect(service.errorMessage()).toBe('Authentication failed.');
  });

  it('clears a previously set error message', () => {
    login.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 401 })));
    service.login(new LoginRequest({ email: user.email, password: 'wrong' }), () => {});
    expect(service.errorMessage()).toBeDefined();

    service.clearError();

    expect(service.errorMessage()).toBeUndefined();
  });
});
