import { HttpErrorResponse } from '@angular/common/http';

/** Identity API calls where 401 is an expected outcome (wrong password, bad OTP, etc.). */
export function isIdentityAuthEndpoint(url: string): boolean {
  const u = url.toLowerCase();
  return (
    u.includes('/auth/login') ||
    u.includes('/auth/register') ||
    u.includes('/auth/password/')
  );
}

/** Maps HttpClient failures (e.g. API not running) to a clear message for the UI. */
export function friendlyIdentityApiError(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse && err.status === 0) {
    return (
      'Cannot reach IFMS Identity API on port 5001. Start it: from the IFMS folder run ' +
      '`docker compose up -d sqlserver identity-api`, or run `dotnet run --project IFMS.Identity.API` ' +
      '(HTTP profile listens on http://localhost:5001).'
    );
  }
  if (err instanceof HttpErrorResponse && typeof err.error === 'object' && err.error) {
    const o = err.error as { error?: unknown; message?: unknown; detail?: unknown; title?: unknown };
    if (typeof o.error === 'string' && o.error.trim()) return o.error;
    if (typeof o.message === 'string' && o.message.trim()) return o.message;
    if (typeof o.detail === 'string' && o.detail.trim()) return o.detail;
    if (typeof o.title === 'string' && o.title.trim()) return o.title;
  }
  return fallback;
}
