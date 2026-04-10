using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using ASP.NET_Hands_on.ClientSideDatabase;
using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Model;
using Microsoft.Extensions.Logging;

namespace ASP.NET_Hands_on.Service
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;

        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
        }
        public List<Product> GetAll()
        {
            _logger.LogInformation("ProductService.GetAll - retrieving all products");
            return MockDatabase.Products;
        }

        public List<Product> SearchByNameOrProductId(string keyword)
        {
            _logger.LogInformation("ProductService.Search - keyword: {Keyword}", keyword);
            if (string.IsNullOrWhiteSpace(keyword)) return new List<Product>();

            var results = MockDatabase.Products
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            p.ProductId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("ProductService.Search - found {Count} results", results.Count);
            return results;
        }

        public Product Create(Product newProduct)
        {
            _logger.LogInformation("ProductService.Create - creating product {ProductId} {Name}", newProduct.ProductId, newProduct.Name);
            newProduct.Id = MockDatabase.ProductIdCounter++;
            MockDatabase.Products.Add(newProduct);
            _logger.LogInformation("ProductService.Create - created id {Id}", newProduct.Id);
            return newProduct;
        }
        public List<Product> CreateMany(List<Product> productList)
        {
            _logger.LogInformation("ProductService.CreateMany - creating {Count} products", productList?.Count ?? 0);
            var created = new List<Product>();
            foreach (var product in productList)
            {
                var p = Create(product);
                created.Add(p);
            }
            _logger.LogInformation("ProductService.CreateMany - created {Count} products", created.Count);
            return created;
        }

        public Product? Update(int id, Product updateData)
        {
            _logger.LogInformation("ProductService.Update - updating id {Id}", id);
            var product = MockDatabase.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return null;

            product.Name = updateData.Name;
            product.ProductId = updateData.ProductId;
            product.Price = updateData.Price;

            _logger.LogInformation("ProductService.Update - updated id {Id}", id);
            return product;
        }

        public bool Delete(int id)
        {
            _logger.LogInformation("ProductService.Delete - deleting id {Id}", id);
            var product = MockDatabase.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return false;

            MockDatabase.Products.Remove(product);
            _logger.LogInformation("ProductService.Delete - deleted id {Id}", id);
            return true;
        }

        public Product? Patch(int id, ProductPatchRequest patchRequest)
        {
            _logger.LogInformation("ProductService.Patch - patching id {Id} field {Field}", id, patchRequest?.FieldName);
            var product = MockDatabase.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return null;

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

            _logger.LogInformation("ProductService.Patch - patched id {Id} field {Field}", id, field);
            return product;
        }


    }
}
