export interface OpportunityApplicationDto {
  id: string;
  marketplaceOpportunityId: string;
  employeeId: string;
  employeeDisplayName?: string | null;
  marketplaceOpportunityTitle?: string | null;
  applicationStatus: number;
  motivationStatement?: string | null;
  appliedOnUtc: string;
  reviewedOnUtc?: string | null;
  notes?: string | null;
}

export interface ApplyOpportunityRequest {
  marketplaceOpportunityId: string;
  employeeId: string;
  motivationStatement?: string | null;
}

export interface UpdateOpportunityApplicationRequest {
  motivationStatement?: string | null;
  notes?: string | null;
}

export interface OpportunityApplicationFilterRequest {
  page?: number;
  pageSize?: number;
  marketplaceOpportunityId?: string | null;
  employeeId?: string | null;
}
