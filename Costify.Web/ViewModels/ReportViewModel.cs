namespace Costify.Web.ViewModels;

public class ReportViewModel
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int RecipeCount { get; set; }
    public decimal AvgFoodCostPct { get; set; }

    public List<MonthlySpendRow> MonthlySpends { get; set; } = [];
    public List<VendorSpendRow> VendorSpends { get; set; } = [];
    public List<StockValueRow> StockValues { get; set; } = [];
    public List<RecipeCostRow> RecipeCosts { get; set; } = [];
}

public record MonthlySpendRow(int Year, int Month, decimal Amount);
public record VendorSpendRow(string VendorName, decimal Amount, decimal Percentage);
public record StockValueRow(string CategoryName, string ColorHex, decimal Value, decimal Percentage);
public record RecipeCostRow(string Name, string Category, decimal Cost, decimal SellingPrice, decimal FoodCostPct);
