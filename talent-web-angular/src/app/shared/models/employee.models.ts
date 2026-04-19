export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  accessToken: string;
  expiresAtUtc: string;
  tokenType: string;
}

export interface EmployeeDto {
  id: string;
  employeeNumber: string;
  fullNameAr: string;
  fullNameEn: string;
  email: string;
  organizationUnitId: string;
  positionId: string;
}

export type EmployeeListItemDto = EmployeeDto;

export interface EmployeeFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateEmployeeRequest {
  employeeNumber: string;
  fullNameAr: string;
  fullNameEn: string;
  email: string;
  organizationUnitId: string;
  positionId: string;
}
