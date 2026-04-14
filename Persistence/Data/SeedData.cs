using ASP.NET_Hands_on.DatabseContext;
using ASP.NET_Hands_on.Model;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Hands_on.Data
{
    public class SeedData
    {
    //    public static async Task Seed100kProductsAsync(DbContext context, CancellationToken cancellationToken = default)
    //    {
    //        var categoryPrefixes = new Dictionary<string, string>
    //{
    //    { "Bàn phím", "BP" },
    //    { "Chuột", "CH" },
    //    { "Máy tính", "MT" },
    //    { "Laptop", "LT" },
    //    { "Điện thoại", "DT" },
    //    { "Cáp sạc", "CS" },
    //    { "Connector", "CN" },
    //    { "RAM", "RM" },
    //    { "SSD", "SD" },
    //    { "CPU", "CP" }
    //};

    //        var brands = new List<string> { "Asus", "Dell", "HP", "Apple", "Samsung", "Logitech", "Corsair", "Kingston", "Intel", "AMD", "Sony", "Razer" };

    //        var random = new Random();
    //        var categories = categoryPrefixes.Keys.ToList();

    //        int totalRecords = 100000;
    //        int batchSize = 10000;

    //        var batchProducts = new List<Product>(batchSize);

    //        // 2. Bắt đầu vòng lặp sinh dữ liệu
    //        for (int i = 1; i <= totalRecords; i++)
    //        {
    //            // Random danh mục và hãng
    //            string randomCategory = categories[random.Next(categories.Count)];
    //            string randomBrand = brands[random.Next(brands.Count)];

    //            // Tạo Name: "Laptop Asus", "Chuột Logitech"...
    //            string productName = $"{randomCategory} {randomBrand}";

    //            // Tạo ProductId: Lấy Prefix + Định dạng số i thành 7 chữ số. VD: LT0000001
    //            string prefix = categoryPrefixes[randomCategory];
    //            string productId = $"{prefix}{i.ToString("D7")}";

    //            // Tạo Giá ngẫu nhiên nhưng chẵn (nhân với 100.000 VNĐ)
    //            // Giá sẽ dao động từ 100,000 đ đến 50,000,000 đ
    //            decimal price = random.Next(1, 501) * 100000m;

    //            // Đưa vào danh sách chờ
    //            batchProducts.Add(new Product { Name = productName, ProductId = productId, Price = price });

    //            if (i % batchSize == 0)
    //            {
    //                // Thay _db bằng context
    //                await context.Set<Product>().AddRangeAsync(batchProducts, cancellationToken);
    //                await context.SaveChangesAsync(cancellationToken);

    //                batchProducts.Clear();
    //                context.ChangeTracker.Clear();

    //                Console.WriteLine($"Đã insert thành công {i} / {totalRecords} sản phẩm...");
    //            }
    //        }

    //    }
    }
}
