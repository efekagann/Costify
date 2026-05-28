namespace Costify.Domain.Enums;

public enum OrderStatus
{
    Draft = 0,      // Taslak
    Ordered = 1,    // Sipariş Verildi
    Received = 2,   // Teslim Alındı
    PartiallyReceived = 3,  // Kısmen Teslim Alındı
    Cancelled = 4   // İptal Edildi
}
