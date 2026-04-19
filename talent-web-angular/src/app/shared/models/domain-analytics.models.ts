export interface EnumCountDto {
  value: number;
  count: number;
}

export interface NamedCountDto {
  name: string;
  count: number;
}

export interface NineBoxDistributionItemDto {
  nineBoxCode: number;
  count: number;
}

export interface TalentDistributionSummaryDto {
  byNineBox: readonly NineBoxDistributionItemDto[];
  byPerformanceBand: readonly EnumCountDto[];
  byPotentialBand: readonly EnumCountDto[];
  byCategoryName: readonly NamedCountDto[];
}

export interface TalentAnalyticsFilterRequest {
  performanceCycleId?: string | null;
  organizationUnitId?: string | null;
}

export interface TalentClassificationByCycleSummaryDto {
  performanceCycleId: string;
  performanceCycleNameEn: string;
  classificationCount: number;
}

export interface SuccessionAnalyticsSummaryDto {
  totalCriticalPositions: number;
  activeCriticalPositions: number;
  totalSuccessionPlans: number;
  activeSuccessionPlans: number;
  plansWithReadyNowSuccessor: number;
  plansWithPrimarySuccessor: number;
  averageCoverageScore?: number | null;
  successorReadinessBreakdown: readonly EnumCountDto[];
}

export interface DevelopmentItemTypeBreakdownDto {
  itemType: number;
  itemCount: number;
  completedCount: number;
  inProgressCount: number;
}

export interface DevelopmentAnalyticsSummaryDto {
  totalDevelopmentPlans: number;
  activeDevelopmentPlans: number;
  completedDevelopmentPlans: number;
  cancelledDevelopmentPlans: number;
  totalDevelopmentPlanItems: number;
  completedDevelopmentPlanItems: number;
  inProgressDevelopmentPlanItems: number;
  averageProgressPercentageActiveItems?: number | null;
  itemsByType: readonly DevelopmentItemTypeBreakdownDto[];
}

export interface OpportunityTypeBreakdownDto {
  opportunityType: number;
  opportunityCount: number;
  openCount: number;
}

export interface MarketplaceAnalyticsSummaryDto {
  totalMarketplaceOpportunities: number;
  draftOpportunities: number;
  openOpportunities: number;
  closedOpportunities: number;
  cancelledOpportunities: number;
  archivedOpportunities: number;
  totalApplications: number;
  submittedApplications: number;
  underReviewApplications: number;
  shortlistedApplications: number;
  acceptedApplications: number;
  rejectedApplications: number;
  withdrawnApplications: number;
  averageMatchScore?: number | null;
  opportunitiesByType: readonly OpportunityTypeBreakdownDto[];
}

export interface PerformanceCycleAnalyticsBreakdownDto {
  performanceCycleId: string;
  performanceCycleNameEn: string;
  totalGoals: number;
  completedGoals: number;
  totalEvaluations: number;
  finalizedEvaluations: number;
  averageFinalizedOverallScore?: number | null;
}

export interface PerformanceAnalyticsSummaryDto {
  totalPerformanceCycles: number;
  activePerformanceCycles: number;
  totalGoals: number;
  completedGoals: number;
  totalEvaluations: number;
  finalizedEvaluations: number;
  averageOverallEvaluationScoreFinalized?: number | null;
  breakdownByCycle: readonly PerformanceCycleAnalyticsBreakdownDto[];
}
