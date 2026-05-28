namespace Costify.Domain.Enums;

public enum StockTransactionType
{
    Purchase = 1,   // Satın Alma (Giriş)
    Usage = 2,      // Kullanım/Sarfiyat (Çıkış)
    Adjustment = 3, // Sayım Düzeltmesi
    Waste = 4,      // Fire/Kayıp
    Return = 5      // İade
}
