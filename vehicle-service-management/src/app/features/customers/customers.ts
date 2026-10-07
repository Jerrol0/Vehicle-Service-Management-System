import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CustomerService } from './services/customer.service';
import { CustomerListQuery } from './models/customer-list-query';

import { ArchiveStatus, CustomerDto } from '../../core/api/generated/types.gen';

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

import { LucideArchive, LucideArchiveRestore, LucideSquarePen } from '@lucide/angular';

@Component({
  selector: 'app-customers',
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
  ],
  templateUrl: './customers.html',
  styleUrl: './customers.scss',
})
export class Customers {
  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly listQuery = signal<CustomerListQuery>({
    archiveStatus: 0,
    pageNumber: 1,
    pageSize: 10,
  });

  protected readonly totalCount = signal(0);

  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | undefined>(undefined);

  protected readonly displayedColumns = [
    'fullName',
    'contactNumber',
    'email',
    'address',
    'actions',
  ];

  constructor(
    private readonly customerService: CustomerService,
    private readonly notificationService: NotificationService,
    private readonly dialog: MatDialog,
  ) {
    this.loadCustomers();
  }

  protected requestArchive(customer: CustomerDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Archive ${customer.fullName}?`,
      'This customer will no longer appear in the active customer list. You can restore archived records later if the business requires it.',
      'Archive Customer',
      'danger',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.archiveCustomer(customer);
      }
    });
  }

  protected requestUnarchive(customer: CustomerDto): void {
    const dialogRef = this.openConfirmationDialog(
      `Unarchive ${customer.fullName}?`,
      'This customer will be restored to the active customer list.',
      'Unarchive Customer',
      'primary',
    );

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.unarchiveCustomer(customer);
      }
    });
  }

  protected archiveCustomer(customer: CustomerDto): void {
    this.customerService
      .archiveCustomer(Number(customer.id), {
        rowVersion: customer.rowVersion ?? '',
      })
      .subscribe({
        next: () => {
          this.customers.update((customers) => customers.filter((item) => item.id !== customer.id));

          this.notificationService.success(`${customer.fullName} was archived successfully.`);
        },
        error: (error) => {
          if (this.isConcurrencyConflict(error)) {
            this.loadCustomers();

            this.notificationService.warning(
              'This customer was modified by another user. Please review the latest customer list and try again.',
            );

            return;
          }

          this.notificationService.error(this.getCustomerActionErrorMessage(error, 'archive'));
        },
      });
  }

  protected unarchiveCustomer(customer: CustomerDto): void {
    this.customerService
      .unarchiveCustomer(Number(customer.id), {
        rowVersion: customer.rowVersion ?? '',
      })
      .subscribe({
        next: () => {
          this.customers.update((customers) => customers.filter((item) => item.id !== customer.id));

          this.notificationService.success(`${customer.fullName} was unarchived successfully.`);
        },
        error: (error) => {
          if (this.isConcurrencyConflict(error)) {
            this.loadCustomers();

            this.notificationService.warning(
              'This customer was modified by another user. Please review the latest customer list and try again.',
            );

            return;
          }

          this.notificationService.error(this.getCustomerActionErrorMessage(error, 'unarchive'));
        },
      });
  }

  protected onArchiveStatusChange(value: number): void {
    this.listQuery.update((query) => ({
      ...query,
      archiveStatus: value as ArchiveStatus,
      pageNumber: 1,
    }));

    this.loadCustomers();
  }

  protected onSearchChange(value: string): void {
    this.listQuery.update((query) => ({
      ...query,
      searchTerm: value,
      pageNumber: 1,
    }));

    this.loadCustomers();
  }

  protected onPageChange(event: PageEvent): void {
    this.listQuery.update((query) => ({
      ...query,
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    }));

    this.loadCustomers();
  }

  protected onSortChange(sort: Sort): void {
    this.listQuery.update((query) => ({
      ...query,
      sortBy: sort.direction ? sort.active : undefined,
      descending: sort.direction === 'desc' ? true : undefined,
      pageNumber: 1,
    }));

    this.loadCustomers();
  }

  private loadCustomers(): void {
    this.isLoading.set(true);
    this.loadError.set(undefined);

    this.customerService.getCustomers(this.listQuery()).subscribe({
      next: (result) => {
        this.customers.set(result.items ?? []);
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

    return 'Unable to load customers right now. Please try again later.';
  }

  private getCustomerActionErrorMessage(error: unknown, action: 'archive' | 'unarchive'): string {
    const backendMessage = this.getBackendErrorMessage(error);

    if (backendMessage) {
      return backendMessage;
    }

    if (action === 'archive') {
      return 'Customer could not be archived. Please try again.';
    }

    return 'Customer could not be unarchived. Please try again.';
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
