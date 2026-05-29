import { chromium } from 'playwright';
import fs from 'fs';

const BASE = 'http://localhost:5050';
const SHOTS = 'C:\\Temp\\shots';
if (!fs.existsSync(SHOTS)) fs.mkdirSync(SHOTS, { recursive: true });

const browser = await chromium.launch({ headless: true });
const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 } });
const page = await ctx.newPage();
const errors = [];

async function shot(name) {
  const p = `${SHOTS}\\${name}.png`;
  await page.screenshot({ path: p });
  console.log(`[screenshot] ${p}`);
}

// ── 1. Login page (Turkish default) ─────────────────────────────────────
await page.goto(`${BASE}/Auth/Login`);
await page.waitForLoadState('domcontentloaded');
await shot('01_login_tr');

const loginSubtitle = await page.locator('p.text-muted.small').first().textContent();
const loginBtn = await page.locator('button[type="submit"]').textContent();
const loginUserLabel = await page.locator('label').first().textContent();
console.log('[TR] Subtitle:', loginSubtitle?.trim());
console.log('[TR] Button:', loginBtn?.trim());
console.log('[TR] User label:', loginUserLabel?.trim());

if (!loginSubtitle?.includes('Kafe')) errors.push('TR: Login subtitle should be Turkish ("Kafe...") got: ' + loginSubtitle?.trim());
if (!loginBtn?.includes('Giriş')) errors.push('TR: Login button should be Turkish got: ' + loginBtn?.trim());

// ── 2. Switch to English on login page ──────────────────────────────────
await page.click('.dropdown-toggle');
await page.waitForTimeout(400);
await page.click('a:has-text("English")');
await page.waitForLoadState('domcontentloaded');
await shot('02_login_en');

const loginSubtitleEN = await page.locator('p.text-muted.small').first().textContent();
const loginBtnEN = await page.locator('button[type="submit"]').textContent();
const loginUserLabelEN = await page.locator('label').first().textContent();
console.log('[EN] Subtitle:', loginSubtitleEN?.trim());
console.log('[EN] Button:', loginBtnEN?.trim());
console.log('[EN] User label:', loginUserLabelEN?.trim());

if (!loginSubtitleEN?.includes('Cafe')) errors.push('EN: Login subtitle should be English got: ' + loginSubtitleEN?.trim());
if (!loginBtnEN?.includes('Sign')) errors.push('EN: Login button should say Sign In got: ' + loginBtnEN?.trim());

// ── 3. Log in ────────────────────────────────────────────────────────────
await page.fill('input[name="username"]', 'admin');
await page.fill('input[name="password"]', 'Costify2025!');
await page.click('button[type="submit"]');
try {
  await page.waitForURL(`${BASE}/`, { timeout: 6000 });
} catch {
  errors.push(`Login failed – URL: ${page.url()}`);
}
await page.waitForLoadState('domcontentloaded');
await shot('03_dashboard_en');

// ── 4. Dashboard checks (EN) ─────────────────────────────────────────────
const navLinksEN = await page.locator('.sidebar-nav .sidebar-link span:last-child').allTextContents();
console.log('[EN] Nav items:', navLinksEN.map(t => t.trim()));
const statLabelsEN = await page.locator('.stat-label').allTextContents();
console.log('[EN] Stat labels:', statLabelsEN.map(t => t.trim()));
const langBtn = await page.locator('.topbar .dropdown-toggle').textContent();
console.log('[EN] Lang button:', langBtn?.trim());

if (!navLinksEN.some(n => n.includes('Overview'))) errors.push('EN: Nav missing "Overview"');
if (!navLinksEN.some(n => n.includes('Products'))) errors.push('EN: Nav missing "Products"');
if (!statLabelsEN.some(l => l.includes('Total Products'))) errors.push('EN: Stat missing "Total Products"');

// ── 5. Switch to Turkish ─────────────────────────────────────────────────
await page.click('.topbar .dropdown-toggle');
await page.waitForTimeout(400);
await page.click('.dropdown-menu .dropdown-item:has-text("Türkçe")');
await page.waitForLoadState('domcontentloaded');
await shot('04_dashboard_tr');

const navLinksTR = await page.locator('.sidebar-nav .sidebar-link span:last-child').allTextContents();
console.log('[TR] Nav items:', navLinksTR.map(t => t.trim()));
const statLabelsTR = await page.locator('.stat-label').allTextContents();
console.log('[TR] Stat labels:', statLabelsTR.map(t => t.trim()));

if (!navLinksTR.some(n => n.includes('Genel'))) errors.push('TR: Nav missing "Genel Bakış"');
if (!statLabelsTR.some(l => l.includes('Toplam Ürün'))) errors.push('TR: Stat missing "Toplam Ürün"');

// ── 6. Products page (TR) ────────────────────────────────────────────────
await page.click('.sidebar-link:has-text("Ürünler")');
await page.waitForLoadState('domcontentloaded');
await shot('05_products_tr');
const prodHeaders = await page.locator('.card table thead th').allTextContents();
console.log('[TR] Products headers:', prodHeaders.map(t => t.trim()).filter(Boolean));
const newProdBtn = await page.locator('.page-header .btn-primary').textContent();
console.log('[TR] New product btn:', newProdBtn?.trim());
if (!newProdBtn?.includes('Yeni')) errors.push('TR: New product button wrong: ' + newProdBtn?.trim());

// ── 7. Open product modal (TR) ───────────────────────────────────────────
await page.click('.page-header .btn-primary');
await page.waitForSelector('.modal.show', { timeout: 3000 }).catch(() => {});
await page.waitForTimeout(400);
await shot('06_product_modal_tr');
const modalTitle = await page.locator('#productModalTitle').textContent().catch(() => '');
console.log('[TR] Modal title:', modalTitle?.trim());
if (!modalTitle?.includes('Yeni Ürün')) errors.push('TR: Modal title wrong: ' + modalTitle?.trim());
await page.keyboard.press('Escape');
await page.waitForTimeout(300);

// ── 8. Products (EN) ─────────────────────────────────────────────────────
await page.click('.topbar .dropdown-toggle');
await page.waitForTimeout(400);
await page.click('.dropdown-menu .dropdown-item:has-text("English")');
await page.waitForLoadState('domcontentloaded');
await shot('07_products_en');
const prodHeadersEN = await page.locator('.card table thead th').allTextContents();
console.log('[EN] Products headers:', prodHeadersEN.map(t => t.trim()).filter(Boolean));
const newProdBtnEN = await page.locator('.page-header .btn-primary').textContent();
console.log('[EN] New product btn:', newProdBtnEN?.trim());
if (!newProdBtnEN?.includes('New')) errors.push('EN: New product button wrong: ' + newProdBtnEN?.trim());

// ── 9. Reports (EN) ──────────────────────────────────────────────────────
await page.click('.sidebar-link:has-text("Reports")');
await page.waitForLoadState('domcontentloaded');
await shot('08_reports_en');
const reportStatsEN = await page.locator('.stat-label').allTextContents();
console.log('[EN] Reports stats:', reportStatsEN.map(t => t.trim()));
if (!reportStatsEN.some(l => l.includes('Total Purchases') || l.includes('Inventory')))
  errors.push('EN: Reports stats not English');

// ── 10. Reports (TR) ─────────────────────────────────────────────────────
await page.click('.topbar .dropdown-toggle');
await page.waitForTimeout(400);
await page.click('.dropdown-menu .dropdown-item:has-text("Türkçe")');
await page.waitForLoadState('domcontentloaded');
await shot('09_reports_tr');
const reportStatsTR = await page.locator('.stat-label').allTextContents();
console.log('[TR] Reports stats:', reportStatsTR.map(t => t.trim()));
if (!reportStatsTR.some(l => l.includes('Stok'))) errors.push('TR: Reports stats not Turkish');

// ── 11. Definitions (TR) ─────────────────────────────────────────────────
await page.click('.sidebar-link:has-text("Kategoriler")');
await page.waitForLoadState('domcontentloaded');
await shot('10_definitions_tr');
const tabs = await page.locator('.nav-tabs .nav-link').allTextContents();
console.log('[TR] Definitions tabs:', tabs.map(t => t.trim()));

// ── Summary ───────────────────────────────────────────────────────────────
await browser.close();
console.log('\n' + '─'.repeat(60));
if (errors.length === 0) {
  console.log('✅  ALL CHECKS PASSED');
} else {
  console.log(`❌  ${errors.length} FAILURE(S):`);
  errors.forEach(e => console.log('  •', e));
}
console.log('Screenshots saved to: C:\\Temp\\shots\\');
