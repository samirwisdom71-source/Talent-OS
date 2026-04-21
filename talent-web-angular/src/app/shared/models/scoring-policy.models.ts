export interface ScoringPolicyDto {
  id: string;
  name: string;
  version: string;
  performanceWeight: number;
  potentialWeight: number;
  effectiveFromUtc: string;
  notes?: string | null;
  recordStatus: number;
}

export interface ScoringPolicyFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateScoringPolicyRequest {
  name: string;
  version: string;
  performanceWeight: number;
  potentialWeight: number;
  effectiveFromUtc: string;
  notes?: string | null;
}

export interface UpdateScoringPolicyRequest extends CreateScoringPolicyRequest {}

