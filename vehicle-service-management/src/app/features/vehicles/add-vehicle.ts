import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { VehicleForm, VehicleFormValue } from './components/vehicle-form/vehicle-form';
import { VehicleService } from './services/vehicle.service';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-add-vehicle',
  standalone: true,
  imports: [RouterLink, VehicleForm, MatIconModule, MatButtonModule],
  templateUrl: './add-vehicle.html',
  styleUrl: './add-vehicle.scss',
})
export class AddVehicle {
  private readonly vehicleService = inject(VehicleService);
  private readonly router = inject(Router);
  private readonly notificationService = inject(NotificationService);

  protected readonly isSubmitting = signal(false);

  protected createVehicle(vehicle: VehicleFormValue): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);

    this.vehicleService.createVehicle(vehicle).subscribe({
      next: () => {
        this.isSubmitting.set(false);

        this.notificationService.success('Vehicle was created successfully.');

        void this.router.navigate(['/vehicles']);
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);

        this.notificationService.error(this.getCreateErrorMessage(error));
      },
    });
  }

  protected cancel(): void {
    this.router.navigate(['/vehicles']);
  }

  private getCreateErrorMessage(error: unknown): string {
    if (typeof error === 'object' && error !== null && 'Message' in error) {
      const message = (error as { Message?: unknown }).Message;

      if (typeof message === 'string') {
        return message;
      }
    }

    return 'Vehicle could not be created. Please try again.';
  }
}
