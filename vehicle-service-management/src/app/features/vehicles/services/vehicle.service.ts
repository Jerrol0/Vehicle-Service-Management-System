import { Injectable } from '@angular/core';
import { from, Observable } from 'rxjs';

import {
  getApiVehicles,
  getApiVehiclesById,
  postApiVehicles,
  putApiVehiclesById,
  patchApiVehiclesByIdArchive,
  patchApiVehiclesByIdUnarchive,
  putApiVehiclesByIdReassign,
  getApiVehiclesByIdServices,
} from '../../../core/api/generated/sdk.gen';

import {
  ArchiveRequestDto,
  CreateVehicleDto,
  PagedResultDtoOfVehicleDto,
  ReassignVehicleDto,
  ServiceRecordDto,
  UpdateVehicleDto,
  VehicleDto,
} from '../../../core/api/generated/types.gen';

import { VehicleListQuery } from '../models/vehicle-list-query';

@Injectable({
  providedIn: 'root',
})
export class VehicleService {
  getVehicles(
    query: VehicleListQuery = {
      pageNumber: 1,
      pageSize: 10,
      archiveStatus: 0,
    },
  ): Observable<PagedResultDtoOfVehicleDto> {
    return from(
      getApiVehicles({
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

  getVehicleById(id: number): Observable<VehicleDto> {
    return from(
      getApiVehiclesById({
        path: { id },
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  createVehicle(vehicle: CreateVehicleDto): Observable<VehicleDto> {
    return from(
      postApiVehicles({
        body: vehicle,
        throwOnError: true,
      }).then((result) => result.data),
    );
  }

  updateVehicle(id: number, vehicle: UpdateVehicleDto): Observable<void> {
    return from(
      putApiVehiclesById({
        path: { id },
        body: vehicle,
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  archiveVehicle(id: number, request: ArchiveRequestDto): Observable<void> {
    return from(
      patchApiVehiclesByIdArchive({
        path: { id },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  unarchiveVehicle(id: number, request: ArchiveRequestDto): Observable<void> {
    return from(
      patchApiVehiclesByIdUnarchive({
        path: { id },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  reassignVehicle(id: number, request: ReassignVehicleDto): Observable<void> {
    return from(
      putApiVehiclesByIdReassign({
        path: { id },
        body: request,
        responseStyle: 'data',
        throwOnError: true,
      }).then(() => undefined),
    );
  }

  getVehicleServices(id: number): Observable<ServiceRecordDto[]> {
    return from(
      getApiVehiclesByIdServices({
        path: { id },
        throwOnError: true,
      }).then((result) => result.data),
    );
  }
}
