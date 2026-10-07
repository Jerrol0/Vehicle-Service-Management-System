import { ArchiveStatus, ServiceStatus, ServiceType } from '../../../core/api/generated/types.gen';

import { ListQuery } from '../../../shared/models/list-query';

export interface ServiceRecordListQuery extends ListQuery {
  vehicleId?: number;
  serviceType?: ServiceType;
  status?: ServiceStatus;
  startDate?: string;
  endDate?: string;
  archiveStatus?: ArchiveStatus;
}
