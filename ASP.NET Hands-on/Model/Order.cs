using FluentValidation;
using System.Collections.Generic;

namespace ASP.NET_Hands_on.Model
{
    public class Order
    {
        public int OrderId { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public List<OrderProduct> OrderProducts { get; set; } = new(); 
    }

    public class OrderValidator : AbstractValidator<Order>
    {
    }
}
