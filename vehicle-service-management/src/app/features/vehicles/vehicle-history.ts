import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { LucideArrowLeft } from '@lucide/angular';

import { ServiceRecordDto, VehicleDto } from '../../core/api/generated/types.gen';
import { VehicleService } from './services/vehicle.service';

@Component({
  selector: 'app-vehicle-history',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatDividerModule, LucideArrowLeft],
  templateUrl: './vehicle-history.html',
  styleUrl: './vehicle-history.scss',
})
export class VehicleHistory {
  protected readonly vehicle = signal<VehicleDto | undefined>(undefined);
  protected readonly serviceRecords = signal<ServiceRecordDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly vehicleId: number;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly vehicleService: VehicleService,
  ) {
    this.vehicleId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadVehicleHistory();
  }

  protected loadVehicleHistory(): void {
    this.isLoading.set(true);
    this.loadError.set(undefined);

    this.vehicleService.getVehicleById(this.vehicleId).subscribe({
      next: (vehicle) => {
        this.vehicle.set(vehicle);
        this.loadServiceRecords();
      },
      error: (error: HttpErrorResponse) => {
        this.vehicle.set(undefined);
        this.serviceRecords.set([]);
        this.loadError.set(this.getVehicleLoadErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  protected loadServiceRecords(): void {
    this.vehicleService.getVehicleServices(this.vehicleId).subscribe({
      next: (serviceRecords) => {
        this.serviceRecords.set(serviceRecords);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.serviceRecords.set([]);
        this.loadError.set(this.getServiceHistoryErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  protected getVehicleDisplayName(vehicle: VehicleDto): string {
    return `${vehicle.plateNumber} - ${vehicle.brand} ${vehicle.model}`;
  }

  protected getVehicleLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    if (error.status === 404) {
      return 'The vehicle could not be found.';
    }

    return 'Unable to load the vehicle right now. Please try again later.';
  }

  protected getServiceHistoryErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    if (error.status === 404) {
      return 'The service history for this vehicle could not be found.';
    }

    return 'Unable to load the vehicle service history right now. Please try again later.';
  }

  protected formatStatus(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Pending';
      case 'InProgress':
        return 'In Progress';
      case 'Completed':
        return 'Completed';
      case 'Cancelled':
        return 'Cancelled';
      default:
        return 'Unknown';
    }
  }

  protected formatServiceType(serviceType: string): string {
    switch (serviceType) {
      case 'Maintenance':
        return 'Maintenance';
      case 'Repair':
        return 'Repair';
      case 'Inspection':
        return 'Inspection';
      case 'Replacement':
        return 'Replacement';
      case 'Diagnostic':
        return 'Diagnostic';
      default:
        return 'Unknown';
    }
  }

  protected formatDate(date: string): string {
    return new Intl.DateTimeFormat('en-PH', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }).format(new Date(date));
  }

  protected formatCurrency(value: number | string): string {
    return new Intl.NumberFormat('en-PH', {
      style: 'currency',
      currency: 'PHP',
    }).format(Number(value));
  }

  protected formatMileage(value: number | string): string {
    return `${Number(value).toLocaleString('en-PH')} km`;
  }
}
