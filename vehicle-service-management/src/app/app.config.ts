import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { client } from './core/api/generated/client.gen';
import { provideHeyApiClient } from './core/api/generated/client/client.gen';

client.setConfig({
  baseUrl: 'https://localhost:7102',
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideHeyApiClient(client),
  ],
};
