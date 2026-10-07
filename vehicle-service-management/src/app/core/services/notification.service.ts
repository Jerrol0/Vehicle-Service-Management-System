import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  NotificationSnackbar,
  NotificationType,
} from '../../shared/components/notification-snackbar/notification-snackbar';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.show('Success', message, 'success', 50000);
  }

  error(message: string): void {
    this.show('Error', message, 'error', 7000);
  }

  warning(message: string): void {
    this.show('Warning', message, 'warning', 6000);
  }

  info(message: string): void {
    this.show('Information', message, 'info', 5000);
  }

  private show(title: string, message: string, type: NotificationType, duration: number): void {
    this.snackBar.openFromComponent(NotificationSnackbar, {
      duration,
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
      panelClass: ['notification-snackbar'],
      data: {
        title,
        message,
        type,
      },
    });
  }
}
