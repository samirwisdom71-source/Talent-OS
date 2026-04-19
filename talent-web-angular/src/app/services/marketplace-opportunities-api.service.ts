import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  CreateMarketplaceOpportunityRequest,
  MarketplaceOpportunityDto,
  MarketplaceOpportunityFilterRequest,
} from '../shared/models/marketplace.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class MarketplaceOpportunitiesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/marketplace-opportunities');

  getPaged(filter: MarketplaceOpportunityFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      status: filter.status ?? undefined,
      opportunityType: filter.opportunityType ?? undefined,
      organizationUnitId: filter.organizationUnitId ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<MarketplaceOpportunityDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<MarketplaceOpportunityDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateMarketplaceOpportunityRequest) {
    return this.http
      .post<ApiResponse<MarketplaceOpportunityDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
