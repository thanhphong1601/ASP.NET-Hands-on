using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Domain.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET_Hands_on.Application.IRepository
{
    public interface IProductRepository
    {
        Task<int> CountAsync(CancellationToken cancellationToken);
        Task<List<ProductDto>> GetPagedProductDtosAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<List<ProductDto>> SearchByNameOrProductIdAsync(string keyword, CancellationToken cancellationToken);
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task AddRangeAsync(List<Product> products, CancellationToken cancellationToken);
        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
        Task RemoveAsync(Product product, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
