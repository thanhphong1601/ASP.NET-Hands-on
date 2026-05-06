using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.IRepository;
using ASP.NET_Hands_on.Domain.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using ASP.NET_Hands_on.Application.Interface;

namespace ASP.NET_Hands_on.Application.CQRS.Orders
{
    public record GetOrdersQuery(int PageNumber, int PageSize) : IRequest<(List<Order> Items, int TotalCount)>;
    public record GetOrderByIdQuery(int OrderId) : IRequest<object>;
    public record CreateOrderCommand(Dictionary<int, int> ProductIdsAndQuantity, string Email, int CustomerId, string CustomerAddress) : IRequest<Order>;
    public record AddProductToOrderCommand(int OrderId, int ProductId, int Quantity) : IRequest<bool>;
    public record DeleteOrderCommand(int OrderId) : IRequest<bool>;

    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, (List<Order> Items, int TotalCount)>
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<GetOrdersQueryHandler> _logger;

        public GetOrdersQueryHandler(IOrderRepository repo, ILogger<GetOrdersQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<(List<Order> Items, int TotalCount)> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetOrdersQueryHandler - page {Page} size {Size}", request.PageNumber, request.PageSize);

            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 30 : request.PageSize;

            var total = await _repo.CountOrdersAsync(cancellationToken);
            var items = await _repo.GetOrdersPagedAsync(pageNumber, pageSize, cancellationToken);

            return (items, total);
        }
    }

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, object>
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        public GetOrderByIdQueryHandler(IOrderRepository repo, ILogger<GetOrderByIdQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<object> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetOrderByIdQueryHandler - orderId: {OrderId}", request.OrderId);

            var order = await _repo.GetByIdWithIncludesAsync(request.OrderId, cancellationToken);
            if (order == null) throw new KeyNotFoundException($"Order with id {request.OrderId} was not found.");

            var products = order.OrderProducts
                .Select(op => new ProductDto(op.Product.Id, op.Product.ProductId, op.Product.Name ?? string.Empty, op.Product.Price))
                .ToList();

            var orderDetails = new OrderDetailDto(order.OrderId, order.OrderDate, order.TotalPrice, products, order.Customer?.Name);
            return orderDetails;
        }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Order>
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IEmailService _emailService;

        public CreateOrderCommandHandler(IOrderRepository repo, ILogger<CreateOrderCommandHandler> logger, IBackgroundTaskQueue queue, IEmailService emailService)
        {
            _repo = repo;
            _logger = logger;
            _queue = queue;
            _emailService = emailService;
        }

        public async Task<Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("CreateOrderCommandHandler - creating order with {Count} productIds", request.ProductIdsAndQuantity?.Count ?? 0);

            if (request.ProductIdsAndQuantity == null || request.ProductIdsAndQuantity.Count == 0)
                throw new ArgumentException("There is no products found");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (!_emailService.CheckValidEmail(request.Email))
                throw new ArgumentException("Invalid email address");

            if (string.IsNullOrWhiteSpace(request.CustomerAddress))
                throw new ArgumentException("Email is required");

            var newOrder = new Order {};
            if (request is object)
            {
                // if CreateOrderCommand carries customer id, set it
                var custId = (request as CreateOrderCommand)?.CustomerId;
                if (custId.HasValue) newOrder.CustomerId = custId.Value;
            }
            await _repo.AddOrderAsync(newOrder, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            var validProductIds = request.ProductIdsAndQuantity.Keys.ToList();
            var validProducts = await _repo.GetProductsByIdsAsync(validProductIds, cancellationToken);

            foreach (var kv in request.ProductIdsAndQuantity)
            {
                var productId = kv.Key;
                var qty = kv.Value;

                if (qty <= 0)
                    continue;

                if (!validProducts.ContainsKey(productId))
                    continue;

                await _repo.AddOrderProductAsync(new OrderProduct
                {
                    OrderId = newOrder.OrderId,
                    ProductId = productId,
                    Quantity = qty
                }, cancellationToken);
            }

            await _repo.SaveChangesAsync(cancellationToken);

            var totalPrice = await _repo.CalculateTotalPriceAsync(newOrder.OrderId, cancellationToken);
            var order = await _repo.GetByIdAsync(newOrder.OrderId, cancellationToken);
            order.Address = request.CustomerAddress;
            if (order != null)
            {
                order.TotalPrice = totalPrice;
                await _repo.SaveChangesAsync(cancellationToken);
            }

            // enqueue email job
            //var emailJob = new EmailJob
            //{
            //    To = request.Email,
            //    Subject = $"Order Confirmation #{newOrder.OrderId}",
            //    Body = $"Your order {newOrder.OrderId} has been created. Total: {newOrder.TotalPrice}"
            //};

            //await _queue.QueueEmailAsync(emailJob);

            _logger.LogInformation("CreateOrderCommandHandler - created order {OrderId}", newOrder.OrderId);
            return newOrder;
        }
    }

    public class AddProductToOrderCommandHandler : IRequestHandler<AddProductToOrderCommand, bool>
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<AddProductToOrderCommandHandler> _logger;

        public AddProductToOrderCommandHandler(IOrderRepository repo, ILogger<AddProductToOrderCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<bool> Handle(AddProductToOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("AddProductToOrderCommandHandler - orderId: {OrderId}, productId: {ProductId}, qty: {Qty}", request.OrderId, request.ProductId, request.Quantity);

            var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null) throw new KeyNotFoundException($"Order with id {request.OrderId} was not found.");

            var product = await _repo.GetProductByIdAsync(request.ProductId, cancellationToken);
            if (product == null) throw new KeyNotFoundException($"Product with id {request.ProductId} was not found.");

            var record = await _repo.GetOrderProductAsync(request.OrderId, request.ProductId, cancellationToken);
            if (record != null)
            {
                record.Quantity += request.Quantity;
                await _repo.UpdateOrderProductAsync(record, cancellationToken);
            }
            else
            {
                await _repo.AddOrderProductAsync(new OrderProduct
                {
                    OrderId = request.OrderId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                }, cancellationToken);
            }

            await _repo.SaveChangesAsync(cancellationToken);

            // recalculate total
            var total = await _repo.CalculateTotalPriceAsync(request.OrderId, cancellationToken);
            var o = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
            if (o != null)
            {
                o.TotalPrice = total;
                await _repo.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("AddProductToOrderCommandHandler - added product to order {OrderId}", request.OrderId);
            return true;
        }
    }

    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IOrderRepository _repo;
        private readonly ILogger<DeleteOrderCommandHandler> _logger;

        public DeleteOrderCommandHandler(IOrderRepository repo, ILogger<DeleteOrderCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DeleteOrderCommandHandler - deleting order {OrderId}", request.OrderId);

            var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("DeleteOrderCommandHandler - order {OrderId} not found", request.OrderId);
                return false;
            }

            await _repo.RemoveOrderProductsByOrderIdAsync(request.OrderId, cancellationToken);
            await _repo.DeleteOrderAsync(order, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("DeleteOrderCommandHandler - deleted order {OrderId}", request.OrderId);
            return true;
        }
    }
}
