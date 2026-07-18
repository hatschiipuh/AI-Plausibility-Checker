import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { API_BUSINESSSERVICECLIENT_BASE_URL } from './api/business-service-client';
import { authTokenInterceptor } from './auth-token.interceptor';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authTokenInterceptor])),
    provideAnimations(),
    provideRouter(routes),
    { provide: API_BUSINESSSERVICECLIENT_BASE_URL, useValue: '' }
  ]
};
