import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { LucideArrowLeft, LucideSquarePen } from '@lucide/angular';

import { ServiceRecordDto } from '../../core/api/generated/types.gen';
import { ServiceRecordService } from './services/service-record.service';

@Component({
  selector: 'app-service-record-view',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatDividerModule, LucideArrowLeft, LucideSquarePen],
  templateUrl: './view-service-record.html',
  styleUrl: './view-service-record.scss',
})
export class ViewServiceRecord {
  protected readonly serviceRecord = signal<ServiceRecordDto | undefined>(undefined);

  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly returnToVehicleHistoryId: number | undefined;

  private readonly serviceRecordId: number;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly serviceRecordService: ServiceRecordService,
  ) {
    this.serviceRecordId = Number(this.route.snapshot.paramMap.get('id'));

    const from = this.route.snapshot.queryParamMap.get('from');
    const vehicleId = Number(this.route.snapshot.queryParamMap.get('vehicleId'));

    this.returnToVehicleHistoryId =
      from === 'vehicle-history' && Number.isInteger(vehicleId) && vehicleId > 0
        ? vehicleId
        : undefined;

    this.loadServiceRecord();
  }

  protected loadServiceRecord(): void {
    this.isLoading.set(true);
    this.loadError.set(undefined);

    this.serviceRecordService.getServiceRecordById(this.serviceRecordId).subscribe({
      next: (serviceRecord) => {
        this.serviceRecord.set(serviceRecord);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.serviceRecord.set(undefined);
        this.loadError.set(this.getLoadErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  protected getLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    if (error.status === 404) {
      return 'The service record could not be found.';
    }

    return 'Unable to load the service record right now. Please try again later.';
  }

  protected getVehicleDisplayName(serviceRecord: ServiceRecordDto): string {
    const vehicle = serviceRecord.vehicle;

    if (!vehicle) {
      return 'Vehicle information unavailable';
    }

    return `${vehicle.plateNumber} - ${vehicle.brand} ${vehicle.model}`;
  }

  protected getCustomerName(serviceRecord: ServiceRecordDto): string {
    return serviceRecord.vehicle?.customer?.fullName ?? 'Unassigned';
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
