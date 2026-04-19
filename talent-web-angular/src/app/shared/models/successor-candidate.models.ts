export interface SuccessorCandidateDto {
  id: string;
  successionPlanId: string;
  employeeId: string;
  readinessLevel: number;
  rankOrder: number;
  isPrimarySuccessor: boolean;
  notes?: string | null;
}

export interface SuccessorCandidateFilterRequest {
  page?: number;
  pageSize?: number;
  successionPlanId: string;
}

export interface CreateSuccessorCandidateRequest {
  successionPlanId: string;
  employeeId: string;
  readinessLevel: number;
  rankOrder: number;
  isPrimarySuccessor: boolean;
  notes?: string | null;
}

export interface UpdateSuccessorCandidateRequest {
  readinessLevel: number;
  rankOrder: number;
  isPrimarySuccessor: boolean;
  notes?: string | null;
}
