import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: 1,
  reporter: [['html'], ['list']],

  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  /**
   * Start both servers before running tests.
   * Set reuseExistingServer: true locally so you can pre-start them manually.
   * On CI both will be started fresh.
   */
  webServer: [
    {
      command: 'cd ../backend/TodoApi && dotnet run --urls http://localhost:5000',
      url: 'http://localhost:5000/swagger/index.html',
      reuseExistingServer: !process.env['CI'],
      timeout: 120_000,
    },
    {
      command: 'cd ../frontend && npm start -- --port 4200',
      url: 'http://localhost:4200',
      reuseExistingServer: !process.env['CI'],
      timeout: 120_000,
    },
  ],
});
