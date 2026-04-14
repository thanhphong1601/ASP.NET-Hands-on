using FluentValidation;
using System;

namespace ASP.NET_Hands_on.DTO
{
    public record OrderDto
    (
        int OrderId,
        DateTime OrderDate,
        decimal TotalPrice
    );

    public class OrderDtoValidator : AbstractValidator<OrderDto>
    {
        public OrderDtoValidator()
        {
            RuleFor(o => o.OrderId).GreaterThan(0);
            RuleFor(o => o.OrderDate).NotEmpty();
            RuleFor(o => o.TotalPrice).GreaterThanOrEqualTo(0);
        }
    }
}
