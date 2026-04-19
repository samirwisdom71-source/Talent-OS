export interface MarketplaceOpportunityDto {
  id: string;
  title: string;
  description?: string | null;
  opportunityType: number;
  organizationUnitId: string;
  positionId?: string | null;
  requiredCompetencySummary?: string | null;
  status: number;
  openDate: string;
  closeDate?: string | null;
  maxApplicants?: number | null;
  isConfidential: boolean;
  notes?: string | null;
}

export interface MarketplaceOpportunityFilterRequest {
  page?: number;
  pageSize?: number;
  status?: number | null;
  opportunityType?: number | null;
  organizationUnitId?: string | null;
}

export interface CreateMarketplaceOpportunityRequest {
  title: string;
  description?: string | null;
  opportunityType: number;
  organizationUnitId: string;
  positionId?: string | null;
  requiredCompetencySummary?: string | null;
  openDate: string;
  closeDate?: string | null;
  maxApplicants?: number | null;
  isConfidential: boolean;
  notes?: string | null;
}
