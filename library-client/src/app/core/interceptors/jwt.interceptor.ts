import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const user = inject(AuthService).currentUser();
  const lang = localStorage.getItem('lang') || 'SK';

  const headers: Record<string, string> = {
    'Accept-Language': lang
  };

  if (user?.token) {
    headers['Authorization'] = `Bearer ${user.token}`;
  }

  req = req.clone({
    setHeaders: headers
  });

  return next(req);
};
