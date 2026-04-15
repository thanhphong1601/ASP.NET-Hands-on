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
    public class DiscountDayRepository : IDiscountDayRepository
    {
        private readonly AppDbContext _db;

        public DiscountDayRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<DiscountDay>> GetAllWithProductsAsync(CancellationToken cancellationToken)
        {
            return _db.DiscountDays
                .AsNoTracking()
                .Include(dd => dd.DiscountDayProducts)
                    .ThenInclude(dpd => dpd.Product)
                .ToListAsync(cancellationToken);
        }

        public Task AddDiscountDayAsync(DiscountDay entity, CancellationToken cancellationToken)
            => _db.DiscountDays.AddAsync(entity, cancellationToken).AsTask();

        public Task<List<Product>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
            => _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

        public Task AddDiscountDayProductAsync(DiscountDayProduct ddp, CancellationToken cancellationToken)
            => _db.Set<DiscountDayProduct>().AddAsync(ddp, cancellationToken).AsTask();

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => _db.SaveChangesAsync(cancellationToken);
    }
}
