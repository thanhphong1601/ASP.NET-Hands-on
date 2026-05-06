using FluentValidation;

namespace ASP.NET_Hands_on.Application.DTO
{
    public record DiscountDayRequestDto
    (
        string DayName,
        DateTime CreatedDate,
        DateTime FromDate,
        DateTime ToDate,
        List<int> ProductIds
    );

    public class DiscountDayRequestDtoValiator : AbstractValidator<DiscountDayRequestDto>
    {
        public DiscountDayRequestDtoValiator()
        {
            RuleFor(d => d.DayName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(d => d.CreatedDate)
                .NotEmpty();

            RuleFor(d => d.FromDate)
                .LessThan(d => d.ToDate)
                .WithMessage("FromDate must be earlier than ToDate");

            RuleFor(d => d.ProductIds)
                .NotNull()
                .Must(list => list.Count > 0)
                .WithMessage("At least one product must be assigned to a discount day");

        }
    }
}
