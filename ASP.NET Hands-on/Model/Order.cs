using System.Collections.Generic;

namespace ASP.NET_Hands_on.Model
{
    public class Order
    {
        public int OrderId { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();

        public decimal TotalPrice { get; set; }
    }
}
