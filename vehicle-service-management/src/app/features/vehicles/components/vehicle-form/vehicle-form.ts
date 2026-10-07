import { Component, DestroyRef, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { catchError, map, of } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import {
  CreateVehicleDto,
  CustomerDto,
  VehicleDto,
} from '../../../../core/api/generated/types.gen';

import { CustomerService } from '../../../customers/services/customer.service';
import { DebouncedSearchDirective } from './../../../../shared/directives/debounced-search/debounced-search';

const wholeNumberValidator = (): ValidatorFn => {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;

    if (value === null || value === undefined || value === '') {
      return null;
    }

    return Number.isInteger(Number(value)) ? null : { wholeNumber: true };
  };
};

export type VehicleFormValue = CreateVehicleDto;

@Component({
  selector: 'app-vehicle-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DebouncedSearchDirective,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './vehicle-form.html',
  styleUrl: './vehicle-form.scss',
})
export class VehicleForm {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() submitLabel = 'Save Vehicle';
  @Input() isSubmitting = false;

  @Output() formSubmit = new EventEmitter<VehicleFormValue>();
  @Output() cancel = new EventEmitter<void>();

  protected readonly customers = signal<CustomerDto[]>([]);
  protected readonly isLoadingCustomers = signal(false);
  protected readonly customerLoadError = signal(false);

  protected readonly vehicleForm = this.formBuilder.group({
    plateNumber: this.formBuilder.control('', [Validators.required, Validators.maxLength(20)]),
    brand: this.formBuilder.control('', [Validators.required, Validators.maxLength(50)]),
    model: this.formBuilder.control('', [Validators.required, Validators.maxLength(50)]),
    year: this.formBuilder.control<number | null>(null, [
      Validators.required,
      Validators.min(1900),
    ]),
    color: this.formBuilder.control('', [Validators.required, Validators.maxLength(50)]),
    vin: this.formBuilder.control('', [Validators.maxLength(17)]),
    currentMileage: this.formBuilder.control<number | null>(null, [
      Validators.required,
      Validators.min(0),
      wholeNumberValidator(),
    ]),
    customerId: this.formBuilder.control<number | null>(null),
  });

  protected selectedCustomer: CustomerDto | null = null;

  @Input()
  set vehicle(value: VehicleDto | null) {
    if (!value) {
      return;
    }

    this.selectedCustomer = value.customer
      ? {
          id: value.customer.id,
          fullName: value.customer.fullName,
          contactNumber: '',
          email: '',
          address: '',
          createdAt: '',
          updatedAt: '',
          isArchived: false,
          rowVersion: '',
        }
      : null;

    this.vehicleForm.patchValue({
      plateNumber: value.plateNumber ?? '',
      brand: value.brand ?? '',
      model: value.model ?? '',
      year: Number(value.year),
      color: value.color ?? '',
      vin: value.vin ?? '',
      currentMileage: Number(value.currentMileage),
      customerId:
        value.customerId === null || value.customerId === undefined
          ? null
          : Number(value.customerId),
    });

    if (this.selectedCustomer) {
      this.customers.update((customers) => {
        const exists = customers.some(
          (customer) => Number(customer.id) === Number(this.selectedCustomer?.id),
        );

        return exists ? customers : [this.selectedCustomer!, ...customers];
      });
    }
  }

  protected displayCustomer(customer: CustomerDto | null): string {
    return customer?.fullName ?? '';
  }

  protected selectCustomer(customer: CustomerDto): void {
    this.selectedCustomer = customer;

    this.vehicleForm.patchValue({
      customerId: Number(customer.id),
    });
  }

  protected clearCustomer(): void {
    this.selectedCustomer = null;

    this.vehicleForm.patchValue({
      customerId: null,
    });
  }

  protected onCustomerSearch(value: string): void {
    this.loadCustomers(value);
  }

  protected submit(): void {
    if (this.vehicleForm.invalid || this.isSubmitting) {
      this.vehicleForm.markAllAsTouched();
      return;
    }

    const value = this.vehicleForm.getRawValue();

    this.formSubmit.emit({
      plateNumber: value.plateNumber.trim(),
      brand: value.brand.trim(),
      model: value.model.trim(),
      year: value.year!,
      color: value.color.trim(),
      vin: value.vin.trim() || null,
      currentMileage: value.currentMileage!,
      customerId: value.customerId,
    });
  }

  constructor() {
    this.loadCustomers();
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

        if (this.selectedCustomer) {
          this.customers.update((currentCustomers) => {
            const exists = currentCustomers.some(
              (customer) => Number(customer.id) === Number(this.selectedCustomer?.id),
            );

            return exists ? currentCustomers : [this.selectedCustomer!, ...currentCustomers];
          });
        }
      });
  }
}
