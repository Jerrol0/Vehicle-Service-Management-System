import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { CreateCustomerDto, CustomerDto } from '../../../../core/api/generated/types.gen';

export type CustomerFormValue = CreateCustomerDto;

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './customer-form.html',
  styleUrl: './customer-form.scss',
})
export class CustomerForm implements OnChanges {
  private readonly formBuilder = inject(NonNullableFormBuilder);

  @Input() customer: CustomerDto | undefined;
  @Input() submitLabel = 'Save Customer';
  @Input() isSubmitting = false;

  @Output() formSubmit = new EventEmitter<CustomerFormValue>();
  @Output() cancel = new EventEmitter<void>();

  protected readonly customerForm = this.formBuilder.group({
    fullName: this.formBuilder.control('', [Validators.required, Validators.maxLength(100)]),
    contactNumber: this.formBuilder.control('', [Validators.required, Validators.maxLength(20)]),
    email: this.formBuilder.control('', [
      Validators.required,
      Validators.email,
      Validators.maxLength(100),
    ]),
    address: this.formBuilder.control('', [Validators.required, Validators.maxLength(200)]),
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['customer'] && this.customer) {
      this.customerForm.reset({
        fullName: this.customer.fullName,
        contactNumber: this.customer.contactNumber,
        email: this.customer.email,
        address: this.customer.address,
      });
    }
  }

  protected submit(): void {
    if (this.customerForm.invalid || this.isSubmitting) {
      this.customerForm.markAllAsTouched();
      return;
    }

    const value = this.customerForm.getRawValue();

    this.formSubmit.emit({
      fullName: value.fullName.trim(),
      contactNumber: value.contactNumber.trim(),
      email: value.email.trim(),
      address: value.address.trim(),
    });
  }

  public setEmailDuplicateError(message: string): void {
    const emailControl = this.customerForm.controls.email;

    emailControl.setErrors({
      ...emailControl.errors,
      duplicate: message,
    });

    emailControl.markAsTouched();
  }
}
