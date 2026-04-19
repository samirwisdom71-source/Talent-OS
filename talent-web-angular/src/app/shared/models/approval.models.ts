export interface ApprovalRequestDto {
  id: string;
  requestType: number;
  relatedEntityId: string;
  requestedByUserId: string;
  currentApproverUserId?: string | null;
  status: number;
  submittedOnUtc?: string | null;
  completedOnUtc?: string | null;
  title: string;
  summary?: string | null;
  notes?: string | null;
  actions: readonly ApprovalActionDto[];
  assignments: readonly ApprovalAssignmentDto[];
}

export interface ApprovalActionDto {
  id: string;
  actionType: number;
  actionByUserId: string;
  comments?: string | null;
  actionedOnUtc: string;
}

export interface ApprovalAssignmentDto {
  id: string;
  assignedToUserId: string;
  assignedByUserId: string;
  assignedOnUtc: string;
  isCurrent: boolean;
  notes?: string | null;
}

export interface ApprovalRequestListItemDto {
  id: string;
  requestType: number;
  relatedEntityId: string;
  requestedByUserId: string;
  currentApproverUserId?: string | null;
  status: number;
  submittedOnUtc?: string | null;
  title: string;
}

export interface ApprovalRequestFilterRequest {
  page?: number;
  pageSize?: number;
  status?: number | null;
  requestType?: number | null;
}

export interface ApprovalWorkflowCommentRequest {
  comments?: string | null;
}

export interface ApprovalAssignRequest {
  approverUserId: string;
  notes?: string | null;
}

export interface ApprovalReassignRequest {
  newApproverUserId: string;
  notes?: string | null;
}

export interface CreateApprovalRequestRequest {
  requestType: number;
  relatedEntityId: string;
  title: string;
  summary?: string | null;
  notes?: string | null;
}
