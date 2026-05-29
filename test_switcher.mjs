import { chromium } from 'playwright';

const BASE = 'http://localhost:5050';
const SHOTS = 'C:\\Temp\\shots';
const browser = await chromium.launch({ headless: true });
const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 } });
const page = await ctx.newPage();

// Screenshot login TR
await page.goto(`${BASE}/Auth/Login`);
await page.waitForLoadState('domcontentloaded');
await page.screenshot({ path: `${SHOTS}\\lang_login_tr.png` });

// Switch to EN
await page.goto(`${BASE}/Language/Set?culture=en-US&returnUrl=/Auth/Login`);
await page.waitForLoadState('domcontentloaded');
await page.screenshot({ path: `${SHOTS}\\lang_login_en.png` });

// Login and show dashboard TR
await page.goto(`${BASE}/Language/Set?culture=tr-TR&returnUrl=/Auth/Login`);
await page.fill('input[name="username"]', 'admin');
await page.fill('input[name="password"]', 'Costify2025!');
await page.click('button[type="submit"]');
await page.waitForURL(`${BASE}/`, { timeout: 5000 }).catch(() => {});
await page.waitForLoadState('domcontentloaded');
await page.screenshot({ path: `${SHOTS}\\lang_dashboard_tr.png` });

// Switch to EN
await page.click('.lang-switcher a:last-child');
await page.waitForLoadState('domcontentloaded');
await page.screenshot({ path: `${SHOTS}\\lang_dashboard_en.png` });

await browser.close();
console.log('Done. Screenshots in C:\\Temp\\shots\\');
