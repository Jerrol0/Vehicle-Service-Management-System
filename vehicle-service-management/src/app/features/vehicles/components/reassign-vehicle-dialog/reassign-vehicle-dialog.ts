import { Component, DestroyRef, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { catchError, map, of } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CustomerDto } from '../../../../core/api/generated/types.gen';
import { CustomerService } from '../../../customers/services/customer.service';
import { DebouncedSearchDirective } from '../../../../shared/directives/debounced-search/debounced-search';

export interface ReassignVehicleDialogData {
  vehicleName: string;
  currentCustomerName?: string;
}

@Component({
  selector: 'app-reassign-vehicle-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    DebouncedSearchDirective,
  ],
  templateUrl: './reassign-vehicle-dialog.html',
  styleUrl: './reassign-vehicle-dialog.scss',
})
export class ReassignVehicleDialog {
  private readonly dialogRef = inject(MatDialogRef<ReassignVehicleDialog>);
  private readonly customerService = inject(CustomerService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly data = inject<ReassignVehicleDialogData>(MAT_DIALOG_DATA);

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoadingCustomers = signal(false);
  protected readonly customerLoadError = signal(false);

  protected selectedCustomer: CustomerDto | null = null;

  constructor() {
    this.loadCustomers();
  }

  protected selectCustomer(customer: CustomerDto): void {
    this.selectedCustomer = customer;
  }

  protected onCustomerSearch(value: string): void {
    this.selectedCustomer = null;
    this.loadCustomers(value);
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  protected continue(): void {
    if (!this.selectedCustomer) {
      return;
    }

    this.dialogRef.close(this.selectedCustomer);
  }

  private loadCustomers(searchTerm?: string): void {
    this.isLoadingCustomers.set(true);
    this.customerLoadError.set(false);

    this.customerService
      .getCustomers({
        searchTerm: searchTerm?.trim() || undefined,
        archiveStatus: 0,
        pageNumber: 1,
        pageSize: 20,
      })
      .pipe(
        map((result) => result.items ?? []),
        catchError(() => {
          this.customerLoadError.set(true);
          return of([]);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((customers) => {
        this.customers.set(customers);
        this.isLoadingCustomers.set(false);
      });
  }
}
