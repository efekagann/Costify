using Costify.Domain.Entities;

namespace Costify.Domain.Interfaces.Repositories;

public interface IVendorRepository : IRepository<Vendor>
{
    Task<Vendor?> GetWithOrdersAsync(int id);
}
