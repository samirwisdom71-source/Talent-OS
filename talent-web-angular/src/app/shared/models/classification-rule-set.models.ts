export interface ClassificationRuleSetDto {
  id: string;
  name: string;
  version: string;
  lowThreshold: number;
  highThreshold: number;
  effectiveFromUtc: string;
  notes?: string | null;
  recordStatus: number;
}

export interface ClassificationRuleSetFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateClassificationRuleSetRequest {
  name: string;
  version: string;
  lowThreshold: number;
  highThreshold: number;
  effectiveFromUtc: string;
  notes?: string | null;
}

export interface UpdateClassificationRuleSetRequest {
  name: string;
  version: string;
  lowThreshold: number;
  highThreshold: number;
  effectiveFromUtc: string;
  notes?: string | null;
}
