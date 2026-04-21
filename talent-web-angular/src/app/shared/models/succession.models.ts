export interface SuccessionPlanDto {
  id: string;
  criticalPositionId: string;
  performanceCycleId: string;
  planName: string;
  status: number;
  notes?: string | null;
  coverageSnapshot?: SuccessionCoverageSnapshotDto | null;
}

export interface SuccessionCoverageSnapshotDto {
  id: string;
  successionPlanId: string;
  totalCandidates: number;
  hasReadyNow: boolean;
  hasPrimarySuccessor: boolean;
  coverageScore: number;
  calculatedOnUtc: string;
}

export interface SuccessionPlanFilterRequest {
  page?: number;
  pageSize?: number;
  criticalPositionId?: string | null;
  performanceCycleId?: string | null;
}

export interface CriticalPositionDto {
  id: string;
  positionId: string;
  criticalityLevel: number;
  riskLevel: number;
  notes?: string | null;
  recordStatus: number;
}

export interface CriticalPositionFilterRequest {
  page?: number;
  pageSize?: number;
  positionId?: string | null;
  activeOnly?: boolean | null;
}

export interface CreateSuccessionPlanRequest {
  criticalPositionId: string;
  performanceCycleId: string;
  planName: string;
  notes?: string | null;
}

/** PUT /api/succession-plans/{id} */
export interface UpdateSuccessionPlanRequest {
  planName: string;
  notes?: string | null;
}

/** POST /api/critical-positions */
export interface CreateCriticalPositionRequest {
  positionId: string;
  criticalityLevel: number;
  riskLevel: number;
  notes?: string | null;
}

/** PUT /api/critical-positions/{id} */
export interface UpdateCriticalPositionRequest {
  criticalityLevel: number;
  riskLevel: number;
  notes?: string | null;
}
