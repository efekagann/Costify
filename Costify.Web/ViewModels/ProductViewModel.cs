using System.ComponentModel.DataAnnotations;
using Costify.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Costify.Web.ViewModels;

public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(200)]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Stok Kodu (SKU)")]
    public string? SKU { get; set; }

    [Required(ErrorMessage = "Kategori seçiniz.")]
    [Display(Name = "Kategori")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Birim seçiniz.")]
    [Display(Name = "Birim")]
    public int UnitId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Mevcut Stok")]
    public decimal CurrentStock { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Minimum Stok Uyarı Sınırı")]
    public decimal MinimumStock { get; set; }

    [Display(Name = "Son Alış Fiyatı (₺)")]
    public decimal LastPurchasePrice { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    // Dropdown listeler
    public SelectList? Categories { get; set; }
    public SelectList? Units { get; set; }

    public static ProductViewModel FromEntity(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        SKU = p.SKU,
        CategoryId = p.CategoryId,
        UnitId = p.UnitId,
        CurrentStock = p.CurrentStock,
        MinimumStock = p.MinimumStock,
        LastPurchasePrice = p.LastPurchasePrice,
        IsActive = p.IsActive
    };
}
