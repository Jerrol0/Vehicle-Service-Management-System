import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { VehicleService } from './services/vehicle.service';
import { VehicleListQuery } from './models/vehicle-list-query';

import { ArchiveStatus, CustomerDto, VehicleDto } from '../../core/api/generated/types.gen';

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

import {
  ReassignVehicleDialog,
  ReassignVehicleDialogData,
} from './components/reassign-vehicle-dialog/reassign-vehicle-dialog';

import {
  LucideArchive,
  LucideArchiveRestore,
  LucideRotateCcwClock,
  LucideSquarePen,
  LucideUserRoundArrowLeft,
} from '@lucide/angular';

@Component({
  selector: 'app-vehicles',
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

    LucideSquarePen,
    LucideRotateCcwClock,
    LucideUserRoundArrowLeft,
    LucideArchive,
    LucideArchiveRestore,
  ],
  templateUrl: './vehicles.html',
  styleUrl: './vehicles.scss',
})
export class Vehicles {
  protected readonly vehicles = signal<VehicleDto[]>([]);

  protected readonly listQuery = signal<VehicleListQuery>({
    archiveStatus: 0,
    pageNumber: 1,
    pageSize: 10,
  });

  protected readonly totalCount = signal(0);

  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly displayedColumns = [
    'plateNumber',
    'vehicle',
    'year',
    'color',
    'currentMileage',
    'customerId',
    'actions',
  ];

  constructor(
    private readonly vehicleService: VehicleService,
    private readonly notificationService: NotificationService,
    private readonly dialog: MatDialog,
  ) {
    this.loadVehicles();
  }

  protected requestReassign(vehicle: VehicleDto): void {
    const dialogRef = this.dialog.open<
      ReassignVehicleDialog,
      ReassignVehicleDialogData,
      CustomerDto
    >(ReassignVehicleDialog, {
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      panelClass: 'confirmation-dialog-panel',
      autoFocus: false,
      data: {
        vehicleName: this.getVehicleDisplayName(vehicle),
        currentCustomerName: vehicle.customer?.fullName ?? undefined,
      },
    });

    dialogRef.afterClosed().subscribe((customer) => {
      if (customer) {
        this.confirmReassign(vehicle, customer);
      }
    });
  }

  private confirmReassign(vehicle: VehicleDto, customer: CustomerDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Reassign ${vehicle.plateNumber ?? 'this vehicle'}?`,
      `This vehicle will be reassigned to ${customer.fullName}.`,
      'Reassign Vehicle',
      'primary',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.reassignVehicle(vehicle, customer);
      }
    });
  }

  private reassignVehicle(vehicle: VehicleDto, customer: CustomerDto): void {
    this.vehicleService
      .reassignVehicle(Number(vehicle.id), {
        customerId: Number(customer.id),
      })
      .subscribe({
        next: () => {
          this.loadVehicles();

          this.notificationService.success(
            `${vehicle.plateNumber ?? 'Vehicle'} was reassigned to ${customer.fullName} successfully.`,
          );
        },
        error: (error) => {
          this.notificationService.error(this.getReassignErrorMessage(error));
        },
      });
  }

  protected requestArchive(vehicle: VehicleDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Archive ${vehicle.plateNumber ?? 'this vehicle'}?`,
      'This vehicle will no longer appear in the active vehicle list. You can restore archived records later if the business requires it.',
      'Archive Vehicle',
      'danger',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.archiveVehicle(vehicle);
      }
    });
  }

  protected requestUnarchive(vehicle: VehicleDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Unarchive ${vehicle.plateNumber ?? 'this vehicle'}?`,
      'This vehicle will be restored to the active vehicle list.',
      'Unarchive Vehicle',
      'primary',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.unarchiveVehicle(vehicle);
      }
    });
  }

  protected archiveVehicle(vehicle: VehicleDto): void {
    this.vehicleService
      .archiveVehicle(Number(vehicle.id), {
        rowVersion: vehicle.rowVersion ?? '',
      })
      .subscribe({
        next: () => {
          this.vehicles.update((vehicles) => vehicles.filter((item) => item.id !== vehicle.id));

          this.notificationService.success(
            `${vehicle.plateNumber ?? 'Vehicle'} was archived successfully.`,
          );
        },
        error: (error) => {
          if (this.isConcurrencyConflict(error)) {
            this.loadVehicles();

            this.notificationService.warning(
              'This vehicle was modified by another user. Please review the latest vehicle list and try again.',
            );

            return;
          }

          this.notificationService.error(this.getVehicleActionErrorMessage(error, 'archive'));
        },
      });
  }

  protected unarchiveVehicle(vehicle: VehicleDto): void {
    this.vehicleService
      .unarchiveVehicle(Number(vehicle.id), {
        rowVersion: vehicle.rowVersion ?? '',
      })
      .subscribe({
        next: () => {
          this.vehicles.update((vehicles) => vehicles.filter((item) => item.id !== vehicle.id));

          this.notificationService.success(
            `${vehicle.plateNumber ?? 'Vehicle'} was unarchived successfully.`,
          );
        },
        error: (error) => {
          if (this.isConcurrencyConflict(error)) {
            this.loadVehicles();

            this.notificationService.warning(
              'This vehicle was modified by another user. Please review the latest vehicle list and try again.',
            );

            return;
          }

          this.notificationService.error(this.getVehicleActionErrorMessage(error, 'unarchive'));
        },
      });
  }

  protected onArchiveStatusChange(value: number): void {
    this.listQuery.update((query) => ({
      ...query,
      archiveStatus: value as ArchiveStatus,
      pageNumber: 1,
    }));

    this.loadVehicles();
  }

  protected onSearchChange(value: string): void {
    this.listQuery.update((query) => ({
      ...query,
      searchTerm: value,
      pageNumber: 1,
    }));

    this.loadVehicles();
  }

  protected onPageChange(event: PageEvent): void {
    this.listQuery.update((query) => ({
      ...query,
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    }));

    this.loadVehicles();
  }

  protected onSortChange(sort: Sort): void {
    this.listQuery.update((query) => ({
      ...query,
      sortBy: sort.direction ? sort.active : undefined,
      descending: sort.direction === 'desc' ? true : undefined,
      pageNumber: 1,
    }));

    this.loadVehicles();
  }

  protected formatVehicle(vehicle: VehicleDto): string {
    return [vehicle.brand, vehicle.model].filter((value): value is string => !!value).join(' ');
  }

  protected formatMileage(mileage: number): string {
    return `${mileage.toLocaleString()} km`;
  }

  private loadVehicles(): void {
    this.isLoading.set(true);
    this.loadError.set(undefined);

    this.vehicleService.getVehicles(this.listQuery()).subscribe({
      next: (result) => {
        this.vehicles.set(result.items ?? []);
        this.totalCount.set(Number(result.totalCount));
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(this.getLoadErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  private getLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Unable to load vehicles right now. Please try again later.';
  }

  private getVehicleActionErrorMessage(error: unknown, action: 'archive' | 'unarchive'): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    if (action === 'archive') {
      return 'Vehicle could not be archived. Please try again.';
    }

    return 'Vehicle could not be unarchived. Please try again.';
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

  private getVehicleDisplayName(vehicle: VehicleDto): string {
    const vehicleName = this.formatVehicle(vehicle);

    if (vehicle.plateNumber && vehicleName) {
      return `${vehicleName} — ${vehicle.plateNumber}`;
    }

    return vehicle.plateNumber || vehicleName || 'Vehicle';
  }

  private getReassignErrorMessage(error: unknown): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    return 'Vehicle could not be reassigned. Please try again.';
  }
}
