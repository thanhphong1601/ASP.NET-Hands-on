using ASP.NET_Hands_on.Application.DTO;
using Refit;

namespace ASP.NET_Hands_on.Application.Interface
{
    public interface IProductsFetchingApiByUrl
    {
        [Get("/products")]
        Task<ProductResponse> GetProducts();

        [Get("/api/mock")]
        Task<int> Test();
    }
}
