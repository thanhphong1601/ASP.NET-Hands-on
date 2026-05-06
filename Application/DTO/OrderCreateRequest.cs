using System.Collections.Generic;

namespace ASP.NET_Hands_on.Application.DTO
{
    public class OrderCreateRequest
    {
        public Dictionary<int, int> ProductIdsAndQuantity { get; set; } = new();
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int CustomerId { get; set; }
    }
}
