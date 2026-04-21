export interface JobCompetencyRequirementDto {
  id: string;
  positionId: string;
  competencyId: string;
  requiredLevelId: string;
}

export interface JobCompetencyRequirementFilterRequest {
  page?: number;
  pageSize?: number;
  positionId?: string | null;
  competencyId?: string | null;
}

export interface CreateJobCompetencyRequirementRequest {
  positionId: string;
  competencyId: string;
  requiredLevelId: string;
}

export interface UpdateJobCompetencyRequirementRequest {
  positionId: string;
  competencyId: string;
  requiredLevelId: string;
}
