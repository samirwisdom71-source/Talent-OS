export interface DevelopmentPlanItemDto {
  id: string;
  developmentPlanId: string;
  title: string;
  description?: string | null;
  itemType: number;
  relatedCompetencyId?: string | null;
  targetDate?: string | null;
  status: number;
  progressPercentage: number;
  notes?: string | null;
}

export interface DevelopmentPlanItemFilterRequest {
  page?: number;
  pageSize?: number;
  developmentPlanId: string;
}

export interface UpdateDevelopmentPlanItemProgressRequest {
  progressPercentage: number;
}
