using FluentValidation;
using System.Collections.Generic;

namespace ASP.NET_Hands_on.Domain.Model
{
    public class Order
    {
        public int OrderId { get; set; }

        public decimal TotalPrice { get; set; } = 0;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string? Address { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public List<OrderProduct> OrderProducts { get; set; } = new(); 
    }

}
