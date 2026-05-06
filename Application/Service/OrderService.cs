using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Domain.Model;
using Microsoft.Extensions.Logging;
using System.Threading;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.IRepository;

namespace ASP.NET_Hands_on.Application.Service
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderRepository _orderRepository;

        public OrderService(ILogger<OrderService> logger, IOrderRepository orderRepository)
        {
            _logger = logger;
            _orderRepository = orderRepository;
        }
        //Need DTO productRequest (productId, quantity) to create order_product entry in the database, and also to calculate total price of the order
        public async Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.AddProductToOrderAsync - orderId: {OrderId}, productId: {ProductId}, qty: {Qty}", orderId, productId, quantity);

            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null) throw new KeyNotFoundException($"Order with id {orderId} was not found.");

            var product = await _orderRepository.GetProductByIdAsync(productId, cancellationToken);
            if (product == null) throw new KeyNotFoundException($"Product with id {productId} was not found.");

            var record = await _orderRepository.GetOrderProductAsync(orderId, productId, cancellationToken);
            if (record != null)
            {
                record.Quantity += quantity;
                await _orderRepository.UpdateOrderProductAsync(record, cancellationToken);
            }
            else
            {
                await _orderRepository.AddOrderProductAsync(new OrderProduct
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity
                }, cancellationToken);
            }

            await _orderRepository.SaveChangesAsync(cancellationToken);
            await CalculateTotalPriceWhenAddOrRemoveProduct(orderId);

            _logger.LogInformation("OrderService.AddProductToOrderAsync - added product to order {OrderId}", orderId);
            return true;
        }

        public async Task<Order> CreateOrderAsync(List<int> productIds, string email, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.CreateOrderAsync - creating order with {Count} productIds", productIds?.Count ?? 0);

            if (productIds == null || productIds.Count == 0)
                throw new ArgumentException("There is no products found");

            var productQuantities = productIds.GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            var validProductIds = productQuantities.Keys.ToList();

            var validProducts = await _orderRepository.GetProductsByIdsAsync(validProductIds, cancellationToken);

            if (validProducts.Count == 0)
                throw new ArgumentException("All provided products are invalid");

            var newOrder = new Order();

            foreach (var kv in productQuantities)
            {
                var productId = kv.Key;
                var qty = kv.Value;

                if (!validProducts.TryGetValue(productId, out var product))
                    continue; 

                // Thêm trực tiếp OrderProduct vào list của Order cha
                newOrder.OrderProducts.Add(new OrderProduct
                {
                    ProductId = productId,
                    Quantity = qty
                    // TUYỆT ĐỐI KHÔNG gán OrderId = newOrder.OrderId ở đây.
                    // EF Core sẽ tự động biết và gán ID sau khi Insert Order thành công!
                });

                // TỐI ƯU HÓA: Tính luôn tổng tiền ngay trên RAM, 
                // không cần gọi hàm CalculateTotalPrice... tốn thêm 1 nhịp query DB nữa.
                newOrder.TotalPrice += (product.Price * qty); 
            }

            await _orderRepository.AddOrderAsync(newOrder, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("OrderService.CreateOrderAsync - created order {OrderId}", newOrder.OrderId);

            return newOrder;
        }

        public async Task<object> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.GetOrderByIdAsync - orderId: {OrderId}", orderId);

            var order = await _orderRepository.GetByIdWithIncludesAsync(orderId, cancellationToken);

            if (order == null) throw new KeyNotFoundException($"Order with id {orderId} was not found.");

            var products = order.OrderProducts
                .Select(op => new ProductDto(op.Product.Id, op.Product.ProductId, op.Product.Name ?? string.Empty, op.Product.Price))
                .ToList();

            var orderDetails = new DTO.OrderDetailDto(order.OrderId, order.OrderDate, order.TotalPrice, products, order.Customer?.Name);

            return orderDetails;
        }

        public async Task<(List<Order> Items, int TotalCount)> GetOrdersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.GetOrdersAsync - retrieving paged orders page {Page} size {Size}", pageNumber, pageSize);

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 30;

            var totalCount = await _orderRepository.CountOrdersAsync(cancellationToken);
            var items = await _orderRepository.GetOrdersPagedAsync(pageNumber, pageSize, cancellationToken);

            return (items, totalCount);
        }

        public async Task CalculateTotalPriceWhenAddOrRemoveProduct(int orderId)
        {
            var totalPrice = await _orderRepository.CalculateTotalPriceAsync(orderId, CancellationToken.None);
            var order = await _orderRepository.GetByIdAsync(orderId, CancellationToken.None);
            if (order != null)
            {
                order.TotalPrice = totalPrice;
                await _orderRepository.SaveChangesAsync(CancellationToken.None);
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.DeleteOrderAsync - deleting order {OrderId}", orderId);

            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("OrderService.DeleteOrderAsync - order {OrderId} not found", orderId);
                return false;
            }

            await _orderRepository.RemoveOrderProductsByOrderIdAsync(orderId, cancellationToken);
            await _orderRepository.DeleteOrderAsync(order, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("OrderService.DeleteOrderAsync - deleted order {OrderId}", orderId);
            return true;
        }
    }
}
