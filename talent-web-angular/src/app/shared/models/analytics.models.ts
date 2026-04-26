export interface NineBoxDistributionItemDto {
  nineBoxCode: number;
  count: number;
}

export interface EnumCountDto {
  value: number;
  count: number;
}

export interface NamedCountDto {
  name: string;
  count: number;
}

export interface ExecutiveCycleSnapshotDto {
  performanceCycleId: string;
  performanceCycleNameEn: string;
  performanceCycleNameAr?: string;
  finalizedEvaluations: number;
}

export interface ExecutiveCodeCountDto {
  code: number;
  count: number;
}

export interface ExecutiveDashboardSummaryDto {
  totalEmployees: number;
  totalTalentScores: number;
  totalTalentClassifications: number;
  highPotentialCount: number;
  highPerformerCount: number;
  strategicLeaderCount: number;
  activePerformanceCycleCount: number;
  activeSuccessionPlanCount: number;
  openMarketplaceOpportunityCount: number;
  activeDevelopmentPlanCount: number;
  /** Extended executive payload (API v2). Optional for older servers. */
  totalPerformanceGoals?: number;
  completedPerformanceGoals?: number;
  finalizedEvaluationCount?: number;
  pendingApprovalCount?: number;
  talentInsightCount?: number;
  talentRecommendationCount?: number;
  totalMarketplaceApplicationCount?: number;
  nineBoxDistribution?: readonly NineBoxDistributionItemDto[];
  byPerformanceBand?: readonly EnumCountDto[];
  byPotentialBand?: readonly EnumCountDto[];
  topTalentCategories?: readonly NamedCountDto[];
  performanceByCycle?: readonly ExecutiveCycleSnapshotDto[];
  developmentItemsByType?: readonly ExecutiveCodeCountDto[];
  marketplaceOpportunitiesByType?: readonly ExecutiveCodeCountDto[];
  successorReadiness?: readonly EnumCountDto[];
}
