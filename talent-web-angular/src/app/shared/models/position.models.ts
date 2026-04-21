export interface PositionDto {
  id: string;
  titleAr: string;
  titleEn: string;
  organizationUnitId: string;
  organizationUnitNameAr?: string | null;
  organizationUnitNameEn?: string | null;
  jobGradeId: string;
  jobGradeName?: string | null;
  jobGradeLevel: number;
}

export interface PositionFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  organizationUnitId?: string | null;
  jobGradeId?: string | null;
}

export interface CreatePositionRequest {
  titleAr: string;
  titleEn: string;
  organizationUnitId: string;
  jobGradeId: string;
}

export interface UpdatePositionRequest {
  titleAr: string;
  titleEn: string;
  organizationUnitId: string;
  jobGradeId: string;
}
