export interface OrganizationUnitDto {
  id: string;
  nameAr: string;
  nameEn: string;
  parentId?: string | null;
  parentNameAr?: string | null;
  parentNameEn?: string | null;
}

export interface OrganizationUnitFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  parentId?: string | null;
}

export interface CreateOrganizationUnitRequest {
  nameAr: string;
  nameEn: string;
  parentId?: string | null;
}

export interface UpdateOrganizationUnitRequest {
  nameAr: string;
  nameEn: string;
  parentId?: string | null;
}
