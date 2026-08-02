import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../auth.service';
import { AuthDialogComponent } from '../auth-dialog/auth-dialog.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-account-menu',
  imports: [MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule, MatTooltipModule],
  template: `
    <button
      mat-icon-button
      type="button"
      [matMenuTriggerFor]="menu"
      [matTooltip]="authService.isAuthenticated() ? 'Account verwalten' : 'Anmelden'"
      aria-label="Account-Menü öffnen"
    >
      <mat-icon>account_circle</mat-icon>
    </button>

    <mat-menu #menu="matMenu" xPosition="before">
      @if (authService.currentUser(); as user) {
        <div class="account-info">
          <p class="account-name">{{ user.displayName }}</p>
          <p class="account-email">{{ user.email }}</p>
        </div>
        <mat-divider />
        <button mat-menu-item type="button" (click)="onLogout()">
          <mat-icon>logout</mat-icon>
          <span>Logout</span>
        </button>
        <button mat-menu-item type="button" (click)="onDeleteAccount()">
          <mat-icon color="warn">delete_forever</mat-icon>
          <span>Account löschen</span>
        </button>
      } @else {
        <button mat-menu-item type="button" (click)="onOpenAuthDialog()">
          <mat-icon>login</mat-icon>
          <span>Anmelden / Registrieren</span>
        </button>
      }
    </mat-menu>
  `,
  styles: `
    .account-info {
      padding: 0.5rem 1rem;
    }

    .account-name {
      margin: 0;
      font-weight: 500;
    }

    .account-email {
      margin: 0;
      font-size: 0.8rem;
      opacity: 0.7;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountMenuComponent {
  protected readonly authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);

  onOpenAuthDialog(): void {
    this.authService.clearError();
    this.dialog.open(AuthDialogComponent, { width: '420px' });
  }

  onLogout(): void {
    this.authService.logout();
  }

  onDeleteAccount(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Account löschen',
        message:
          'Möchtest du deinen Account wirklich löschen? Alle deine Validierungen werden unwiderruflich entfernt.',
        confirmLabel: 'Account löschen',
        cancelLabel: 'Abbrechen'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.authService.deleteAccount(() => {});
      }
    });
  }
}
