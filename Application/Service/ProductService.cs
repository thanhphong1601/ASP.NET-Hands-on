using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Domain.Model;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Application.IRepository;

namespace ASP.NET_Hands_on.Application.Service
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly IProductRepository _productRepository;

        public ProductService(ILogger<ProductService> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<(List<ProductDto> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.GetAllAsync - retrieving paged products page {Page} size {Size}", pageNumber, pageSize);

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 30;

            var totalCount = await _productRepository.CountAsync(cancellationToken);
            var items = await _productRepository.GetPagedProductDtosAsync(pageNumber, pageSize, cancellationToken);
            //                .Select(p => new ProductDto(p.ProductId, p.Name, p.Price))

            return (items, totalCount);
        }

        public async Task<List<ProductDto>> SearchByNameOrProductIdAsync(string keyword, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.SearchByNameOrProductIdAsync - keyword: {Keyword}", keyword);
            if (string.IsNullOrWhiteSpace(keyword)) return new List<ProductDto>();

            return await _productRepository.SearchByNameOrProductIdAsync(keyword, cancellationToken);
        }

        public async Task<Product> CreateAsync(Product newProduct, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.CreateAsync - creating product {ProductId} {Name}", newProduct.ProductId, newProduct.Name);
            await _productRepository.AddAsync(newProduct, cancellationToken);
            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("ProductService.CreateAsync - created id {Id}", newProduct.Id);
            return newProduct;
        }

        public async Task<List<Product>> CreateManyAsync(List<Product> productList, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.CreateManyAsync - creating {Count} products", productList?.Count ?? 0);
            await _productRepository.AddRangeAsync(productList, cancellationToken);
            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("ProductService.CreateManyAsync - created {Count} products", productList.Count);
            return productList;
        }

        public async Task<Product> UpdateAsync(int id, Product updateData, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.UpdateAsync - updating id {Id}", id);
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) throw new KeyNotFoundException("Product not found");

            product.Name = updateData.Name;
            product.ProductId = updateData.ProductId;
            product.Price = updateData.Price;

            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("ProductService.UpdateAsync - updated id {Id}", id);
            return product;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.DeleteAsync - deleting id {Id}", id);
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) throw new KeyNotFoundException("Product not found");

            await _productRepository.RemoveAsync(product, cancellationToken);
            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("ProductService.DeleteAsync - deleted id {Id}", id);
        }

        public async Task<Product> PatchAsync(int id, ProductPatchRequest patchRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProductService.PatchAsync - patching id {Id} field {Field}", id, patchRequest?.FieldName);
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null) throw new KeyNotFoundException("Product not found");

            if (patchRequest == null) throw new ArgumentNullException(nameof(patchRequest));
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

            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("ProductService.PatchAsync - patched id {Id} field {Field}", id, field);
            return product;
        }
    }
}
