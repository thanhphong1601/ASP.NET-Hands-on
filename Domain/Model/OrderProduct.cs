using System.Text.Json.Serialization;

namespace ASP.NET_Hands_on.Domain.Model
{
    public class OrderProduct
    {
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } 
    }
}
