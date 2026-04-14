using System.Collections.Generic;

namespace ASP.NET_Hands_on.DTO
{
    public class OrderCreateRequest
    {
        public List<int> ProductIds { get; set; } = new();
        public string Email { get; set; } = string.Empty;
    }
}
