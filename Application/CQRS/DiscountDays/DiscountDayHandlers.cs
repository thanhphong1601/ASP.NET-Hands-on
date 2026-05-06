using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Application.DTO;
using ASP.NET_Hands_on.Application.IRepository;
using ASP.NET_Hands_on.Domain.Model;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ASP.NET_Hands_on.Application.CQRS.DiscountDays
{
    public record GetAllDiscountDaysQuery() : IRequest<List<DiscountDayDto>>;
    public record CreateDiscountDayCommand(DiscountDayRequestDto Dto) : IRequest<DiscountDayDto>;

    public class GetAllDiscountDaysQueryHandler : IRequestHandler<GetAllDiscountDaysQuery, List<DiscountDayDto>>
    {
        private readonly IDiscountDayRepository _repo;
        private readonly ILogger<GetAllDiscountDaysQueryHandler> _logger;
        private readonly IMemoryCache _cache;
        private const string CACHE_KEY = "DiscountDay_All";

        public GetAllDiscountDaysQueryHandler(IDiscountDayRepository repo, ILogger<GetAllDiscountDaysQueryHandler> logger, IMemoryCache cache)
        {
            _repo = repo;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<DiscountDayDto>> Handle(GetAllDiscountDaysQuery request, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CACHE_KEY, out List<DiscountDayDto>? cached))
            {
                _logger.LogInformation("GetAllDiscountDaysQueryHandler - returned from cache");
                return cached!;
            }

            var items = await _repo.GetAllWithProductsAsync(cancellationToken);

            var result = items.Select(dd => new DiscountDayDto(
                dd.Id,
                dd.DayName,
                dd.CreatedDate.Date,
                dd.FromDate,
                dd.ToDate,
                dd.DiscountDayProducts.Select(dpd => new ProductDto(dpd.Product.Id, dpd.Product.ProductId ?? string.Empty, dpd.Product.Name ?? string.Empty, dpd.Product.Price)).ToList()
            )).ToList();

            _cache.Set(CACHE_KEY, result, TimeSpan.FromSeconds(60));

            return result;
        }
    }

    public class CreateDiscountDayCommandHandler : IRequestHandler<CreateDiscountDayCommand, DiscountDayDto>
    {
        private readonly IDiscountDayRepository _repo;
        private readonly ILogger<CreateDiscountDayCommandHandler> _logger;

        public CreateDiscountDayCommandHandler(IDiscountDayRepository repo, ILogger<CreateDiscountDayCommandHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<DiscountDayDto> Handle(CreateDiscountDayCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto ?? throw new ArgumentNullException(nameof(request.Dto));

            var entity = new DiscountDay
            {
                DayName = dto.DayName,
                CreatedDate = dto.CreatedDate.Date,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate
            };

            await _repo.AddDiscountDayAsync(entity, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            var products = await _repo.GetProductsByIdsAsync(dto.ProductIds, cancellationToken);

            foreach (var p in products)
            {
                await _repo.AddDiscountDayProductAsync(new DiscountDayProduct
                {
                    DiscountDayId = entity.Id,
                    ProductId = p.Id
                }, cancellationToken);
            }

            await _repo.SaveChangesAsync(cancellationToken);

            var displayedProducts = products.Select(p => new ProductDto(p.Id, p.ProductId ?? string.Empty, p.Name ?? string.Empty, p.Price)).ToList();
            var created = new DiscountDayDto(entity.Id, entity.DayName, entity.CreatedDate, entity.FromDate, entity.ToDate, displayedProducts);
            return created;
        }
    }
}
