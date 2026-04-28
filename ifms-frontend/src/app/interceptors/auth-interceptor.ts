import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { isIdentityAuthEndpoint } from '../utils/auth-http.util';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  const hadToken = !!token;
  const outgoing = token
    ? req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) })
    : req;

  return next(outgoing).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        if (hadToken && !isIdentityAuthEndpoint(req.url)) {
          authService.logout();
          void router.navigate(['/login'], {
            queryParams: { reason: 'session' },
            replaceUrl: true
          });
        }
      }
      return throwError(() => err);
    })
  );
};
