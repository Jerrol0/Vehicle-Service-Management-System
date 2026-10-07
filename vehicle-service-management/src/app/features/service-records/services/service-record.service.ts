import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import {
  getApiServiceRecords,
  getApiServiceRecordsById,
  patchApiServiceRecordsByIdArchive,
  patchApiServiceRecordsByIdUnarchive,
  postApiServiceRecords,
  putApiServiceRecordsById,
} from '../../../core/api/generated/sdk.gen';

import {
  ArchiveRequestDto,
  CreateServiceRecordDto,
  PagedResultDtoOfServiceRecordDto,
  ServiceRecordDto,
  UpdateServiceRecordDto,
} from '../../../core/api/generated/types.gen';

import { ServiceRecordListQuery } from '../models/service-record-list-query';

@Injectable({
  providedIn: 'root',
})
export class ServiceRecordService {
  getServiceRecords(
    query: ServiceRecordListQuery = {
      pageNumber: 1,
      pageSize: 10,
      archiveStatus: 0,
    },
  ): Observable<PagedResultDtoOfServiceRecordDto> {
    return from(
      getApiServiceRecords({
        query: {
          searchTerm: query.searchTerm,
          vehicleId: query.vehicleId,
          serviceType: query.serviceType,
          status: query.status,
          startDate: query.startDate,
          endDate: query.endDate,
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

  getServiceRecordById(id: number): Observable<ServiceRecordDto> {
    return from(
      getApiServiceRecordsById({
        path: { id },
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  createServiceRecord(serviceRecord: CreateServiceRecordDto): Observable<ServiceRecordDto> {
    return from(
      postApiServiceRecords({
        body: serviceRecord,
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  updateServiceRecord(id: number, serviceRecord: UpdateServiceRecordDto): Observable<void> {
    return from(
      putApiServiceRecordsById({
        path: { id },
        body: serviceRecord,
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  archiveServiceRecord(id: number, request: ArchiveRequestDto): Observable<void> {
    return from(
      patchApiServiceRecordsByIdArchive({
        path: { id },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  unarchiveServiceRecord(id: number, request: ArchiveRequestDto): Observable<void> {
    return from(
      patchApiServiceRecordsByIdUnarchive({
        path: { id },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }
}
