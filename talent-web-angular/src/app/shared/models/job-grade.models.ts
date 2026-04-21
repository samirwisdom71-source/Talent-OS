export interface JobGradeDto {
  id: string;
  name: string;
  level: number;
}

export interface JobGradeFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  level?: number | null;
}

export interface CreateJobGradeRequest {
  name: string;
  level: number;
}

export interface UpdateJobGradeRequest {
  name: string;
  level: number;
}
