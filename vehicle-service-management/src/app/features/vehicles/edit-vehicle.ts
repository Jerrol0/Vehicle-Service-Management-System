import { Component, DestroyRef, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { VehicleDto } from '../../core/api/generated/types.gen';

import { VehicleForm, VehicleFormValue } from './components/vehicle-form/vehicle-form';
import { VehicleService } from './services/vehicle.service';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-edit-vehicle',
  standalone: true,
  imports: [RouterLink, VehicleForm, MatIconModule, MatButtonModule],
  templateUrl: './edit-vehicle.html',
  styleUrl: './edit-vehicle.scss',
})
export class EditVehicle {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly vehicleService = inject(VehicleService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationService = inject(NotificationService);

  protected readonly vehicle = signal<VehicleDto | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | undefined>(undefined);

  private vehicleId!: number;
  private rowVersion!: string;

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.errorMessage.set('Invalid vehicle ID.');
      this.isLoading.set(false);
      return;
    }

    this.vehicleId = id;
    this.loadVehicle();
  }

  protected updateVehicle(vehicle: VehicleFormValue): void {
    if (this.isSubmitting() || !this.vehicle()) {
      return;
    }

    this.isSubmitting.set(true);

    this.vehicleService
      .updateVehicle(Number(this.vehicle()!.id), {
        ...vehicle,
        rowVersion: this.rowVersion,
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);

          this.notificationService.success('Vehicle was updated successfully.');

          void this.router.navigate(['/vehicles']);
        },
        error: (error: unknown) => {
          this.isSubmitting.set(false);

          if (this.isConcurrencyConflict(error)) {
            this.notificationService.warning(
              'This vehicle was modified by another user. Please reload the vehicle and try again.',
            );

            return;
          }

          this.notificationService.error(this.getUpdateErrorMessage(error));
        },
      });
  }

  protected cancel(): void {
    this.router.navigate(['/vehicles']);
  }

  private loadVehicle(): void {
    this.vehicleService
      .getVehicleById(this.vehicleId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (vehicle) => {
          this.vehicle.set(vehicle);
          this.rowVersion = vehicle.rowVersion ?? '';
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Unable to load the vehicle right now. Please try again later.');
          this.isLoading.set(false);
        },
      });
  }

  private getUpdateErrorMessage(error: unknown): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    return 'Vehicle could not be updated. Please try again.';
  }

  private getBackendErrorMessage(error: unknown): string | undefined {
    if (typeof error === 'object' && error !== null && 'Message' in error) {
      const message = (error as { Message?: unknown }).Message;

      if (typeof message === 'string') {
        return message;
      }
    }

    return undefined;
  }

  private isConcurrencyConflict(error: unknown): boolean {
    return (
      this.getBackendErrorMessage(error) ===
      'The record was modified by another user. Please reload the data and try again.'
    );
  }
}
