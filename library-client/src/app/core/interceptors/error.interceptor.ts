import { HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error) => {
      const correlationId = error.headers?.get('X-Correlation-ID');
      if (correlationId) {
        console.error(`[Application Error] Correlation ID: ${correlationId}`, error);
      }
      return throwError(() => error);
    })
  );
};
