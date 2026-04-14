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
        private readonly AppDbContext _db;

        public OrderService(ILogger<OrderService> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }
        //Need DTO productRequest (productId, quantity) to create order_product entry in the database, and also to calculate total price of the order
        public async Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.AddProductToOrderAsync - orderId: {OrderId}, productId: {ProductId}, qty: {Qty}", orderId, productId, quantity);

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
            if (order == null) throw new KeyNotFoundException($"Order with id {orderId} was not found.");

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product == null) throw new KeyNotFoundException($"Product with id {productId} was not found.");

            var record = await _db.OrderProducts.FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId, cancellationToken);
            if (record != null)
            {
                record.Quantity += quantity;
                _db.OrderProducts.Update(record);
            }
            else
            {
                await _db.OrderProducts.AddAsync(new OrderProduct
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity
                }, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
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
            await _db.Orders.AddAsync(newOrder, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var validProductIds = productQuantities.Keys.ToList();
            var validProducts = await _db.Products
                .Where(p => validProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            // create order-product records
            foreach (var kv in productQuantities)
            {
                var productId = kv.Key;
                var qty = kv.Value;

                if (!validProducts.ContainsKey(productId))
                    continue; // ignore invalid product ids

                await _db.OrderProducts.AddAsync(new OrderProduct
                {
                    OrderId = newOrder.OrderId,
                    ProductId = productId,
                    Quantity = qty
                }, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
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

            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

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

            var totalCount = await _db.Orders.AsNoTracking().CountAsync(cancellationToken);

            var items = await _db.Orders
                .AsNoTracking()
                .OrderBy(o => o.OrderId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task CalculateTotalPriceWhenAddOrRemoveProduct(int orderId)
        {
            decimal totalPrice = await _db.OrderProducts
                .Where(op => op.OrderId == orderId)
                .Join(_db.Products, op => op.ProductId, p => p.Id, (op, p) => op.Quantity * p.Price)
                .SumAsync();

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order != null)
            {
                order.TotalPrice = totalPrice;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("OrderService.DeleteOrderAsync - deleting order {OrderId}", orderId);

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("OrderService.DeleteOrderAsync - order {OrderId} not found", orderId);
                return false;
            }

            // remove related order-product entries
            var related = _db.OrderProducts.Where(op => op.OrderId == orderId);
            _db.OrderProducts.RemoveRange(related);

            // remove order
            _db.Orders.Remove(order);

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("OrderService.DeleteOrderAsync - deleted order {OrderId}", orderId);
            return true;
        }
    }
}
