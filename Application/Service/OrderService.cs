using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Model;
using Microsoft.Extensions.Logging;
using ASP.NET_Hands_on.DatabseContext;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using ASP.NET_Hands_on.DTO;

namespace ASP.NET_Hands_on.Service
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly ASP.NET_Hands_on.Persistence.Interface.IOrderRepository _orderRepository;

        public OrderService(ILogger<OrderService> logger, ASP.NET_Hands_on.Persistence.Interface.IOrderRepository orderRepository)
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

            // group product ids to get quantities
            var productQuantities = productIds.GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            // validate product ids and create order
            var newOrder = new Order();
            await _orderRepository.AddOrderAsync(newOrder, cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            var validProductIds = productQuantities.Keys.ToList();
            var validProducts = await _orderRepository.GetProductsByIdsAsync(validProductIds, cancellationToken);

            // create order-product records
            foreach (var kv in productQuantities)
            {
                var productId = kv.Key;
                var qty = kv.Value;

                if (!validProducts.ContainsKey(productId))
                    continue; // ignore invalid product ids

                await _orderRepository.AddOrderProductAsync(new OrderProduct
                {
                    OrderId = newOrder.OrderId,
                    ProductId = productId,
                    Quantity = qty
                }, cancellationToken);
            }

            await _orderRepository.SaveChangesAsync(cancellationToken);
            await CalculateTotalPriceWhenAddOrRemoveProduct(newOrder.OrderId);

            var createdOrder = new OrderDetailDto(newOrder.OrderId, 
                newOrder.OrderDate, 
                newOrder.TotalPrice, 
                validProducts.Values.Select(p => new ProductDto(p.ProductId, p.Name ?? string.Empty, p.Price)).ToList());

            _logger.LogInformation("OrderService.CreateOrderAsync - created order {OrderId}", newOrder.OrderId);
            return newOrder;
        }

        public async Task<object> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.GetOrderByIdAsync - orderId: {OrderId}", orderId);

            var order = await _orderRepository.GetByIdWithIncludesAsync(orderId, cancellationToken);

            if (order == null) throw new KeyNotFoundException($"Order with id {orderId} was not found.");

            var products = order.OrderProducts
                .Select(op => new ProductDto(op.Product.ProductId, op.Product.Name ?? string.Empty, op.Product.Price))
                .ToList();

            var orderDetails = new DTO.OrderDetailDto(order.OrderId, order.OrderDate, order.TotalPrice, products);

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
