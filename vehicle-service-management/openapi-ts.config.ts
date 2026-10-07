import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
  input: 'https://localhost:7102/openapi/v1.json',
  output: {
    path: 'src/app/core/api/generated',
    postProcess: ['prettier'],
  },
  plugins: ['@hey-api/client-angular', '@hey-api/typescript', '@hey-api/sdk'],
});
