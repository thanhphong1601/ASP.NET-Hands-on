using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Hands_on.Model
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string ProductId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } = decimal.Zero;
    }
}
