using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Hands_on.Domain.Model
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string? ProductId { get; set; }
        [Required]
        public string? Name { get; set; }
        public decimal Price { get; set; } = decimal.Zero;

        public List<OrderProduct> OrderProducts { get; set; } = new();
        public List<DiscountDayProduct> DiscountDayProducts { get; set; } = new();
    }

    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator() {
            RuleFor(p => p.ProductId)
                .NotEmpty()
                .Length(5, 10)
                .WithMessage("ProductId is required.");
            RuleFor(p => p.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Name is required.");
            RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price must be greater than or equal to zero.");
        }
    }
}
