import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';
import { vi, type Mock } from 'vitest';
import { AuthService } from '../auth.service';
import { User } from '../api/business-service-client';
import { AccountMenuComponent } from './account-menu.component';

describe('AccountMenuComponent', () => {
  let fixture: ComponentFixture<AccountMenuComponent>;
  let component: AccountMenuComponent;
  let authService: {
    currentUser: Mock<() => User | undefined>;
    isAuthenticated: Mock<() => boolean>;
    clearError: Mock<() => void>;
    logout: Mock<() => void>;
    deleteAccount: Mock<(onSuccess: () => void) => void>;
  };
  let dialogOpen: Mock<MatDialog['open']>;

  const user = new User({ id: 'user-id', email: 'user@example.com', displayName: 'Test User' });

  beforeEach(async () => {
    authService = {
      currentUser: vi.fn().mockReturnValue(undefined),
      isAuthenticated: vi.fn().mockReturnValue(false),
      clearError: vi.fn(),
      logout: vi.fn(),
      deleteAccount: vi.fn(),
    };
    dialogOpen = vi.fn();

    await TestBed.configureTestingModule({
      imports: [AccountMenuComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: MatDialog, useValue: { open: dialogOpen } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders a login menu entry when the user is not authenticated', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('[aria-label="Account-Menü öffnen"]')).toBeTruthy();
  });

  it('opens the auth dialog and clears a previous error when not authenticated', () => {
    component.onOpenAuthDialog();

    expect(authService.clearError).toHaveBeenCalledTimes(1);
    expect(dialogOpen).toHaveBeenCalledTimes(1);
  });

  it('logs out via the auth service', () => {
    component.onLogout();

    expect(authService.logout).toHaveBeenCalledTimes(1);
  });

  it('deletes the account when the confirmation dialog is confirmed', () => {
    dialogOpen.mockReturnValue({ afterClosed: () => of(true) } as unknown as MatDialogRef<unknown, boolean>);

    component.onDeleteAccount();

    expect(dialogOpen).toHaveBeenCalledTimes(1);
    expect(authService.deleteAccount).toHaveBeenCalledTimes(1);
  });

  it('does not delete the account when the confirmation dialog is cancelled', () => {
    dialogOpen.mockReturnValue({ afterClosed: () => of(false) } as unknown as MatDialogRef<unknown, boolean>);

    component.onDeleteAccount();

    expect(authService.deleteAccount).not.toHaveBeenCalled();
  });

  it('shows the authenticated user and account actions', () => {
    authService.currentUser.mockReturnValue(user);
    authService.isAuthenticated.mockReturnValue(true);
    fixture.detectChanges();

    expect(authService.currentUser()).toEqual(user);
    expect(authService.isAuthenticated()).toBe(true);
  });
});
