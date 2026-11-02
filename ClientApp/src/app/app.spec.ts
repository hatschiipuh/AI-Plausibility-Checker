import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of, Subject, throwError } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { AuthService } from './auth.service';
import {
  BusinessServiceClient,
  Validation,
  ValidationDetails,
  ValidateStepResponse,
} from './api/business-service-client';
import { App } from './app';

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let app: App;
  let createValidation: Mock<BusinessServiceClient['createValidation']>;
  let validateStep: Mock<BusinessServiceClient['validateStep']>;
  let getValidationDetails: Mock<BusinessServiceClient['getValidationDetails']>;

  beforeEach(async () => {
    createValidation = vi.fn<BusinessServiceClient['createValidation']>();
    validateStep = vi.fn<BusinessServiceClient['validateStep']>();
    getValidationDetails = vi.fn<BusinessServiceClient['getValidationDetails']>();

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        {
          provide: BusinessServiceClient,
          useValue: { createValidation, validateStep, getValidationDetails },
        },
        {
          provide: AuthService,
          useValue: {
            currentUser: () => ({
              id: 'user-id',
              email: 'user@example.com',
              displayName: 'Test User',
            }),
            isAuthenticated: () => true,
            restoreSession: vi.fn(),
            logout: vi.fn(),
            deleteAccount: vi.fn(),
            clearError: vi.fn(),
          },
        },
        { provide: MatDialog, useValue: { open: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    app = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the app', () => {
    expect(app).toBeTruthy();
  });

  it('should render title', async () => {
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('mat-toolbar')?.textContent).toContain(
      'AI-Assisted Plausibility Checker',
    );
  });

  it('should render account menu button', async () => {
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-account-menu')).toBeTruthy();
  });

  it('prevents duplicate create requests and reports failures', () => {
    const response = new Subject<Validation>();
    createValidation.mockReturnValue(response.asObservable());
    app.question = 'Question?';
    app.initialAnswer = 'Initial answer';

    app.onCreateValidation();
    app.onCreateValidation();
    fixture.detectChanges();

    expect(createValidation).toHaveBeenCalledTimes(1);
    const submitButton = fixture.nativeElement.querySelector(
      'mat-card-actions button',
    ) as HTMLButtonElement;
    expect(submitButton.disabled).toBe(true);

    response.error(new Error('Network unavailable'));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Validation konnte nicht angelegt werden',
    );
    expect(submitButton.disabled).toBe(false);
  });

  it('prevents duplicate scoring requests while one is pending', () => {
    const response = new Subject<ValidateStepResponse>();
    validateStep.mockReturnValue(response.asObservable());
    app.currentValidation = new Validation({ id: 'validation-id' });
    app.firstInquiry = 'Follow-up question?';
    app.firstUserAnswer = 'Answer';

    app.onFirstValidate();
    app.onFirstValidate();

    expect(validateStep).toHaveBeenCalledTimes(1);
    response.error(new Error('Scoring unavailable'));
  });

  it('retains a failed validation step and allows retry', () => {
    createValidation.mockReturnValue(of(new Validation({ id: 'validation-id' })));
    validateStep
      .mockReturnValueOnce(throwError(() => new Error('Scoring unavailable')))
      .mockReturnValue(of(new ValidateStepResponse({ isCompleted: true, score: 0.8 })));
    getValidationDetails.mockReturnValue(of(new ValidationDetails({ steps: [] })));
    app.question = 'Question?';
    app.initialAnswer = 'Initial answer';

    app.onCreateValidation();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Deine Eingabe bleibt erhalten',
    );
    const retryButton = fixture.nativeElement.querySelector(
      '.request-error button',
    ) as HTMLButtonElement;
    expect(retryButton.textContent).toContain('Bewertung erneut versuchen');

    retryButton.click();
    fixture.detectChanges();

    expect(validateStep).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });

  it('allows retrying a failed validation details request', () => {
    createValidation.mockReturnValue(of(new Validation({ id: 'validation-id' })));
    validateStep.mockReturnValue(of(new ValidateStepResponse({ isCompleted: true, score: 0.8 })));
    getValidationDetails
      .mockReturnValueOnce(throwError(() => new Error('Details unavailable')))
      .mockReturnValue(of(new ValidationDetails({ steps: [] })));
    app.question = 'Question?';
    app.initialAnswer = 'Initial answer';

    app.onCreateValidation();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Der Verlauf konnte nicht geladen werden',
    );
    const retryButton = fixture.nativeElement.querySelector(
      '.request-error button',
    ) as HTMLButtonElement;
    expect(retryButton.textContent).toContain('Verlauf erneut laden');

    retryButton.click();
    fixture.detectChanges();

    expect(getValidationDetails).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });
});
