import { environment } from '../../../environments/environment';

/** Builds an absolute API URL in dev, or same-origin `/api/...` when `apiBaseUrl` is empty (typical behind reverse proxy). */
export function apiUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  const base = environment.apiBaseUrl.trim();
  if (!base) return normalized;
  return `${base.replace(/\/$/, '')}${normalized}`;
}

export function isSameAppApiRequest(url: string): boolean {
  if (url.startsWith('/api/')) return true;
  const base = environment.apiBaseUrl.trim();
  return !!base && url.startsWith(base);
}
