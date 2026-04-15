using FluentValidation;
using System;
using System.Collections.Generic;

namespace ASP.NET_Hands_on.Application.DTO
{
    public record DiscountDayDto
    (
        int Id,
        string DayName,
        DateTime CreatedDate,
        DateTime FromDate,
        DateTime ToDate,
        List<ProductDto> Products
    );

    public class DiscountDayDtoValidator : AbstractValidator<DiscountDayDto>
    {
        public DiscountDayDtoValidator()
        {
            RuleFor(d => d.DayName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(d => d.CreatedDate)
                .NotEmpty();

            RuleFor(d => d.FromDate)
                .LessThan(d => d.ToDate)
                .WithMessage("FromDate must be earlier than ToDate");

            RuleFor(d => d.Products)
                .NotNull()
                .Must(list => list.Count > 0)
                .WithMessage("At least one product must be assigned to a discount day");

            RuleForEach(d => d.Products).SetValidator(new ProductDtoValidator());
        }
    }
}
