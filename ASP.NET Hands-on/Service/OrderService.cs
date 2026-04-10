using ASP.NET_Hands_on.ClientSideDatabase;
using ASP.NET_Hands_on.Enum;
using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Model;
using Microsoft.Extensions.Logging;

namespace ASP.NET_Hands_on.Service
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;

        public OrderService(ILogger<OrderService> logger)
        {
            _logger = logger;
        }
        //Need DTO productRequest (productId, quantity) to create order_product entry in the database, and also to calculate total price of the order
        public async Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.AddProductToOrderAsync - orderId: {OrderId}, productId: {ProductId}, qty: {Qty}", orderId, productId, quantity);
            await Task.Delay(200, cancellationToken); // Simulate async work

            Order? order = MockDatabase.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
                throw new KeyNotFoundException($"Order with id {orderId} was not found.");

            Product? product = MockDatabase.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with id {productId} was not found.");

            var record = MockDatabase.OrderProducts.FirstOrDefault(op => op.OrderId == orderId && op.ProductId == productId);
            //if there is a record of that product
            if (record is not null)
            {
                record.Quantity += quantity;
            } else
            {
                order.Products.Add(product);
                MockDatabase.OrderProducts.Add(new Order_Product
                {
                    Id = MockDatabase.OrderProductIdCounter++,
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            CalculateTotalPriceWhenAddOrRemoveProduct(orderId);

            _logger.LogInformation("OrderService.AddProductToOrderAsync - added product to order {OrderId}", orderId);
            return true;
        }

       

        public async Task<Order> CreateOrderAsync(List<int> productIds, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.CreateOrderAsync - creating order with {Count} productIds", productIds?.Count ?? 0);
            await Task.Delay(200, cancellationToken); // Simulate async work

            if (productIds == null || productIds.Count == 0)
                throw new ArgumentException("There is no products found");

            // a list contains products' key and quantity
            var productQuantities = productIds.GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            // find valid products using keys from the productQuantities list, if the product is not found, it will be ignored
            var validProducts = MockDatabase.Products
                .Where(p => productQuantities.ContainsKey(p.Id))
                .ToList();

            Order newOrder = new Order { 
                OrderId = MockDatabase.OrderIdCounter++,
                Products = validProducts
            };

            // create order_product entries for each valid product in the order
            foreach (var product in validProducts)
            {
                int qty = productQuantities[product.Id];

                MockDatabase.OrderProducts.Add(new Order_Product
                {
                    Id = MockDatabase.OrderProductIdCounter++,
                    OrderId = newOrder.OrderId,
                    ProductId = product.Id,
                    Quantity = qty
                });
            }

            MockDatabase.Orders.Add(newOrder);
            CalculateTotalPriceWhenAddOrRemoveProduct(newOrder.OrderId);

            _logger.LogInformation("OrderService.CreateOrderAsync - created order {OrderId}", newOrder.OrderId);
            return newOrder;
        }

        public async Task<object> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.GetOrderByIdAsync - orderId: {OrderId}", orderId);
            var order = MockDatabase.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with id {orderId} was not found.");

            await Task.Delay(200, cancellationToken); // Simulate async work

            var orderDetails = new
            {
                orderId = order.OrderId,
                totalPrice = order.TotalPrice,
                products = MockDatabase.OrderProducts
                    .Where(op => op.OrderId == order.OrderId)
                    .Join(MockDatabase.Products,
                        op => op.ProductId,
                        p => p.Id,
                        (op, p) => new
                        {
                            productId = p.ProductId,
                            price = p.Price,
                            quantity = op.Quantity
                        })
                    .ToList()
            };

            return orderDetails;
        }

        public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.GetOrdersAsync - retrieving all orders");
            await Task.Delay(200, cancellationToken); // Simulate async work

            var orders = MockDatabase.Orders;
            if (orders.Count == 0)
                return new List<Order>();

            return orders;
        }

        public void CalculateTotalPriceWhenAddOrRemoveProduct(int orderId)
        {
            decimal totalPrice = MockDatabase.OrderProducts
                .Where(op => op.OrderId == orderId)
                .Join(MockDatabase.Products, op => op.ProductId, p => p.Id, (op, p) => op.Quantity * p.Price)
                .Sum();

            Order? order = MockDatabase.Orders.FirstOrDefault(o => o.OrderId == orderId);
            order?.TotalPrice = totalPrice;
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.DeleteOrderAsync - deleting order {OrderId}", orderId);
            await Task.Delay(100, cancellationToken);

            var order = MockDatabase.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                _logger.LogWarning("OrderService.DeleteOrderAsync - order {OrderId} not found", orderId);
                return false;
            }

            // remove related order-product entries
            MockDatabase.OrderProducts.RemoveAll(op => op.OrderId == orderId);

            // remove order
            MockDatabase.Orders.Remove(order);

            _logger.LogInformation("OrderService.DeleteOrderAsync - deleted order {OrderId}", orderId);
            return true;
        }
    }
}
