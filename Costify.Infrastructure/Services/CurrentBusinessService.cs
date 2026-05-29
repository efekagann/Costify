using Microsoft.AspNetCore.Http;

namespace Costify.Infrastructure.Services;

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

            return int.TryParse(claim, out var id) && id > 0 ? id : 1;
        }
    }
}
