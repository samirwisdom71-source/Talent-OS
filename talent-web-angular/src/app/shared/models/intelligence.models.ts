export interface TalentInsightDto {
  id: string;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  insightType: number;
  severity: number;
  source: number;
  title: string;
  summary: string;
  confidenceScore: number;
  relatedEntityId?: string | null;
  relatedEntityType?: string | null;
  status: number;
  generatedOnUtc: string;
  notes?: string | null;
}

export interface TalentRecommendationDto {
  id: string;
  employeeId: string;
  performanceCycleId?: string | null;
  recommendationType: number;
  priority: number;
  source: number;
  title: string;
  description: string;
  recommendedAction: string;
  confidenceScore: number;
  relatedEntityId?: string | null;
  relatedEntityType?: string | null;
  status: number;
  generatedOnUtc: string;
  notes?: string | null;
}

export interface TalentInsightFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  status?: number | null;
}

export interface TalentRecommendationFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  status?: number | null;
}

/** Backend `EmployeeIntelligenceGenerationTarget`: Insights=1, Recommendations=2, All=3 */
export type EmployeeIntelligenceGenerationTarget = 1 | 2 | 3;

export interface GenerateEmployeeIntelligenceRequest {
  employeeId: string;
  performanceCycleId: string;
  target?: EmployeeIntelligenceGenerationTarget;
}

export interface GenerateCycleIntelligenceRequest {
  performanceCycleId: string;
}

export interface IntelligenceGenerationResultDto {
  runId?: string | null;
  insightsGenerated: number;
  recommendationsGenerated: number;
}
