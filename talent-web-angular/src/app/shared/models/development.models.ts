export interface DevelopmentPlanDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  planTitle: string;
  sourceType: number;
  status: number;
  targetCompletionDate?: string | null;
  notes?: string | null;
  approvedByEmployeeId?: string | null;
  approvedOnUtc?: string | null;
  links?: readonly DevelopmentPlanLinkDto[] | null;
}

export interface DevelopmentPlanLinkDto {
  id: string;
  developmentPlanId: string;
  linkType: number;
  linkedEntityId: string;
  notes?: string | null;
}

export interface DevelopmentPlanFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
}

export interface CreateDevelopmentPlanRequest {
  employeeId: string;
  performanceCycleId: string;
  planTitle: string;
  sourceType: number;
  targetCompletionDate?: string | null;
  notes?: string | null;
}

export interface UpdateDevelopmentPlanRequest {
  planTitle: string;
  sourceType: number;
  targetCompletionDate?: string | null;
  notes?: string | null;
}
