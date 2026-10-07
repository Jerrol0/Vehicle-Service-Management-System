import { ArchiveStatus } from '../../../core/api/generated/types.gen';
import { ListQuery } from '../../../shared/models/list-query';

export interface CustomerListQuery extends ListQuery {
  archiveStatus?: ArchiveStatus;
}
