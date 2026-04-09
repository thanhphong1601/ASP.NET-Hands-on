using ASP.NET_Hands_on.Enum;
using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.Interface
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken);
        Task<object> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken);
        Task<Order> CreateOrderAsync(List<int> productIds, CancellationToken cancellationToken);
        Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity, CancellationToken cancellationToken);
        Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken);
        void CalculateTotalPriceWhenAddOrRemoveProduct(int orderId);
    }
}
