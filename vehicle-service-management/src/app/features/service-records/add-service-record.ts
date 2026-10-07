import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LucideArrowLeft, LucideSave } from '@lucide/angular';

import {
  CreateServiceRecordDto,
  ServiceType,
  VehicleDto,
} from '../../core/api/generated/types.gen';
import { NotificationService } from '../../core/services/notification.service';
import { DebouncedSearchDirective } from '../../shared/directives/debounced-search/debounced-search';
import { VehicleService } from '../vehicles/services/vehicle.service';
import { ServiceRecordService } from './services/service-record.service';

@Component({
  selector: 'app-add-service-record',
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DebouncedSearchDirective,

    LucideArrowLeft,
    LucideSave,
  ],
  templateUrl: './add-service-record.html',
  styleUrl: './add-service-record.scss',
})
export class AddServiceRecord {
  private readonly formBuilder = inject(FormBuilder);

  protected readonly vehicleResults = signal<VehicleDto[]>([]);
  protected readonly selectedVehicle = signal<VehicleDto | undefined>(undefined);

  protected readonly isSearchingVehicles = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly vehicleSearchError = signal<string | undefined>(undefined);

  protected readonly serviceTypes = [
    { value: 1, label: 'Maintenance' },
    { value: 2, label: 'Repair' },
    { value: 3, label: 'Inspection' },
    { value: 4, label: 'Replacement' },
    { value: 5, label: 'Diagnostic' },
  ];

  protected readonly form = this.formBuilder.nonNullable.group({
    vehicleId: [0, Validators.min(1)],
    serviceTitle: ['', [Validators.required, Validators.maxLength(100)]],
    serviceType: [1 as ServiceType, Validators.required],
    description: ['', [Validators.required, Validators.maxLength(500)]],
    serviceDate: ['', Validators.required],
    mileageAtService: [0],
    laborCost: [0],
    partsCost: [0],
  });

  constructor(
    private readonly router: Router,
    private readonly vehicleService: VehicleService,
    private readonly serviceRecordService: ServiceRecordService,
    private readonly notificationService: NotificationService,
  ) {}

  protected searchVehicles(searchTerm: string): void {
    const term = searchTerm.trim();

    this.selectedVehicle.set(undefined);
    this.form.controls.vehicleId.setValue(0);
    this.vehicleSearchError.set(undefined);
    this.isSearchingVehicles.set(true);

    this.vehicleService
      .getVehicles({
        searchTerm: term || undefined,
        pageNumber: 1,
        pageSize: 10,
        archiveStatus: 0,
      })
      .subscribe({
        next: (result) => {
          this.vehicleResults.set(result.items ?? []);
          this.isSearchingVehicles.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.vehicleResults.set([]);
          this.vehicleSearchError.set(this.getVehicleSearchErrorMessage(error));
          this.isSearchingVehicles.set(false);
        },
      });
  }

  protected openVehicleSearch(): void {
    if (this.vehicleResults().length > 0 || this.isSearchingVehicles()) {
      return;
    }

    this.searchVehicles('');
  }

  protected readonly displayVehicleOption = (vehicle: VehicleDto | null): string => {
    if (!vehicle) {
      return '';
    }

    return `${vehicle.plateNumber} — ${this.getVehicleDisplayName(vehicle)}`;
  };

  protected selectVehicle(vehicle: VehicleDto): void {
    this.selectedVehicle.set(vehicle);
    this.form.controls.vehicleId.setValue(Number(vehicle.id));
    this.form.controls.mileageAtService.setValue(Number(vehicle.currentMileage));
  }

  protected clearVehicleSelection(): void {
    this.selectedVehicle.set(undefined);
    this.form.controls.vehicleId.setValue(0);
    this.form.controls.mileageAtService.setValue(0);
  }

  protected save(): void {
    if (this.form.invalid || !this.selectedVehicle()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();

    const serviceRecord: CreateServiceRecordDto = {
      vehicleId: value.vehicleId,
      serviceTitle: value.serviceTitle,
      serviceType: value.serviceType,
      description: value.description,
      serviceDate: value.serviceDate,
      mileageAtService: value.mileageAtService || undefined,
      laborCost: value.laborCost || undefined,
      partsCost: value.partsCost || undefined,
    };

    this.isSaving.set(true);

    this.serviceRecordService.createServiceRecord(serviceRecord).subscribe({
      next: (createdServiceRecord) => {
        this.notificationService.success('Service record created successfully.');

        this.router.navigate(['/service-records', createdServiceRecord.id]);
      },
      error: (error: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.notificationService.error(this.getCreateErrorMessage(error));
      },
    });
  }

  protected getVehicleDisplayName(vehicle: VehicleDto): string {
    return `${vehicle.brand} ${vehicle.model}`;
  }

  protected getCustomerName(vehicle: VehicleDto): string {
    return vehicle.customer?.fullName ?? 'Unassigned';
  }

  private getVehicleSearchErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Unable to search vehicles right now. Please try again.';
  }

  private getCreateErrorMessage(error: HttpErrorResponse): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Service record could not be created. Please try again.';
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
}
