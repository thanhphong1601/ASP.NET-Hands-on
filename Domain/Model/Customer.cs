using FluentValidation;
using System.Collections.Generic;

namespace ASP.NET_Hands_on.Domain.Model
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty; // "Male"/"Female" or other

        public List<Order> Orders { get; set; } = new();
    }

    //public class CustomerValidator : AbstractValidator<Customer>
    //{
    //    public CustomerValidator()
    //    {
    //        RuleFor(c => c.Username).NotEmpty().WithMessage("Username is required.");
    //        RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
    //        RuleFor(c => c.Name).NotEmpty().WithMessage("Name is required.");
    //        RuleFor(c => c.Password).NotEmpty().WithMessage("Password is required.");
    //    }
    //}
}
