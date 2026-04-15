using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET_Hands_on.Persistence.Interface
{
    public interface IDiscountDayRepository
    {
        Task<List<DiscountDay>> GetAllWithProductsAsync(CancellationToken cancellationToken);
        Task AddDiscountDayAsync(DiscountDay entity, CancellationToken cancellationToken);
        Task<List<Product>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
        Task AddDiscountDayProductAsync(DiscountDayProduct ddp, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
