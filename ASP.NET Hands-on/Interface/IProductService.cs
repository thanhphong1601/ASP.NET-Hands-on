using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.Interface
{
    public interface IProductService
    {
        List<Product> GetAll();
        List<Product> SearchByNameOrProductId(string keyword);
        Product Create(Product newProduct);
        List<Product> CreateMany(List<Product> productList);
        Product? Update(int id, Product updateData);
        bool Delete(int id);
    }
}
