export interface PotentialAssessmentDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  assessedByEmployeeId: string;
  agilityScore: number;
  leadershipScore: number;
  growthScore: number;
  mobilityScore: number;
  overallPotentialScore: number;
  potentialLevel: number;
  comments?: string | null;
  status: number;
  assessedOnUtc?: string | null;
  factors: readonly PotentialAssessmentFactorDto[];
}

export interface PotentialAssessmentFactorDto {
  id: string;
  factorName: string;
  score: number;
  weight: number;
  notes?: string | null;
}

export interface PotentialAssessmentFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  potentialLevel?: number | null;
  status?: number | null;
}

export interface PotentialAssessmentFactorItemDto {
  factorName: string;
  score: number;
  weight: number;
  notes?: string | null;
}

export interface CreatePotentialAssessmentRequest {
  employeeId: string;
  performanceCycleId: string;
  assessedByEmployeeId: string;
  agilityScore: number;
  leadershipScore: number;
  growthScore: number;
  mobilityScore: number;
  comments?: string | null;
  status: number;
  factors: PotentialAssessmentFactorItemDto[];
}

export interface UpdatePotentialAssessmentRequest {
  assessedByEmployeeId: string;
  agilityScore: number;
  leadershipScore: number;
  growthScore: number;
  mobilityScore: number;
  comments?: string | null;
  status: number;
  factors: PotentialAssessmentFactorItemDto[];
}
