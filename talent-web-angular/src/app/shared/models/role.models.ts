export interface RoleListItemDto {
  id: string;
  name: string;
  description?: string | null;
  isSystemRole: boolean;
}

export interface RoleDto extends RoleListItemDto {
  permissionCodes: readonly string[];
}

export interface PermissionDto {
  id: string;
  code: string;
  name: string;
  module: string;
}

export interface RoleFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
}

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
  isSystemRole: boolean;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string | null;
}

export interface AssignRolePermissionsRequest {
  permissionIds: string[];
}
