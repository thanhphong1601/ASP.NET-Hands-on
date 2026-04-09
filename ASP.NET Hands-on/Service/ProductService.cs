using System;
using System.Linq;
using System.Collections.Generic;
using ASP.NET_Hands_on.ClientSideDatabase;
using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.Service
{
    public class ProductService : IProductService
    {
        public List<Product> GetAll()
        {
            return MockDatabase.Products;
        }

        public List<Product> SearchByNameOrProductId(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return [];

            var results = MockDatabase.Products
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                p.ProductId.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            return results;
        }

        public Product Create(Product newProduct)
        {
            newProduct.Id = MockDatabase.ProductIdCounter++;
            MockDatabase.Products.Add(newProduct);
            return newProduct;
        }
        public List<Product> CreateMany(List<Product> productList)
        {
            var created = new List<Product>();
            foreach (var product in productList)
            {
                var p = Create(product);
                created.Add(p);
            }
            return created;
        }

        public Product? Update(int id, Product updateData)
        {
            var product = MockDatabase.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return null;

            product.Name = updateData.Name;
            product.ProductId = updateData.ProductId;
            product.Price = updateData.Price;

            return product;
        }

        public bool Delete(int id)
        {
            var product = MockDatabase.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return false;

            MockDatabase.Products.Remove(product);
            return true;
        }


    }
}
