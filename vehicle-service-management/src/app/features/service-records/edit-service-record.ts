import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LucideArrowLeft, LucideSave } from '@lucide/angular';

import { ServiceRecordDto } from '../../core/api/generated/types.gen';
import { NotificationService } from '../../core/services/notification.service';
import { ServiceRecordService } from './services/service-record.service';

@Component({
  selector: 'app-edit-service-record',
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,

    LucideArrowLeft,
    LucideSave,
  ],
  templateUrl: './edit-service-record.html',
  styleUrl: './edit-service-record.scss',
})
export class EditServiceRecord {
  private readonly formBuilder = inject(FormBuilder);
  protected readonly serviceRecord = signal<ServiceRecordDto | undefined>(undefined);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly serviceTypes = [
    { value: 1, label: 'Maintenance' },
    { value: 2, label: 'Repair' },
    { value: 3, label: 'Inspection' },
    { value: 4, label: 'Replacement' },
    { value: 5, label: 'Diagnostic' },
  ];

  protected readonly serviceStatuses = [
    { value: 1, label: 'Pending' },
    { value: 2, label: 'In Progress' },
    { value: 3, label: 'Completed' },
    { value: 4, label: 'Cancelled' },
  ];

  protected readonly form = this.formBuilder.nonNullable.group({
    serviceTitle: ['', [Validators.required, Validators.maxLength(100)]],
    serviceType: [1, Validators.required],
    description: ['', [Validators.required]],
    serviceDate: ['', Validators.required],
    mileageAtService: [0],
    laborCost: [0],
    partsCost: [0],
    status: [1, Validators.required],
  });

  private readonly serviceRecordId: number;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly serviceRecordService: ServiceRecordService,
    private readonly notificationService: NotificationService,
  ) {
    this.serviceRecordId = Number(this.route.snapshot.paramMap.get('id'));

    this.loadServiceRecord();
  }

  protected loadServiceRecord(): void {
    this.isLoading.set(true);
    this.loadError.set(undefined);

    this.serviceRecordService.getServiceRecordById(this.serviceRecordId).subscribe({
      next: (serviceRecord) => {
        this.serviceRecord.set(serviceRecord);
        this.populateForm(serviceRecord);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.serviceRecord.set(undefined);
        this.loadError.set(this.getLoadErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  protected save(): void {
    if (this.form.invalid || !this.serviceRecord()) {
      this.form.markAllAsTouched();
      return;
    }

    const serviceRecord = this.serviceRecord();

    if (!serviceRecord) {
      return;
    }

    this.isSaving.set(true);

    this.serviceRecordService
      .updateServiceRecord(this.serviceRecordId, {
        serviceTitle: this.form.controls.serviceTitle.value,
        serviceType: this.form.controls.serviceType.value,
        description: this.form.controls.description.value,
        serviceDate: this.form.controls.serviceDate.value,
        mileageAtService: this.form.controls.mileageAtService.value || undefined,
        laborCost: this.form.controls.laborCost.value || undefined,
        partsCost: this.form.controls.partsCost.value || undefined,
        status: this.form.controls.status.value,
        rowVersion: serviceRecord.rowVersion,
      })
      .subscribe({
        next: () => {
          this.notificationService.success('Service record updated successfully.');

          this.router.navigate(['/service-records', this.serviceRecordId]);
        },
        error: (error: HttpErrorResponse) => {
          this.isSaving.set(false);

          if (this.isConcurrencyConflict(error)) {
            this.notificationService.warning(
              'The record was modified by another user. Please reload the data and try again.',
            );
            this.loadServiceRecord();
            return;
          }

          this.notificationService.error(this.getUpdateErrorMessage(error));
        },
      });
  }

  protected getVehicleDisplayName(serviceRecord: ServiceRecordDto): string {
    const vehicle = serviceRecord.vehicle;

    if (!vehicle) {
      return 'Vehicle information unavailable';
    }

    return `${vehicle.plateNumber} — ${vehicle.brand} ${vehicle.model}`;
  }

  protected getCustomerName(serviceRecord: ServiceRecordDto): string {
    return serviceRecord.vehicle?.customer?.fullName ?? 'Unassigned';
  }

  private populateForm(serviceRecord: ServiceRecordDto): void {
    this.form.patchValue({
      serviceTitle: serviceRecord.serviceTitle,
      serviceType: this.getServiceTypeValue(serviceRecord.serviceType),
      description: serviceRecord.description,
      serviceDate: serviceRecord.serviceDate.slice(0, 10),
      mileageAtService: Number(serviceRecord.mileageAtService),
      laborCost: Number(serviceRecord.laborCost),
      partsCost: Number(serviceRecord.partsCost),
      status: this.getServiceStatusValue(serviceRecord.status),
    });
  }

  private getServiceTypeValue(serviceType: string): number {
    switch (serviceType) {
      case 'Maintenance':
        return 1;
      case 'Repair':
        return 2;
      case 'Inspection':
        return 3;
      case 'Replacement':
        return 4;
      case 'Diagnostic':
        return 5;
      default:
        return 1;
    }
  }

  private getServiceStatusValue(status: string): number {
    switch (status) {
      case 'Pending':
        return 1;
      case 'InProgress':
        return 2;
      case 'Completed':
        return 3;
      case 'Cancelled':
        return 4;
      default:
        return 1;
    }
  }

  private getLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    if (error.status === 404) {
      return 'The service record could not be found.';
    }

    return 'Unable to load the service record right now. Please try again later.';
  }

  private getUpdateErrorMessage(error: HttpErrorResponse): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Service record could not be updated. Please try again.';
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
