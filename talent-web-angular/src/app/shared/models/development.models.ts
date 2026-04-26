export interface DevelopmentPlanItemPathHelperDto {
  id: string;
  developmentPlanItemPathId: string;
  helperKind: number;
  helperEntityId: string;
}

export interface DevelopmentPlanItemPathDto {
  id: string;
  developmentPlanItemId: string;
  sortOrder: number;
  title: string;
  description?: string | null;
  plannedStartUtc?: string | null;
  plannedEndUtc?: string | null;
  status: number;
  /** قيمة الأثر المحقق عند إكمال المسار (من الخادم؛ يعاد توزيعها عند تغيّر عدد المسارات) */
  achievedImpactValue?: number | null;
  helpers?: readonly DevelopmentPlanItemPathHelperDto[] | null;
}

export interface DevelopmentPlanItemPathHelperInputDto {
  helperKind: number;
  helperEntityId: string;
}

export interface CreateDevelopmentPlanItemPathRequest {
  sortOrder: number;
  title: string;
  description?: string | null;
  plannedStartUtc?: string | null;
  plannedEndUtc?: string | null;
  helpers?: readonly DevelopmentPlanItemPathHelperInputDto[] | null;
}

export interface UpdateDevelopmentPlanItemPathRequest {
  sortOrder: number;
  title: string;
  description?: string | null;
  plannedStartUtc?: string | null;
  plannedEndUtc?: string | null;
  status: number;
  helpers?: readonly DevelopmentPlanItemPathHelperInputDto[] | null;
}

export interface DevelopmentPlanStructuredItemInputDto {
  title: string;
  description?: string | null;
  itemType: number;
  relatedCompetencyId?: string | null;
  targetDate?: string | null;
  paths?: readonly CreateDevelopmentPlanItemPathRequest[] | null;
}

export interface DevelopmentPlanLinkInputDto {
  linkType: number;
  linkedEntityId: string;
  notes?: string | null;
}

export interface DevelopmentPlanImpactSnapshotDto {
  id: string;
  developmentPlanId: string;
  phase: number;
  recordedOnUtc: string;
  summaryNotes?: string | null;
  metricScore?: number | null;
}

export interface SuggestDevelopmentPlanRequest {
  employeeId: string;
  performanceCycleId: string;
  sourceType: number;
}

export interface DevelopmentPlanSuggestionDto {
  planTitle: string;
  notes?: string | null;
  links: readonly DevelopmentPlanLinkInputDto[];
  items: readonly DevelopmentPlanStructuredItemInputDto[];
}

export interface DevelopmentPlanDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  planTitle: string;
  sourceType: number;
  /** يُرجع من الـ API للخطط المقترحة آلياً */
  isSystemSuggested?: boolean;
  status: number;
  targetCompletionDate?: string | null;
  notes?: string | null;
  approvedByEmployeeId?: string | null;
  approvedOnUtc?: string | null;
  links?: readonly DevelopmentPlanLinkDto[] | null;
  impactSnapshots?: readonly DevelopmentPlanImpactSnapshotDto[] | null;
}

export interface DevelopmentPlanLinkDto {
  id: string;
  developmentPlanId: string;
  linkType: number;
  linkedEntityId: string;
  notes?: string | null;
}

export interface DevelopmentPlanFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
}

export interface CreateDevelopmentPlanRequest {
  employeeId: string;
  performanceCycleId: string;
  planTitle: string;
  sourceType: number;
  /** مقترح من النظام */
  isSystemSuggested?: boolean;
  targetCompletionDate?: string | null;
  notes?: string | null;
  links?: readonly DevelopmentPlanLinkInputDto[] | null;
  structuredItems?: readonly DevelopmentPlanStructuredItemInputDto[] | null;
}

export interface UpdateDevelopmentPlanRequest {
  planTitle: string;
  sourceType: number;
  targetCompletionDate?: string | null;
  notes?: string | null;
}
