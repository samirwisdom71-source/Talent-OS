export interface CompetencyDto {
  id: string;
  code: string;
  nameAr: string;
  nameEn: string;
  description?: string | null;
  competencyCategoryId: string;
}

export interface CompetencyFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateCompetencyRequest {
  code: string;
  nameAr: string;
  nameEn: string;
  description?: string | null;
  competencyCategoryId: string;
}

export interface UpdateCompetencyRequest {
  code: string;
  nameAr: string;
  nameEn: string;
  description?: string | null;
  competencyCategoryId: string;
}
