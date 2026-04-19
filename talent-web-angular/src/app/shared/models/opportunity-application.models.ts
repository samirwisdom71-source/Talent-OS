export interface OpportunityApplicationDto {
  id: string;
  marketplaceOpportunityId: string;
  employeeId: string;
  applicationStatus: number;
  motivationStatement?: string | null;
  appliedOnUtc: string;
  reviewedOnUtc?: string | null;
  notes?: string | null;
}

export interface OpportunityApplicationFilterRequest {
  page?: number;
  pageSize?: number;
  marketplaceOpportunityId?: string | null;
  employeeId?: string | null;
}
