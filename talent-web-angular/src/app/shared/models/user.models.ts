export interface UserListItemDto {
  id: string;
  userName: string;
  email: string;
  isActive: boolean;
  employeeId?: string | null;
  lastLoginUtc?: string | null;
}

export interface UserDto extends UserListItemDto {
  roleNames: readonly string[];
}

export interface UserFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateUserRequest {
  userName: string;
  email: string;
  password: string;
  employeeId?: string | null;
  roleIds: string[];
}

export interface UpdateUserRequest {
  userName: string;
  email: string;
  employeeId?: string | null;
  newPassword?: string | null;
}

export interface AssignUserRolesRequest {
  roleIds: string[];
}
