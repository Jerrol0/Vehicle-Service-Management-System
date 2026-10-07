import { Component, ViewChild, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';

import { CustomerForm, CustomerFormValue } from './components/customer-form/customer-form';
import { CustomerService } from './services/customer.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-add-customer',
  imports: [RouterLink, MatButtonModule, CustomerForm],
  templateUrl: './add-customer.html',
  styleUrl: './add-customer.scss',
})
export class AddCustomer {
  @ViewChild(CustomerForm)
  private readonly customerForm?: CustomerForm;

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | undefined>(undefined);

  constructor(
    private readonly customerService: CustomerService,
    private readonly router: Router,
    private readonly notificationService: NotificationService,
  ) {}

  protected createCustomer(customer: CustomerFormValue): void {
    this.errorMessage.set(undefined);
    this.isSubmitting.set(true);

    this.customerService.createCustomer(customer).subscribe({
      next: () => {
        this.isSubmitting.set(false);

        this.notificationService.success('Customer was created successfully.');

        void this.router.navigate(['/customers']);
      },

      error: (error: unknown) => {
        this.isSubmitting.set(false);

        const message = this.getErrorMessage(error);

        if (message === 'A customer with this email already exists.') {
          this.customerForm?.setEmailDuplicateError(message);
          return;
        }

        this.errorMessage.set(message);
      },
    });
  }

  protected cancel(): void {
    void this.router.navigate(['/customers']);
  }

  private getErrorMessage(error: unknown): string {
    if (typeof error === 'object' && error !== null) {
      const errorBody = error as {
        Message?: unknown;
        message?: unknown;
        errors?: Record<string, string[]>;
      };

      if (typeof errorBody.Message === 'string') {
        return errorBody.Message;
      }

      if (typeof errorBody.message === 'string') {
        return errorBody.message;
      }

      if (errorBody.errors) {
        const firstValidationMessage = Object.values(errorBody.errors).flat().at(0);

        if (firstValidationMessage) {
          return firstValidationMessage;
        }
      }
    }

    return 'Customer could not be created. Please review the form and try again.';
  }
}
