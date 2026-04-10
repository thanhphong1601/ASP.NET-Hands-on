using FluentValidation;

namespace ASP.NET_Hands_on.DTO
{
    public class ProductPatchRequest
    {
        public string? FieldName { get; set; }
        public string? NewValue { get; set; }
    }

    public class ProductPatchRequestValidator : AbstractValidator<ProductPatchRequest>
    {
        public ProductPatchRequestValidator()
        {
            RuleFor(p => p.FieldName)
                .NotEmpty()
                .Must(field => field == "productId" || field == "name" || field == "price")
                .WithMessage("FieldName must be either 'productId', 'name', or 'price'.");
            RuleFor(p => p.NewValue)
                .NotEmpty()
                .WithMessage("NewValue is required.");
        }
    }
}
