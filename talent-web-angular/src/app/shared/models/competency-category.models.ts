export interface CompetencyCategoryDto {
  id: string;
  nameAr: string;
  nameEn: string;
  description?: string | null;
}

export interface CompetencyCategoryFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateCompetencyCategoryRequest {
  nameAr: string;
  nameEn: string;
  description?: string | null;
}

export interface UpdateCompetencyCategoryRequest {
  nameAr: string;
  nameEn: string;
  description?: string | null;
}
