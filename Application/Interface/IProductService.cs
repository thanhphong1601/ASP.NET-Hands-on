using ASP.NET_Hands_on.Domain.Model;
using ASP.NET_Hands_on.Application.DTO;
using System.Threading;

namespace ASP.NET_Hands_on.Application.Interface
{
    public interface IProductService
    {
        // Returns paged items and total count
        Task<(List<ProductDto> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<List<ProductDto>> SearchByNameOrProductIdAsync(string keyword, CancellationToken cancellationToken);
        Task<Product> CreateAsync(Product newProduct, CancellationToken cancellationToken);
        Task<List<Product>> CreateManyAsync(List<Product> productList, CancellationToken cancellationToken);
        Task<Product> UpdateAsync(int id, Product updateData, CancellationToken cancellationToken);
        Task<Product> PatchAsync(int id, ProductPatchRequest patchRequest, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
