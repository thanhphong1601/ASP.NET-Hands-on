using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Domain.Model;

namespace ASP.NET_Hands_on.Application.Interface
{
    public interface IDiscountDayService
    {
        Task<List<DiscountDayDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<DiscountDayDto> CreateAsync(DiscountDayRequestDto dto, CancellationToken cancellationToken);
    }
}
