import { chromium } from 'playwright';

const b = await chromium.launch({ headless: true });
const p = await b.newPage({ viewport: { width: 1440, height: 900 } });

await p.goto('http://localhost:5050/Auth/Login');
await p.fill('input[name="username"]', 'admin');
await p.fill('input[name="password"]', 'Costify2025!');
await p.click('button[type="submit"]');
await p.waitForURL('http://localhost:5050/', { timeout: 5000 });

// Products page
await p.click('.sidebar-link:has-text("Ürünler")');
await p.waitForLoadState('domcontentloaded');
await p.screenshot({ path: 'C:\\Temp\\shots\\pagination_products.png' });
const paginInfo = await p.locator('.d-flex:has(.pagination)').first().textContent().catch(() => 'not found');
console.log('Products pagination bar:', paginInfo?.trim().replace(/\s+/g, ' '));

// Purchase Orders
await p.click('.sidebar-link:has-text("Satın")');
await p.waitForLoadState('domcontentloaded');
await p.screenshot({ path: 'C:\\Temp\\shots\\pagination_orders.png' });

// Vendors
await p.click('.sidebar-link:has-text("Tedarik")');
await p.waitForLoadState('domcontentloaded');
await p.screenshot({ path: 'C:\\Temp\\shots\\pagination_vendors.png' });

await b.close();
console.log('Done');
