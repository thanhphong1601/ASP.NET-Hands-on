using ASP.NET_Hands_on.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET_Hands_on.Persistence.Interface
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken);
        Task<Order?> GetByIdWithIncludesAsync(int orderId, CancellationToken cancellationToken);
        Task AddOrderAsync(Order order, CancellationToken cancellationToken);
        Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
        Task<OrderProduct?> GetOrderProductAsync(int orderId, int productId, CancellationToken cancellationToken);
        Task AddOrderProductAsync(OrderProduct orderProduct, CancellationToken cancellationToken);
        Task UpdateOrderProductAsync(OrderProduct orderProduct, CancellationToken cancellationToken);
        Task<Dictionary<int, Product>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
        Task<int> CountOrdersAsync(CancellationToken cancellationToken);
        Task<List<Order>> GetOrdersPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<decimal> CalculateTotalPriceAsync(int orderId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
        Task DeleteOrderAsync(Order order, CancellationToken cancellationToken);
        Task RemoveOrderProductsByOrderIdAsync(int orderId, CancellationToken cancellationToken);
    }
}
