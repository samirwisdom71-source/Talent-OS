export interface PerformanceCycleDto {
  id: string;
  nameAr: string;
  nameEn: string;
  startDate: string;
  endDate: string;
  status: number;
  description?: string | null;
}

export interface PerformanceCycleFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  status?: number | null;
}
