namespace Costify.Domain.Entities.Base;

/// <summary>
/// Multi-tenant yapı için tüm işletmeye ait entity'lerin uyguladığı arayüz.
/// Global Query Filter bu interface üzerinden çalışır.
/// </summary>
public interface IBusinessEntity
{
    int BusinessId { get; set; }
}
