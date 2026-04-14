using ASP.NET_Hands_on.Enum;
using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.Interface
{
    public interface IOrderService
    {
        // returns paged orders and total count
        Task<(List<Order> Items, int TotalCount)> GetOrdersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<object> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken);
        Task<Order> CreateOrderAsync(List<int> productIds, CancellationToken cancellationToken);
        Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity, CancellationToken cancellationToken);
        Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken);
        Task CalculateTotalPriceWhenAddOrRemoveProduct(int orderId);
    }
}
