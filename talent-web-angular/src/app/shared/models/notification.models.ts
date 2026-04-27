export interface NotificationListItemDto {
  id: string;
  notificationType: number;
  title: string;
  message: string;
  channel: number;
  isRead: boolean;
  relatedEntityId?: string | null;
  relatedEntityType?: string | null;
  createdOnUtc: string;
}

export interface NotificationFilterRequest {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean | null;
}

export interface NotificationTemplateDto {
  id: string;
  code: string;
  name: string;
  subjectTemplate?: string | null;
  bodyTemplate: string;
  notificationType: number;
  channel: number;
  isActive: boolean;
}

export interface NotificationTemplateFilterRequest {
  page?: number;
  pageSize?: number;
  search?: string | null;
  activeOnly?: boolean | null;
}

export interface CreateNotificationTemplateRequest {
  code: string;
  name: string;
  subjectTemplate?: string | null;
  bodyTemplate: string;
  notificationType: number;
  channel: number;
}

export interface UpdateNotificationTemplateRequest {
  name: string;
  subjectTemplate?: string | null;
  bodyTemplate: string;
  notificationType: number;
  channel: number;
  isActive: boolean;
}
