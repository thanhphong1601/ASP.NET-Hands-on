using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.IRepository;
using ASP.NET_Hands_on.Domain.Model;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ASP.NET_Hands_on.Application.CQRS.Products
{
    // Queries and Commands
    public record GetAllProductsQuery(int PageNumber, int PageSize) : IRequest<(List<ProductDto> Items, int TotalCount)>;
    public record SearchProductsQuery(string Keyword) : IRequest<List<ProductDto>>;

    public record CreateProductCommand(Product NewProduct) : IRequest<Product>;
    public record CreateManyProductsCommand(List<Product> Products) : IRequest<List<Product>>;
    public record UpdateProductCommand(int Id, Product UpdateData) : IRequest<Product>;
    public record PatchProductCommand(int Id, ProductPatchRequest PatchRequest) : IRequest<Product>;
    public record DeleteProductCommand(int Id) : IRequest<Unit>;

    // Handlers
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, (List<ProductDto> Items, int TotalCount)>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;

        public GetAllProductsQueryHandler(IProductRepository repo, ILogger<GetAllProductsQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<(List<ProductDto> Items, int TotalCount)> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 30 : request.PageSize;

            _logger.LogInformation("GetAllProductsQueryHandler - page {Page} size {Size}", pageNumber, pageSize);

            var total = await _repo.CountAsync(cancellationToken);
            var items = await _repo.GetPagedProductDtosAsync(pageNumber, pageSize, cancellationToken);

            return (items, total);
        }
    }

    public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, List<ProductDto>>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<SearchProductsQueryHandler> _logger;

        public SearchProductsQueryHandler(IProductRepository repo, ILogger<SearchProductsQueryHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public Task<List<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("SearchProductsQueryHandler - keyword: {Keyword}", request.Keyword);
            if (string.IsNullOrWhiteSpace(request.Keyword)) return Task.FromResult(new List<ProductDto>());
            return _repo.SearchByNameOrProductIdAsync(request.Keyword, cancellationToken);
        }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Product>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(IProductRepository repo, ILogger<CreateProductCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var newProduct = request.NewProduct ?? throw new ArgumentNullException(nameof(request.NewProduct));
            _logger.LogInformation("CreateProductCommandHandler - creating product {ProductId} {Name}", newProduct.ProductId, newProduct.Name);
            await _repo.AddAsync(newProduct, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("CreateProductCommandHandler - created id {Id}", newProduct.Id);
            return newProduct;
        }
    }

    public class CreateManyProductsCommandHandler : IRequestHandler<CreateManyProductsCommand, List<Product>>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<CreateManyProductsCommandHandler> _logger;

        public CreateManyProductsCommandHandler(IProductRepository repo, ILogger<CreateManyProductsCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<Product>> Handle(CreateManyProductsCommand request, CancellationToken cancellationToken)
        {
            var list = request.Products ?? new List<Product>();
            _logger.LogInformation("CreateManyProductsCommandHandler - creating {Count} products", list.Count);
            await _repo.AddRangeAsync(list, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("CreateManyProductsCommandHandler - created {Count} products", list.Count);
            return list;
        }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Product>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(IProductRepository repo, ILogger<UpdateProductCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Product> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("UpdateProductCommandHandler - updating id {Id}", request.Id);
            var product = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (product == null) throw new KeyNotFoundException("Product not found");

            product.Name = request.UpdateData.Name;
            product.ProductId = request.UpdateData.ProductId;
            product.Price = request.UpdateData.Price;

            await _repo.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("UpdateProductCommandHandler - updated id {Id}", request.Id);
            return product;
        }
    }

    public class PatchProductCommandHandler : IRequestHandler<PatchProductCommand, Product>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<PatchProductCommandHandler> _logger;

        public PatchProductCommandHandler(IProductRepository repo, ILogger<PatchProductCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Product> Handle(PatchProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PatchProductCommandHandler - patching id {Id} field {Field}", request.Id, request.PatchRequest?.FieldName);
            var product = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (product == null) throw new KeyNotFoundException("Product not found");

            var patchRequest = request.PatchRequest ?? throw new ArgumentNullException(nameof(request.PatchRequest));
            var field = patchRequest.FieldName?.Trim();
            var newValue = patchRequest.NewValue?.Trim();

            if (string.IsNullOrEmpty(field))
                throw new ArgumentException("FieldName is required.");
            if (newValue == null)
                throw new ArgumentException("NewValue is required.");

            switch (field)
            {
                case "productId":
                    product.ProductId = newValue;
                    break;
                case "name":
                    product.Name = newValue;
                    break;
                case "price":
                    if (!decimal.TryParse(newValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
                        throw new ArgumentException("NewValue for 'price' must be a valid decimal number.");
                    if (price < 0) throw new ArgumentException("Price must be non-negative.");
                    product.Price = price;
                    break;
                default:
                    throw new ArgumentException("Unsupported field.");
            }

            await _repo.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("PatchProductCommandHandler - patched id {Id} field {Field}", request.Id, field);
            return product;
        }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(IProductRepository repo, ILogger<DeleteProductCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DeleteProductCommandHandler - deleting id {Id}", request.Id);
            var product = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (product == null) throw new KeyNotFoundException("Product not found");

            await _repo.RemoveAsync(product, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("DeleteProductCommandHandler - deleted id {Id}", request.Id);
            return Unit.Value;
        }
    }
}
