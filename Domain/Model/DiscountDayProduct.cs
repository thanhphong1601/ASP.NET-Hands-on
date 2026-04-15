namespace ASP.NET_Hands_on.Domain.Model
{
    public class DiscountDayProduct
    {
        public int DiscountDayId { get; set; }
        public DiscountDay DiscountDay { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
