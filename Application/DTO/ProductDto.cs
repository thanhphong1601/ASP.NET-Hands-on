using FluentValidation;
using System;

namespace ASP.NET_Hands_on.Application.DTO
{
    // DTO returned by product APIs: only expose necessary fields
    public record ProductDto
    (
        int id,
        string ProductId,
        string Name,
        decimal Price
    );

    public class ProductDtoValidator : AbstractValidator<ProductDto>
    {
        public ProductDtoValidator()
        {
            RuleFor(p => p.ProductId)
                .NotEmpty()
                .Length(5, 10)
                .WithMessage("ProductId is required and must be between 5 and 10 characters.");

            RuleFor(p => p.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Name is required and must be at most 100 characters.");

            RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price must be greater than or equal to zero.");
        }
    }
}
