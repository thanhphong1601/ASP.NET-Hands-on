namespace ASP.NET_Hands_on.Model
{
    public class Order_Product
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 0;
    }
}
