using ClosedXML.Excel;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ReportsController(
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _currentBusiness = currentBusiness;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var vm = await BuildReportAsync(from, now);
        return View(vm);
    }

    // ── Satın Alma Raporu (Excel) ─────────────────────────────────────────
    public async Task<IActionResult> Export(string period = "monthly")
    {
        var (from, to, label) = GetDateRange(period);

        var orders = await _context.PurchaseOrders
            .Include(o => o.Vendor)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Unit)
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(_localizer["Excel_PurchaseSheet"].Value);

        string[] headers = [
            _localizer["Excel_OrderNo"].Value, _localizer["Excel_Vendor"].Value,
            _localizer["Excel_Date"].Value, _localizer["Excel_Product"].Value,
            _localizer["Excel_Qty"].Value, _localizer["Excel_Unit"].Value,
            _localizer["Excel_UnitPrice"].Value, _localizer["Excel_LineTotal"].Value,
            _localizer["Excel_OrderTotal"].Value, _localizer["Excel_Status"].Value
        ];
        StyleHeader(ws, headers);

        int row = 2;
        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                ws.Cell(row, 1).Value = order.OrderNumber;
                ws.Cell(row, 2).Value = order.Vendor?.Name ?? "";
                ws.Cell(row, 3).Value = order.OrderDate.ToString("dd.MM.yyyy");
                ws.Cell(row, 4).Value = item.Product?.Name ?? "";
                ws.Cell(row, 5).Value = (double)item.Quantity;
                ws.Cell(row, 6).Value = item.Product?.Unit?.Symbol ?? "";
                ws.Cell(row, 7).Value = (double)item.UnitPrice;
                ws.Cell(row, 8).Value = (double)item.TotalPrice;
                ws.Cell(row, 9).Value = (double)order.TotalAmount;
                ws.Cell(row, 10).Value = GetStatusLabel(order.Status);

                ApplyNumberFormat(ws, row, new[] { 7, 8, 9 });
                if (row % 2 == 0) AlternateRow(ws, row, headers.Length);
                row++;
            }
        }

        if (row > 2)
        {
            ws.Cell(row, 7).Value = _localizer["Excel_GrandTotal"].Value;
            ws.Cell(row, 8).FormulaA1 = $"=SUM(H2:H{row - 1})";
            ws.Cell(row, 9).FormulaA1 = $"=SUM(I2:I{row - 1})";
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            ApplyNumberFormat(ws, row, new[] { 8, 9 });
        }

        FinishSheet(ws, headers.Length, row);
        var fileName = $"Costify_SatinAlma_{label}_{DateTime.Today:yyyyMMdd}.xlsx";
        return ExcelFile(wb, fileName);
    }

    // ── Stok Raporu (Excel) ──────────────────────────────────────────────
    public async Task<IActionResult> ExportStock()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category!.Name).ThenBy(p => p.Name)
            .ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(_localizer["Excel_StockSheet"].Value);

        string[] headers = [
            _localizer["Common_Name"].Value, _localizer["Excel_SKU"].Value,
            _localizer["Excel_Category"].Value, _localizer["Excel_Unit"].Value,
            _localizer["Excel_CurrentStock"].Value, _localizer["Excel_MinStock"].Value,
            _localizer["Excel_LastPrice"].Value, _localizer["Excel_StockValue"].Value,
            _localizer["Excel_StockStatus"].Value
        ];
        StyleHeader(ws, headers);

        int row = 2;
        foreach (var p in products)
        {
            var status = p.CurrentStock <= 0 ? _localizer["Excel_OutOfStock"].Value
                       : p.CurrentStock < p.MinimumStock ? _localizer["Excel_Critical"].Value
                       : _localizer["Excel_Normal"].Value;

            ws.Cell(row, 1).Value = p.Name;
            ws.Cell(row, 2).Value = p.SKU ?? "";
            ws.Cell(row, 3).Value = p.Category?.Name ?? "";
            ws.Cell(row, 4).Value = p.Unit?.Symbol ?? "";
            ws.Cell(row, 5).Value = (double)p.CurrentStock;
            ws.Cell(row, 6).Value = (double)p.MinimumStock;
            ws.Cell(row, 7).Value = (double)p.LastPurchasePrice;
            ws.Cell(row, 8).Value = (double)(p.CurrentStock * p.LastPurchasePrice);
            ws.Cell(row, 9).Value = status;

            if (status == _localizer["Excel_OutOfStock"].Value)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
            else if (status == _localizer["Excel_Critical"].Value)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF3C7");
            else if (row % 2 == 0)
                AlternateRow(ws, row, headers.Length);

            ApplyNumberFormat(ws, row, new[] { 5, 6, 7, 8 });
            row++;
        }

        if (row > 2)
        {
            ws.Cell(row, 7).Value = _localizer["Excel_TotalStockValue"].Value;
            ws.Cell(row, 8).FormulaA1 = $"=SUM(H2:H{row - 1})";
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
            ApplyNumberFormat(ws, row, new[] { 8 });
        }

        FinishSheet(ws, headers.Length, row);
        return ExcelFile(wb, $"Costify_Stok_{DateTime.Today:yyyyMMdd}.xlsx");
    }

    // ── Reçete Raporu (Excel — 2 sekme) ──────────────────────────────────
    public async Task<IActionResult> ExportRecipes()
    {
        var recipes = await _context.Recipes
            .Include(r => r.Ingredients).ThenInclude(i => i.Product).ThenInclude(p => p.Unit)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Category).ThenBy(r => r.Name)
            .ToListAsync();

        using var wb = new XLWorkbook();

        var ws1 = wb.Worksheets.Add(_localizer["Excel_RecipeSummarySheet"].Value);
        string[] sumHeaders = [
            _localizer["Excel_Recipe"].Value, _localizer["Excel_Category"].Value,
            _localizer["Excel_SellingPrice"].Value, _localizer["Excel_IngredientCost"].Value,
            _localizer["Excel_GrossProfit"].Value, _localizer["Excel_FoodCostPct"].Value,
            _localizer["Excel_Evaluation"].Value
        ];
        StyleHeader(ws1, sumHeaders);

        int r1 = 2;
        foreach (var rec in recipes)
        {
            ws1.Cell(r1, 1).Value = rec.Name;
            ws1.Cell(r1, 2).Value = rec.Category ?? "";
            ws1.Cell(r1, 3).Value = (double)rec.SellingPrice;
            ws1.Cell(r1, 4).Value = (double)rec.TotalCost;
            ws1.Cell(r1, 5).Value = (double)(rec.SellingPrice - rec.TotalCost);
            ws1.Cell(r1, 6).Value = (double)rec.FoodCostPercentage / 100;
            ws1.Cell(r1, 7).Value = rec.FoodCostPercentage < 25 ? _localizer["Excel_Ideal"].Value
                                  : rec.FoodCostPercentage < 35 ? _localizer["Excel_Moderate"].Value
                                  : _localizer["Excel_High"].Value;

            ws1.Cell(r1, 6).Style.NumberFormat.Format = "0.0%";
            ApplyNumberFormat(ws1, r1, new[] { 3, 4, 5 });

            // Renk kodlama
            var bg = rec.FoodCostPercentage < 25 ? "#D1FAE5"
                   : rec.FoodCostPercentage < 35 ? "#FEF3C7"
                   : "#FEE2E2";
            ws1.Cell(r1, 6).Style.Fill.BackgroundColor = XLColor.FromHtml(bg);
            ws1.Cell(r1, 7).Style.Fill.BackgroundColor = XLColor.FromHtml(bg);

            if (r1 % 2 == 0) AlternateRow(ws1, r1, 5); // sadece ilk 5 sütun
            r1++;
        }
        FinishSheet(ws1, sumHeaders.Length, r1);

        var ws2 = wb.Worksheets.Add(_localizer["Excel_RecipeDetailSheet"].Value);
        string[] detHeaders = [
            _localizer["Excel_Recipe"].Value, _localizer["Excel_Category"].Value,
            _localizer["Excel_Ingredient"].Value, _localizer["Excel_Qty"].Value,
            _localizer["Excel_Unit"].Value, _localizer["Excel_UnitPrice"].Value,
            _localizer["Excel_LineCost"].Value
        ];
        StyleHeader(ws2, detHeaders);

        int r2 = 2;
        foreach (var rec in recipes)
        {
            foreach (var ing in rec.Ingredients)
            {
                ws2.Cell(r2, 1).Value = rec.Name;
                ws2.Cell(r2, 2).Value = rec.Category ?? "";
                ws2.Cell(r2, 3).Value = ing.Product?.Name ?? "";
                ws2.Cell(r2, 4).Value = (double)ing.Quantity;
                ws2.Cell(r2, 5).Value = ing.Product?.Unit?.Symbol ?? "";
                ws2.Cell(r2, 6).Value = (double)(ing.Product?.LastPurchasePrice ?? 0);
                ws2.Cell(r2, 7).Value = (double)ing.TotalCost;

                ws2.Cell(r2, 4).Style.NumberFormat.Format = "#,##0.0000";
                ApplyNumberFormat(ws2, r2, new[] { 6, 7 });
                if (r2 % 2 == 0) AlternateRow(ws2, r2, detHeaders.Length);
                r2++;
            }
        }
        FinishSheet(ws2, detHeaders.Length, r2);

        return ExcelFile(wb, $"Costify_Receteler_{DateTime.Today:yyyyMMdd}.xlsx");
    }

    // ── Yardımcı metotlar ─────────────────────────────────────────────────
    private static void StyleHeader(IXLWorksheet ws, string[] headers)
    {
        for (int c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(1).Height = 22;
        ws.SheetView.FreezeRows(1);
    }

    private static void ApplyNumberFormat(IXLWorksheet ws, int row, int[] cols)
    {
        foreach (var c in cols)
            ws.Cell(row, c).Style.NumberFormat.Format = "#,##0.00";
    }

    private static void AlternateRow(IXLWorksheet ws, int row, int colCount)
    {
        ws.Range(row, 1, row, colCount).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
    }

    private static void FinishSheet(IXLWorksheet ws, int colCount, int lastRow)
    {
        if (lastRow > 1)
        {
            var tableRange = ws.Range(1, 1, lastRow, colCount);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E2E8F0");
        }
        ws.Columns().AdjustToContents(minWidth: 10, maxWidth: 45);
    }

    private IActionResult ExcelFile(XLWorkbook wb, string fileName)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static (DateTime from, DateTime to, string label) GetDateRange(string period)
    {
        var now = DateTime.UtcNow.Date;
        return period switch
        {
            "daily"  => (now, now.AddDays(1).AddSeconds(-1), "Gunluk"),
            "weekly" => (now.AddDays(-6), now.AddDays(1).AddSeconds(-1), "Haftalik"),
            "yearly" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31, 23, 59, 59), "Yillik"),
            _        => (new DateTime(now.Year, now.Month, 1),
                         new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1), "Aylik")
        };
    }

    private string GetStatusLabel(Costify.Domain.Enums.OrderStatus status) => status switch
    {
        Costify.Domain.Enums.OrderStatus.Received          => _localizer["Common_Received"].Value,
        Costify.Domain.Enums.OrderStatus.Ordered           => _localizer["Common_Ordered"].Value,
        Costify.Domain.Enums.OrderStatus.Cancelled         => _localizer["Common_Cancelled"].Value,
        Costify.Domain.Enums.OrderStatus.PartiallyReceived => _localizer["Common_PartiallyReceived"].Value,
        _                                                  => _localizer["Common_Draft"].Value
    };

    private async Task<ReportViewModel> BuildReportAsync(DateTime from, DateTime to)
    {
        var monthlySpends = await _context.PurchaseOrders
            .Where(o => o.OrderDate >= from && o.OrderDate <= to)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(o => o.TotalAmount) })
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .ToListAsync();

        var vendorSpends = await _context.PurchaseOrders
            .Include(o => o.Vendor)
            .GroupBy(o => o.Vendor!.Name)
            .Select(g => new { VendorName = g.Key, Amount = g.Sum(o => o.TotalAmount) })
            .OrderByDescending(r => r.Amount)
            .Take(8)
            .ToListAsync();

        var totalVendorSpend = vendorSpends.Sum(v => v.Amount);

        var stockValues = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .GroupBy(p => new { p.Category!.Name, p.Category.ColorHex })
            .Select(g => new { g.Key.Name, g.Key.ColorHex, Value = g.Sum(p => p.CurrentStock * p.LastPurchasePrice) })
            .OrderByDescending(r => r.Value)
            .ToListAsync();

        var totalStockValue = stockValues.Sum(r => r.Value);

        var recipes = await _context.Recipes
            .Include(r => r.Ingredients).ThenInclude(i => i.Product)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Category).ThenBy(r => r.Name)
            .ToListAsync();

        return new ReportViewModel
        {
            From = from,
            To = to,
            TotalSpend = monthlySpends.Sum(r => r.Amount),
            TotalInventoryValue = totalStockValue,
            RecipeCount = recipes.Count,
            AvgFoodCostPct = recipes.Count > 0
                ? Math.Round(recipes.Average(r => r.FoodCostPercentage), 1) : 0,
            MonthlySpends = monthlySpends.Select(r => new MonthlySpendRow(r.Year, r.Month, r.Amount)).ToList(),
            VendorSpends = vendorSpends.Select(r => new VendorSpendRow(
                r.VendorName ?? "—", r.Amount,
                totalVendorSpend > 0 ? Math.Round(r.Amount / totalVendorSpend * 100, 1) : 0)).ToList(),
            StockValues = stockValues.Select(r => new StockValueRow(
                r.Name ?? "—", r.ColorHex ?? "#6b7280", r.Value,
                totalStockValue > 0 ? Math.Round(r.Value / totalStockValue * 100, 1) : 0)).ToList(),
            RecipeCosts = recipes.Select(r => new RecipeCostRow(
                r.Name, r.Category ?? "—", r.TotalCost, r.SellingPrice, r.FoodCostPercentage)).ToList()
        };
    }
}
