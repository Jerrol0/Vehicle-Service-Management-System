import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { CustomerDto } from '../../core/api/generated/types.gen';
import { CustomerForm, CustomerFormValue } from './components/customer-form/customer-form';
import { CustomerService } from './services/customer.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-edit-customer',
  imports: [RouterLink, CustomerForm],
  templateUrl: './edit-customer.html',
  styleUrl: './edit-customer.scss',
})
export class EditCustomer {
  protected readonly customer = signal<CustomerDto | undefined>(undefined);
  protected readonly isLoading = signal(true);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | undefined>(undefined);

  private customerId?: number;
  private rowVersion?: string;

  constructor(
    private readonly customerService: CustomerService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly notificationService: NotificationService,
  ) {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.isLoading.set(false);
      this.errorMessage.set('The customer ID in the URL is invalid.');
      return;
    }

    this.customerId = id;
    this.loadCustomer();
  }

  protected updateCustomer(customer: CustomerFormValue): void {
    this.errorMessage.set(undefined);

    if (this.customerId === undefined || this.rowVersion === undefined) {
      this.errorMessage.set(
        'Customer information is unavailable. Please return to the list and try again.',
      );
      return;
    }

    this.isSubmitting.set(true);

    this.customerService
      .updateCustomer(this.customerId, {
        ...customer,
        rowVersion: this.rowVersion,
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);

          this.notificationService.success('Customer was updated successfully.');

          void this.router.navigate(['/customers']);
        },

        error: (error: unknown) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(this.getSubmitErrorMessage(error));
        },
      });
  }

  protected cancel(): void {
    void this.router.navigate(['/customers']);
  }

  private loadCustomer(): void {
    if (this.customerId === undefined) {
      return;
    }

    this.customerService.getCustomerById(this.customerId).subscribe({
      next: (customer: CustomerDto) => {
        this.customer.set(customer);
        this.rowVersion = customer.rowVersion;
        this.isLoading.set(false);
      },

      error: (error: unknown) => {
        this.errorMessage.set(this.getLoadErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  private getLoadErrorMessage(error: unknown): string {
    const status = this.getErrorStatus(error);

    if (status === 404) {
      return 'This customer could not be found. It may have been archived.';
    }

    if (status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Customer information could not be loaded. Please return to the list and try again.';
  }

  private getSubmitErrorMessage(error: unknown): string {
    const status = this.getErrorStatus(error);
    const backendMessage = this.getBackendMessage(error);

    if (typeof backendMessage === 'string') {
      return backendMessage;
    }

    if (status === 409) {
      return 'This customer was changed by another user. Refresh the page and review the latest information.';
    }

    if (status === 0) {
      return 'Unable to connect to the server. Check that the API is running and try again.';
    }

    return 'Customer information could not be saved. Please try again.';
  }

  private getErrorStatus(error: unknown): number | undefined {
    if (typeof error === 'object' && error !== null) {
      const errorBody = error as {
        status?: unknown;
      };

      return typeof errorBody.status === 'number' ? errorBody.status : undefined;
    }

    return undefined;
  }

  private getBackendMessage(error: unknown): string | undefined {
    if (typeof error === 'object' && error !== null) {
      const errorBody = error as {
        message?: unknown;
        Message?: unknown;
      };

      if (typeof errorBody.message === 'string') {
        return errorBody.message;
      }

      if (typeof errorBody.Message === 'string') {
        return errorBody.Message;
      }
    }

    return undefined;
  }
}
