export interface ListQuery {
  searchTerm?: string;
  sortBy?: string;
  descending?: boolean;
  pageNumber: number;
  pageSize: number;
}
