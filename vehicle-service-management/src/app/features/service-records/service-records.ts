import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ArchiveStatus, ServiceRecordDto } from '../../core/api/generated/types.gen';
import { ServiceRecordListQuery } from './models/service-record-list-query';
import { ServiceRecordService } from './services/service-record.service';

import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

import { DebouncedSearchDirective } from '../../shared/directives/debounced-search/debounced-search';
import { ConfirmationDialog } from '../../shared/components/confirmation-dialog/confirmation-dialog';
import { NotificationService } from '../../core/services/notification.service';

import { LucideArchive, LucideArchiveRestore, LucideSquarePen, LucideEye } from '@lucide/angular';

@Component({
  selector: 'app-service-records',
  imports: [
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatSortModule,
    MatTableModule,
    MatTooltipModule,
    MatPaginatorModule,
    DebouncedSearchDirective,

    LucideArchive,
    LucideArchiveRestore,
    LucideSquarePen,
    LucideEye,
  ],
  templateUrl: './service-records.html',
  styleUrl: './service-records.scss',
})
export class ServiceRecords {
  protected readonly serviceRecords = signal<ServiceRecordDto[]>([]);

  protected readonly listQuery = signal<ServiceRecordListQuery>({
    pageNumber: 1,
    pageSize: 10,
    archiveStatus: 0,
  });

  protected readonly totalCount = signal(0);

  protected readonly isLoading = signal(true);

  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly displayedColumns = [
    'serviceTitle',
    'vehicle',
    'customer',
    'serviceType',
    'status',
    'serviceDate',
    'totalCost',
    'actions',
  ];

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

  protected readonly archiveStatuses = [
    { value: 0, label: 'Active' },
    { value: 1, label: 'Archived' },
    { value: 2, label: 'All' },
  ];

  constructor(
    private readonly serviceRecordService: ServiceRecordService,
    private readonly dialog: MatDialog,
    private readonly notificationService: NotificationService,
  ) {
    this.loadServiceRecords();
  }

  protected requestArchive(serviceRecord: ServiceRecordDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Archive ${serviceRecord.serviceTitle}?`,
      'This service record will no longer appear in the active service record list. You can restore archived records later if the business requires it.',
      'Archive Service Record',
      'danger',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.archiveServiceRecord(serviceRecord);
      }
    });
  }

  protected requestUnarchive(serviceRecord: ServiceRecordDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Unarchive ${serviceRecord.serviceTitle}?`,
      'This service record will be restored to the active service record list.',
      'Unarchive Service Record',
      'primary',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.unarchiveServiceRecord(serviceRecord);
      }
    });
  }

  protected archiveServiceRecord(serviceRecord: ServiceRecordDto): void {
    this.serviceRecordService
      .archiveServiceRecord(Number(serviceRecord.id), {
        rowVersion: serviceRecord.rowVersion ?? '',
      })
      .subscribe({
        next: () => {
          this.notificationService.success('Service record archived successfully.');
          this.loadServiceRecords();
        },
        error: (error: unknown) => {
          if (this.isConcurrencyConflict(error)) {
            this.notificationService.warning(
              'The record was modified by another user. Please review the latest vehicle list and try again.',
            );
            this.loadServiceRecords();
            return;
          }

          this.notificationService.error(this.getServiceRecordActionErrorMessage(error, 'archive'));
        },
      });
  }

  protected unarchiveServiceRecord(serviceRecord: ServiceRecordDto): void {
    this.serviceRecordService
      .unarchiveServiceRecord(Number(serviceRecord.id), {
        rowVersion: serviceRecord.rowVersion ?? '',
      })
      .subscribe({
        next: () => {
          this.notificationService.success('Service record unarchived successfully.');
          this.loadServiceRecords();
        },
        error: (error: unknown) => {
          if (this.isConcurrencyConflict(error)) {
            this.notificationService.warning(
              'The record was modified by another user. Please reload the data and try again.',
            );
            this.loadServiceRecords();
            return;
          }

          this.notificationService.error(
            this.getServiceRecordActionErrorMessage(error, 'unarchive'),
          );
        },
      });
  }

  protected onServiceTypeChange(value: number | undefined): void {
    this.listQuery.update((query) => ({
      ...query,
      serviceType: value,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected onStatusChange(value: number | undefined): void {
    this.listQuery.update((query) => ({
      ...query,
      status: value,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected onArchiveStatusChange(value: number): void {
    this.listQuery.update((query) => ({
      ...query,
      archiveStatus: value,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected onStartDateChange(value: string): void {
    this.listQuery.update((query) => ({
      ...query,
      startDate: value || undefined,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected onEndDateChange(value: string): void {
    this.listQuery.update((query) => ({
      ...query,
      endDate: value || undefined,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected onSearchChange(searchTerm: string): void {
    this.listQuery.update((query) => ({
      ...query,
      searchTerm: searchTerm.trim() || undefined,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected onPageChange(event: PageEvent): void {
    this.listQuery.update((query) => ({
      ...query,
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    }));

    this.loadServiceRecords();
  }

  protected onSortChange(sort: Sort): void {
    this.listQuery.update((query) => ({
      ...query,
      sortBy: sort.direction ? sort.active : undefined,
      descending: sort.direction === 'desc' ? true : undefined,
      pageNumber: 1,
    }));

    this.loadServiceRecords();
  }

  protected retryLoad(): void {
    this.loadServiceRecords();
  }

  protected formatVehicle(serviceRecord: ServiceRecordDto): string {
    const vehicle = serviceRecord.vehicle;

    if (!vehicle) {
      return 'Unknown vehicle';
    }

    return `${vehicle.plateNumber} — ${vehicle.brand} ${vehicle.model}`;
  }

  protected formatCustomer(serviceRecord: ServiceRecordDto): string {
    return serviceRecord.vehicle?.customer?.fullName ?? 'Unassigned';
  }

  protected formatCurrency(value: number | string): string {
    return Number(value).toLocaleString('en-PH', {
      style: 'currency',
      currency: 'PHP',
    });
  }

  protected formatDate(value: string): string {
    return new Date(value).toLocaleDateString('en-PH', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  private loadServiceRecords(): void {
    this.isLoading.set(true);
    this.loadError.set(undefined);

    this.serviceRecordService.getServiceRecords(this.listQuery()).subscribe({
      next: (result) => {
        this.serviceRecords.set(result.items ?? []);
        this.totalCount.set(Number(result.totalCount ?? 0));
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.serviceRecords.set([]);
        this.totalCount.set(0);
        this.loadError.set(this.getLoadErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  private getLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Unable to load service records right now. Please try again later.';
  }

  private getServiceRecordActionErrorMessage(
    error: unknown,
    action: 'archive' | 'unarchive',
  ): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    if (action === 'archive') {
      return 'Service record could not be archived. Please try again.';
    }

    return 'Service record could not be unarchived. Please try again.';
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

  private openConfirmationDialog(
    title: string,
    message: string,
    confirmButtonText: string,
    confirmButtonVariant: 'primary' | 'danger' = 'primary',
  ) {
    return this.dialog.open(ConfirmationDialog, {
      width: '24rem',
      maxWidth: 'calc(100vw - 2rem)',
      panelClass: 'confirmation-dialog-panel',
      data: {
        title,
        message,
        confirmButtonText,
        confirmButtonVariant,
      },
    });
  }
}
