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

  // Inject script to capture console.log calls
  await page.addInitScript(() => {
    window.__telemetryLogs = [];
    const oldLog = console.log;
    console.log = (...args) => {
      window.__telemetryLogs.push(args.join(" "));
      oldLog(...args);
    };
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

  // Assert that we received at least one telemetry message
  expect(telemetryMessages.length).toBeGreaterThan(0);

  // Assert that the message contains 'Hello Arena'
  const hasHelloArena = telemetryMessages.some(msg => msg.includes('Hello Arena'));
  expect(hasHelloArena).toBe(true);

  // Assert that the message contains 'Pocket Squire'
  const hasPocketSquire = telemetryMessages.some(msg => msg.includes('Pocket Squire'));
  expect(hasPocketSquire).toBe(true);
});
