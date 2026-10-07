import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import {
  getApiCustomers,
  getApiCustomersById,
  postApiCustomers,
  putApiCustomersById,
  patchApiCustomersByIdArchive,
  patchApiCustomersByIdUnarchive,
} from '../../../core/api/generated/sdk.gen';

import {
  ArchiveRequestDto,
  CustomerDto,
  CreateCustomerDto,
  PagedResultDtoOfCustomerDto,
  UpdateCustomerDto,
} from '../../../core/api/generated/types.gen';

import { CustomerListQuery } from '../models/customer-list-query';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  getCustomers(
    query: CustomerListQuery = {
      pageNumber: 1,
      pageSize: 10,
      archiveStatus: 0,
    },
  ): Observable<PagedResultDtoOfCustomerDto> {
    return from(
      getApiCustomers({
        query: {
          searchTerm: query.searchTerm,
          archiveStatus: query.archiveStatus ?? 0,

          ...(query.sortBy
            ? {
                SortBy: query.sortBy,
                Descending: query.descending,
              }
            : {}),

          PageNumber: query.pageNumber,
          PageSize: query.pageSize,
        },
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  getArchivedCustomers(): Observable<PagedResultDtoOfCustomerDto> {
    return this.getCustomers({
      archiveStatus: 1,
      pageNumber: 1,
      pageSize: 10,
    });
  }

  getCustomerById(id: number): Observable<CustomerDto> {
    return from(
      getApiCustomersById({
        path: {
          id,
        },
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  createCustomer(customer: CreateCustomerDto): Observable<CustomerDto> {
    return from(
      postApiCustomers({
        body: customer,
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  updateCustomer(id: number, customer: UpdateCustomerDto): Observable<void> {
    return from(
      putApiCustomersById({
        path: {
          id,
        },
        body: customer,
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  archiveCustomer(id: number, request: ArchiveRequestDto): Observable<void> {
    return from(
      patchApiCustomersByIdArchive({
        path: {
          id,
        },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  unarchiveCustomer(id: number, request: ArchiveRequestDto): Observable<void> {
    return from(
      patchApiCustomersByIdUnarchive({
        path: {
          id,
        },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }
}
