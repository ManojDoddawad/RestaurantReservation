export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
}
