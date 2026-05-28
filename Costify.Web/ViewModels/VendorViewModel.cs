using System.ComponentModel.DataAnnotations;
using Costify.Domain.Entities;

namespace Costify.Web.ViewModels;

public class VendorViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tedarikçi adı zorunludur.")]
    [MaxLength(200)]
    [Display(Name = "Firma Adı")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "İletişim Kişisi")]
    public string? ContactPerson { get; set; }

    [Phone]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [EmailAddress]
    [Display(Name = "E-Posta")]
    public string? Email { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Vergi Numarası")]
    public string? TaxNumber { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public decimal? CurrentBalance { get; set; }

    public static VendorViewModel FromEntity(Vendor v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        ContactPerson = v.ContactPerson,
        Phone = v.Phone,
        Email = v.Email,
        Address = v.Address,
        TaxNumber = v.TaxNumber,
        IsActive = v.IsActive,
        CurrentBalance = v.CurrentBalance
    };
}
