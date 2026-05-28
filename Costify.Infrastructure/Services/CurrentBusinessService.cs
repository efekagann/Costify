using Microsoft.AspNetCore.Http;

namespace Costify.Infrastructure.Services;

/// <summary>
/// HTTP oturumundan aktif işletme ID'sini çözer.
/// İleride JWT claim veya subdomain'den de okunabilir.
/// </summary>
public class CurrentBusinessService : ICurrentBusinessService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentBusinessService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int BusinessId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("BusinessId")?.Value;

            if (int.TryParse(claim, out var id))
                return id;

            // Tek işletmeli MVP için varsayılan değer; SaaS'a geçişte kaldırılır
            return 1;
        }
    }
}
