using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Domain.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ASP.NET_Hands_on.Application.IRepository;

namespace ASP.NET_Hands_on.Application.Service
{
    public class DiscountDayService : IDiscountDayService
    {
        private readonly IDiscountDayRepository _discountDayRepository;
        private readonly ILogger<DiscountDayService> _logger;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "DiscountDay_All";

        public DiscountDayService(IDiscountDayRepository discountDayRepository, ILogger<DiscountDayService> logger, IMemoryCache cache)
        {
            _discountDayRepository = discountDayRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<DiscountDayDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CACHE_KEY, out List<DiscountDayDto>? cached))
            {
                _logger.LogInformation("DiscountDayService.GetAllAsync - returned from cache");
                return cached!;
            }

            var items = await _discountDayRepository.GetAllWithProductsAsync(cancellationToken);

            var result = items.Select(dd => new DiscountDayDto(
                dd.Id,
                dd.DayName,
                dd.CreatedDate.Date,
                dd.FromDate,
                dd.ToDate,
                dd.DiscountDayProducts.Select(dpd => new ProductDto(dpd.Product.ProductId ?? string.Empty, dpd.Product.Name ?? string.Empty, dpd.Product.Price)).ToList()
            )).ToList();

            // cache for 60 seconds
            _cache.Set(CACHE_KEY, result, TimeSpan.FromSeconds(60));

            return result;
        }

        public async Task<DiscountDayDto> CreateAsync(DiscountDayRequestDto dto, CancellationToken cancellationToken)
        {
            var entity = new DiscountDay
            {
                DayName = dto.DayName,
                CreatedDate = dto.CreatedDate.Date,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate
            };

            await _discountDayRepository.AddDiscountDayAsync(entity, cancellationToken);
            await _discountDayRepository.SaveChangesAsync(cancellationToken);

            var products = await _discountDayRepository.GetProductsByIdsAsync(dto.ProductIds, cancellationToken);

            foreach (var p in products)
            {
                await _discountDayRepository.AddDiscountDayProductAsync(new DiscountDayProduct
                {
                    DiscountDayId = entity.Id,
                    ProductId = p.Id
                }, cancellationToken);
            }

            await _discountDayRepository.SaveChangesAsync(cancellationToken);

            // invalidate cache
            _cache.Remove(CACHE_KEY);

            var displayedProducts = products.Select(p => new ProductDto(p.ProductId ?? string.Empty, p.Name ?? string.Empty, p.Price)).ToList();
            // return created dto
            var created = new DiscountDayDto(entity.Id, entity.DayName, entity.CreatedDate, entity.FromDate, entity.ToDate, displayedProducts);
            return created;
        }
    }
}
