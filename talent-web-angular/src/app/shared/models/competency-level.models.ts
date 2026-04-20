export interface CompetencyLevelDto {
  id: string;
  name: string;
  numericValue: number;
  description?: string | null;
}

export interface CompetencyLevelFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateCompetencyLevelRequest {
  name: string;
  numericValue: number;
  description?: string | null;
}

export interface UpdateCompetencyLevelRequest {
  name: string;
  numericValue: number;
  description?: string | null;
}
