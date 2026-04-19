import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { apiUrl } from '../core/config/api-url';
import { ApiResponse, PagedResult } from '../shared/models/api.types';
import {
  ApprovalAssignRequest,
  ApprovalRequestDto,
  ApprovalRequestFilterRequest,
  ApprovalRequestListItemDto,
  ApprovalReassignRequest,
  ApprovalWorkflowCommentRequest,
  CreateApprovalRequestRequest,
} from '../shared/models/approval.models';
import { toHttpParams, unwrapApiResponse } from '../shared/utils/api-helpers';

@Injectable({ providedIn: 'root' })
export class ApprovalsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = apiUrl('/api/approval-requests');

  getPaged(filter: ApprovalRequestFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      status: filter.status ?? undefined,
      requestType: filter.requestType ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<ApprovalRequestListItemDto>>>(this.base, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getMySubmitted(filter: ApprovalRequestFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      status: filter.status ?? undefined,
      requestType: filter.requestType ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<ApprovalRequestListItemDto>>>(`${this.base}/my-submitted`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getMyAssigned(filter: ApprovalRequestFilterRequest) {
    const params = toHttpParams({
      page: filter.page,
      pageSize: filter.pageSize,
      status: filter.status ?? undefined,
      requestType: filter.requestType ?? undefined,
    });
    return this.http
      .get<ApiResponse<PagedResult<ApprovalRequestListItemDto>>>(`${this.base}/my-assigned`, { params })
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  getById(id: string) {
    return this.http
      .get<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}`)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  create(body: CreateApprovalRequestRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(this.base, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  submit(id: string) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/submit`, {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  assign(id: string, body: ApprovalAssignRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/assign`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  reassign(id: string, body: ApprovalReassignRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/reassign`, body)
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  startReview(id: string, body?: ApprovalWorkflowCommentRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/start-review`, body ?? {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  approve(id: string, body?: ApprovalWorkflowCommentRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/approve`, body ?? {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  reject(id: string, body?: ApprovalWorkflowCommentRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/reject`, body ?? {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }

  cancel(id: string, body?: ApprovalWorkflowCommentRequest) {
    return this.http
      .post<ApiResponse<ApprovalRequestDto>>(`${this.base}/${id}/cancel`, body ?? {})
      .pipe(map((r) => unwrapApiResponse(r)));
  }
}
