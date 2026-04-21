export interface TalentScoreDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  performanceScore: number;
  potentialScore: number;
  finalScore: number;
  performanceWeight: number;
  potentialWeight: number;
  calculationVersion: string;
  calculatedOnUtc: string;
  notes?: string | null;
}

export interface TalentScoreFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  minFinalScore?: number | null;
  maxFinalScore?: number | null;
}

export interface CalculateTalentScoreRequest {
  employeeId: string;
  performanceCycleId: string;
  notes?: string | null;
}

export interface RecalculateTalentScoreRequest {
  employeeId: string;
  performanceCycleId: string;
  notes?: string | null;
}
