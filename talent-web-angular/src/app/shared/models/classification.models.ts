export interface TalentClassificationDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  talentScoreId: string;
  performanceBand: number;
  potentialBand: number;
  nineBoxCode: number;
  categoryName: string;
  isHighPotential: boolean;
  isHighPerformer: boolean;
  classifiedOnUtc: string;
  notes?: string | null;
}

export interface TalentClassificationFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  nineBoxCode?: number | null;
  isHighPotential?: boolean | null;
  isHighPerformer?: boolean | null;
  performanceBand?: number | null;
  potentialBand?: number | null;
}

export interface ClassifyTalentClassificationRequest {
  employeeId: string;
  performanceCycleId: string;
  notes?: string | null;
}

export interface ReclassifyTalentClassificationRequest {
  employeeId: string;
  performanceCycleId: string;
  notes?: string | null;
}
