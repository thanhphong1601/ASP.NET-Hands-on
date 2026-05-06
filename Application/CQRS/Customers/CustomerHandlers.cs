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
using System.Security.Cryptography;
using System.Text;

namespace ASP.NET_Hands_on.Application.CQRS.Customers
{
    public record GetCustomerQuery(int Id) : IRequest<CustomerDto?>;
    public record GetCustomersQuery(int PageNumber, int PageSize) : IRequest<(List<CustomerDto> Items, int TotalCount)>;
    public record CreateCustomerCommand(CustomerCreateRequest Request) : IRequest<Customer>;
    public record PatchCustomerCommand(int Id, CustomerPatchRequest Patch) : IRequest<Customer>;
    public record GetCustomerOrdersQuery(int CustomerId, int PageNumber, int PageSize) : IRequest<(List<OrderDetailDto> Items, int TotalCount)>;

    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerDto?>
    {
        private readonly ICustomerRepository _repo;
        private readonly ILogger<GetCustomerQueryHandler> _logger;

        public GetCustomerQueryHandler(ICustomerRepository repo, ILogger<GetCustomerQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<CustomerDto?> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            var c = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (c == null) return null;
            return new CustomerDto(c.Id, c.Name, c.Email, c.Gender);
        }
    }

    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, (List<CustomerDto> Items, int TotalCount)>
    {
        private readonly ICustomerRepository _repo;
        private readonly ILogger<GetCustomersQueryHandler> _logger;

        public GetCustomersQueryHandler(ICustomerRepository repo, ILogger<GetCustomersQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<(List<CustomerDto> Items, int TotalCount)> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber;
            var pageSize = request.PageSize;

            var total = await _repo.CountAsync(cancellationToken);
            var items = await _repo.GetPagedAsync(pageNumber, pageSize, cancellationToken);
            return (items, total);
        }
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Customer>
    {
        private readonly ICustomerRepository _repo;
        private readonly ILogger<CreateCustomerCommandHandler> _logger;

        public CreateCustomerCommandHandler(ICustomerRepository repo, ILogger<CreateCustomerCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var req = request.Request;
            // basic validation
            if (string.IsNullOrWhiteSpace(req.Username)) throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(req.Email)) throw new ArgumentException("Email is required");

            var existing = await _repo.GetByUsernameOrEmailAsync(req.Username, req.Email, cancellationToken);
            if (existing != null) throw new ArgumentException("Username or email already exists");

            var customer = new Customer
            {
                Name = req.Name,
                Username = req.Username,
                Email = req.Email,
                Address = req.Address,
                Gender = req.Gender
            };

            await _repo.AddAsync(customer, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            return customer;
        }
    }

    public class PatchCustomerCommandHandler : IRequestHandler<PatchCustomerCommand, Customer>
    {
        private readonly ICustomerRepository _repo;
        private readonly ILogger<PatchCustomerCommandHandler> _logger;

        public PatchCustomerCommandHandler(ICustomerRepository repo, ILogger<PatchCustomerCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Customer> Handle(PatchCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repo.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException("Customer not found");
            var patch = request.Patch;

            if (patch.Name != null) customer.Name = patch.Name;
            if (patch.Email != null) customer.Email = patch.Email;
            if (patch.Address != null) customer.Address = patch.Address;

            await _repo.UpdateAsync(customer, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            return customer;
        }
    }

    public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, (List<OrderDetailDto> Items, int TotalCount)>
    {
        private readonly ICustomerRepository _repo;

        public GetCustomerOrdersQueryHandler(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<(List<OrderDetailDto> Items, int TotalCount)> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
        {
            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 30 : request.PageSize;

            var total = await _repo.CountOrdersByCustomerIdAsync(request.CustomerId, cancellationToken);
            var orders = await _repo.GetOrdersByCustomerIdPagedAsync(request.CustomerId, page, size, cancellationToken);

            var result = orders.Select(o => new OrderDetailDto(o.OrderId, o.OrderDate, o.TotalPrice,
                o.OrderProducts.Select(op => new ProductDto(op.Product.Id, op.Product.ProductId, op.Product.Name ?? string.Empty, op.Product.Price)).ToList(),
                o.Customer?.Name
            )).ToList();

            return (result, total);
        }
    }
}
