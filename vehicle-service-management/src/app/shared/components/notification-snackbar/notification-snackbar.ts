import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface NotificationSnackbarData {
  title: string;
  message: string;
  type: NotificationType;
}

@Component({
  selector: 'app-notification-snackbar',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  templateUrl: './notification-snackbar.html',
  styleUrl: './notification-snackbar.scss',
})
export class NotificationSnackbar {
  private readonly snackBarRef = inject(MatSnackBarRef<NotificationSnackbar>);

  protected readonly data = inject<NotificationSnackbarData>(MAT_SNACK_BAR_DATA);

  protected get icon(): string {
    switch (this.data.type) {
      case 'success':
        return 'check_circle';

      case 'error':
        return 'error';

      case 'warning':
        return 'warning';

      case 'info':
        return 'info';
    }
  }

  protected close(): void {
    this.snackBarRef.dismiss();
  }
}
