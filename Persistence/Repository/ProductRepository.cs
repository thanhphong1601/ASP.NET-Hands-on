using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Model;
using ASP.NET_Hands_on.Persistence.Interface;
using ASP.NET_Hands_on.DatabseContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET_Hands_on.Persistence.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<int> CountAsync(CancellationToken cancellationToken)
            => _db.Products.AsNoTracking().CountAsync(cancellationToken);

        public Task<List<ProductDto>> GetPagedProductDtosAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 30;

            return _db.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto(p.ProductId, p.Name, p.Price))
                .ToListAsync(cancellationToken);
        }

        public Task<List<ProductDto>> SearchByNameOrProductIdAsync(string keyword, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return Task.FromResult(new List<ProductDto>());
            var q = keyword.Trim();
            return _db.Products
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Name, $"%{q}%") || EF.Functions.Like(p.ProductId, $"%{q}%"))
                .Select(p => new ProductDto(p.ProductId, p.Name, p.Price))
                .ToListAsync(cancellationToken);
        }

        public Task AddAsync(Product product, CancellationToken cancellationToken)
            => _db.Products.AddAsync(product, cancellationToken).AsTask();

        public Task AddRangeAsync(List<Product> products, CancellationToken cancellationToken)
            => _db.Products.AddRangeAsync(products, cancellationToken).AsTask();

        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
            => _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        public Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
            => _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

        public Task RemoveAsync(Product product, CancellationToken cancellationToken)
        {
            _db.Products.Remove(product);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => _db.SaveChangesAsync(cancellationToken);
    }
}
