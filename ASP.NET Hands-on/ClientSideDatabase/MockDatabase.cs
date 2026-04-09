using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.ClientSideDatabase
{
    public static class MockDatabase
    {
        public static List<Product> Products { get; set; } = new List<Product>();
        public static List<Order> Orders { get; set; } = new List<Order>();
        public static List<Order_Product> OrderProducts { get; set; } = new List<Order_Product>();

        // Biến đếm để giả lập tính năng Auto-Increment ID
        public static int ProductIdCounter = 1;
        public static int OrderIdCounter = 1;
        public static int OrderProductIdCounter = 1;
    }
}
