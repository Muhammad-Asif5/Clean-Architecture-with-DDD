using YourApp.Domain.Entities;

namespace YourApp.Domain.Interfaces
{
    public interface IApplicationDbContext
    {
        IQueryable<Product> Products { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}