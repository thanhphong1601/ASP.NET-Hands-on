using ASP.NET_Hands_on.DTO;
using Refit;

namespace ASP.NET_Hands_on.Interface
{
    public interface IProductsFetchingApiByUrl
    {
        [Get("/products")]
        Task<ProductResponse> GetProducts();
    }
}
