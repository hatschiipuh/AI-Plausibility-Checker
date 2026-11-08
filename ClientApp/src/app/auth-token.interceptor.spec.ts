import { HttpRequest, HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { authTokenInterceptor } from './auth-token.interceptor';

describe('authTokenInterceptor', () => {
  const accessTokenStorageKey = 'accessToken';

  afterEach(() => {
    localStorage.clear();
  });

  it('adds the Authorization header when an access token is stored', () => {
    localStorage.setItem(accessTokenStorageKey, 'stored-token');
    const request = new HttpRequest('GET', '/api/BusinessService/Validations/GetValidationsForUser');
    const next = vi.fn().mockReturnValue(of(new HttpResponse({ status: 200 })));

    authTokenInterceptor(request, next).subscribe();

    expect(next).toHaveBeenCalledTimes(1);
    const forwardedRequest = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwardedRequest.headers.get('Authorization')).toBe('Bearer stored-token');
  });

  it('forwards the request unchanged when no access token is stored', () => {
    const request = new HttpRequest('GET', '/api/BusinessService/Validations/GetValidationsForUser');
    const next = vi.fn().mockReturnValue(of(new HttpResponse({ status: 200 })));

    authTokenInterceptor(request, next).subscribe();

    expect(next).toHaveBeenCalledTimes(1);
    const forwardedRequest = next.mock.calls[0][0] as HttpRequest<unknown>;
    expect(forwardedRequest).toBe(request);
    expect(forwardedRequest.headers.has('Authorization')).toBe(false);
  });
});
