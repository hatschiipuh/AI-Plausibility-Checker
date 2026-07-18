import { HttpInterceptorFn } from '@angular/common/http';

const accessTokenStorageKey = 'accessToken';

export const authTokenInterceptor: HttpInterceptorFn = (request, next) => {
  const accessToken = localStorage.getItem(accessTokenStorageKey);

  if (!accessToken) {
    return next(request);
  }

  return next(
    request.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    })
  );
};
