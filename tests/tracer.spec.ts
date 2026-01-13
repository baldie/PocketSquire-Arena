import { test, expect } from '@playwright/test';

test('Unity telemetry validation', async ({ page }) => {
  // Listen for console messages
  const telemetryMessages: string[] = [];

  page.on('console', (msg) => {
    const text = msg.text();
    if (text.includes('TELEMETRY_MESSAGE')) {
      telemetryMessages.push(text);
    }
  });

  // Navigate to the Unity WebGL build
  await page.goto('/index.html');

  // Wait for the telemetry message (with timeout)
  await page.waitForFunction(
    () => {
      const logs = (window as any).__telemetryLogs || [];
      return logs.some((log: string) => log.includes('TELEMETRY_MESSAGE'));
    },
    { timeout: 30000 }
  ).catch(() => {
    // If waitForFunction times out, we'll rely on the console listener
  });

  // Give Unity a bit more time to initialize and log
  await page.waitForTimeout(5000);

  // Assert that we received at least one telemetry message
  expect(telemetryMessages.length).toBeGreaterThan(0);

  // Assert that the message contains 'Hello Arena'
  const hasHelloArena = telemetryMessages.some(msg => msg.includes('Hello Arena'));
  expect(hasHelloArena).toBe(true);
});
