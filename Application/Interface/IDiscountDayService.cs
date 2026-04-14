using ASP.NET_Hands_on.DTO;
using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.Interface
{
    public interface IDiscountDayService
    {
        Task<List<DiscountDayDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<DiscountDayDto> CreateAsync(DiscountDayRequestDto dto, CancellationToken cancellationToken);
    }
}
