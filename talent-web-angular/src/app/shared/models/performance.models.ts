export interface PerformanceCycleDto {
  id: string;
  nameAr: string;
  nameEn: string;
  startDate: string;
  endDate: string;
  status: number;
  description?: string | null;
}

export interface PerformanceCycleFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  status?: number | null;
}

export interface CreatePerformanceCycleRequest {
  nameAr: string;
  nameEn: string;
  startDate: string;
  endDate: string;
  description?: string | null;
}

export interface UpdatePerformanceCycleRequest {
  nameAr: string;
  nameEn: string;
  startDate: string;
  endDate: string;
  description?: string | null;
}

export interface PerformanceEvaluationDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  overallScore: number;
  managerComments?: string | null;
  employeeComments?: string | null;
  status: number;
  evaluatedOnUtc?: string | null;
}

export interface PerformanceEvaluationFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
}

export interface CreatePerformanceEvaluationRequest {
  employeeId: string;
  performanceCycleId: string;
  overallScore: number;
  managerComments?: string | null;
  employeeComments?: string | null;
  status?: number;
}

export interface UpdatePerformanceEvaluationRequest {
  overallScore: number;
  managerComments?: string | null;
  employeeComments?: string | null;
  status: number;
}

export interface PerformanceGoalDto {
  id: string;
  employeeId: string;
  performanceCycleId: string;
  titleAr: string;
  titleEn: string;
  description?: string | null;
  weight: number;
  targetValue?: string | null;
  status: number;
}

export interface PerformanceGoalFilterRequest {
  page?: number;
  pageSize?: number;
  employeeId?: string | null;
  performanceCycleId?: string | null;
  search?: string | null;
}

export interface CreatePerformanceGoalRequest {
  employeeId: string;
  performanceCycleId: string;
  titleAr: string;
  titleEn: string;
  description?: string | null;
  weight: number;
  targetValue?: string | null;
  status?: number;
}

export interface UpdatePerformanceGoalRequest {
  titleAr: string;
  titleEn: string;
  description?: string | null;
  weight: number;
  targetValue?: string | null;
  status: number;
}
