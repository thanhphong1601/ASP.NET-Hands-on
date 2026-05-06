using ASP.NET_Hands_on.Domain.Model;
using ASP.NET_Hands_on.DatabseContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Application.IRepository;

namespace ASP.NET_Hands_on.Persistence.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken)
            => _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        public Task<Order?> GetByIdWithIncludesAsync(int orderId, CancellationToken cancellationToken)
            => _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

        public Task AddOrderAsync(Order order, CancellationToken cancellationToken)
            => _db.Orders.AddAsync(order, cancellationToken).AsTask();

        public Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
            => _db.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        public Task<OrderProduct?> GetOrderProductAsync(int orderId, int productId, CancellationToken cancellationToken)
            => _db.OrderProducts.FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId, cancellationToken);

        public Task AddOrderProductAsync(OrderProduct orderProduct, CancellationToken cancellationToken)
            => _db.OrderProducts.AddAsync(orderProduct, cancellationToken).AsTask();

        public Task UpdateOrderProductAsync(OrderProduct orderProduct, CancellationToken cancellationToken)
        {
            _db.OrderProducts.Update(orderProduct);
            return Task.CompletedTask;
        }

        public async Task<Dictionary<int, Product>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var list = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
            return list.ToDictionary(p => p.Id, p => p);
        }

        public Task<int> CountOrdersAsync(CancellationToken cancellationToken)
            => _db.Orders.AsNoTracking().CountAsync(cancellationToken);

        public Task<List<Order>> GetOrdersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 30;

            return _db.Orders
                .AsNoTracking()
                .OrderBy(o => o.OrderId)
                .Where(o => o.isDeleted == false)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public Task<decimal> CalculateTotalPriceAsync(int orderId, CancellationToken cancellationToken)
        {
            return _db.OrderProducts
                .Where(op => op.OrderId == orderId)
                .Join(_db.Products, op => op.ProductId, p => p.Id, (op, p) => op.Quantity * p.Price)
                .SumAsync(cancellationToken: cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
            => _db.SaveChangesAsync(cancellationToken);

        public Task DeleteOrderAsync(Order order, CancellationToken cancellationToken)
        {
            _db.Orders.Remove(order);
            return Task.CompletedTask;
        }

        public Task RemoveOrderProductsByOrderIdAsync(int orderId, CancellationToken cancellationToken)
        {
            var related = _db.OrderProducts.Where(op => op.OrderId == orderId);
            _db.OrderProducts.RemoveRange(related);
            return Task.CompletedTask;
        }
    }
}
