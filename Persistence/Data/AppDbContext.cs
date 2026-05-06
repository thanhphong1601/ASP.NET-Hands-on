using ASP.NET_Hands_on.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Hands_on.DatabseContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<DiscountDay> DiscountDays { get; set; }
        public DbSet<DiscountDayProduct> DiscountDayProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // making 2 primary keys for OrderProduct
            modelBuilder.Entity<OrderProduct>()
                .HasKey(op => new { op.OrderId, op.ProductId });

            // configuring the relationships 1 to many between OrderProduct and Order with OrderId as foreign key
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId);

            // Order - Customer relationship: one Customer has many Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // configuring the relationships 1 to many between OrderProduct and Product with ProductId as foreign key
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId);

            // DiscountDayProduct relationship
            modelBuilder.Entity<DiscountDayProduct>()
                .HasKey(ddp => new { ddp.DiscountDayId, ddp.ProductId });

            modelBuilder.Entity<DiscountDayProduct>()
                .HasOne(ddp => ddp.DiscountDay)
                .WithMany(dd => dd.DiscountDayProducts)
                .HasForeignKey(ddp => ddp.DiscountDayId);

            modelBuilder.Entity<DiscountDayProduct>()
                .HasOne(ddp => ddp.Product)
                .WithMany(p => p.DiscountDayProducts)
                .HasForeignKey(ddp => ddp.ProductId);

            SeedData(modelBuilder);
        }
        private void SeedData(ModelBuilder modelBuilder)
        {
            // 1. Seed 5 Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "Phong Nguyen", Username = "phong.nguyen", Email = "phong.nguyen@example.com", Address = "Ho Chi Minh City", Gender = "Male" },
                new Customer { Id = 2, Name = "Minh Tran", Username = "minh.tran", Email = "minh.tran@example.com", Address = "Ha Noi", Gender = "Male" },
                new Customer { Id = 3, Name = "Linh Vo", Username = "linh.vo", Email = "linh.vo@example.com", Address = "Da Nang", Gender = "Female" },
                new Customer { Id = 4, Name = "Hoang Le", Username = "hoang.le", Email = "hoang.le@example.com", Address = "Can Tho", Gender = "Male" },
                new Customer { Id = 5, Name = "An Dang", Username = "an.dang", Email = "an.dang@example.com", Address = "Hue", Gender = "Female" }
            );

            // 2. Seed 20 Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, ProductId = "LAP01", Name = "Laptop Asus ROG Strix", Price = 35000000 },
        new Product { Id = 2, ProductId = "LAP02", Name = "Laptop Dell XPS 15", Price = 45000000 },
        new Product { Id = 3, ProductId = "LAP03", Name = "Laptop Lenovo ThinkPad", Price = 28000000 },
        new Product { Id = 4, ProductId = "ACC01", Name = "Bàn phím cơ Keychron K2", Price = 2500000 },
        new Product { Id = 5, ProductId = "ACC02", Name = "Chuột Logitech MX Master 3S", Price = 2200000 },
        new Product { Id = 6, ProductId = "ACC03", Name = "Màn hình LG UltraGear 27\"", Price = 8500000 },
        new Product { Id = 7, ProductId = "ACC04", Name = "Tai nghe Sony WH-1000XM5", Price = 6500000 },
        new Product { Id = 8, ProductId = "ACC05", Name = "Ổ cứng SSD Samsung 1TB", Price = 3200000 },
        new Product { Id = 9, ProductId = "ACC06", Name = "Ghế Ergonomic Herman Miller", Price = 45000000 },
        new Product { Id = 10, ProductId = "BOK01", Name = "Sách C# 12 and .NET 8", Price = 650000 },
        new Product { Id = 11, ProductId = "BOK02", Name = "Sách Clean Architecture", Price = 550000 },
        new Product { Id = 12, ProductId = "BOK03", Name = "Sách SQL Optimization", Price = 480000 },
        new Product { Id = 13, ProductId = "BOK04", Name = "Sách Angular & TypeScript", Price = 520000 },
        new Product { Id = 14, ProductId = "BOK05", Name = "Sách Python Data Science", Price = 600000 },
        new Product { Id = 15, ProductId = "SUB01", Name = "Gói NovelAI Premium (Tháng)", Price = 600000 },
        new Product { Id = 16, ProductId = "SUB02", Name = "Gói Last Origin Monthly Pass", Price = 350000 },
        new Product { Id = 17, ProductId = "SFT01", Name = "Bản quyền JetBrains Rider", Price = 3800000 },
        new Product { Id = 18, ProductId = "SFT02", Name = "OpenAI API Credits", Price = 1200000 },
        new Product { Id = 19, ProductId = "SFT03", Name = "PaddleOCR Pro License", Price = 5000000 },
        new Product { Id = 20, ProductId = "SFT04", Name = "GitHub Copilot (Năm)", Price = 2500000 }
            );

            // 3. Seed 5 Orders
            modelBuilder.Entity<Order>().HasData(
        // Order 1: LAP01 (35M) + ACC01 (2.5M) = 37,500,000
        new Order { OrderId = 1, CustomerId = 1, OrderDate = new DateTime(2026, 4, 19), TotalPrice = 37500000 },

        // Order 2: LAP02 (45M) = 45,000,000
        new Order { OrderId = 2, CustomerId = 2, OrderDate = new DateTime(2026, 4, 20), TotalPrice = 45000000 },

        // Order 3: 2x SUB01 (2 * 600k) = 1,200,000
        new Order { OrderId = 3, CustomerId = 3, OrderDate = new DateTime(2026, 4, 21), TotalPrice = 1200000 },

        // Order 4: BOK01 (650k) + BOK04 (520k) = 1,170,000
        new Order { OrderId = 4, CustomerId = 1, OrderDate = new DateTime(2026, 4, 22), TotalPrice = 1170000 },

        // Order 5: ACC06 (45M) + ACC02 (2.2M) = 47,200,000
        new Order { OrderId = 5, CustomerId = 5, OrderDate = new DateTime(2026, 4, 23), TotalPrice = 47200000 }
    );
            modelBuilder.Entity<OrderProduct>().HasData(
        // Order 1: Sản phẩm 1 (LAP01) và 4 (ACC01)
        new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 1 },
        new OrderProduct { OrderId = 1, ProductId = 4, Quantity = 1 },

        // Order 2: Sản phẩm 2 (LAP02)
        new OrderProduct { OrderId = 2, ProductId = 2, Quantity = 1 },

        // Order 3: Sản phẩm 15 (SUB01) x 2
        new OrderProduct { OrderId = 3, ProductId = 15, Quantity = 2 },

        // Order 4: Sản phẩm 10 (BOK01) và 13 (BOK04)
        new OrderProduct { OrderId = 4, ProductId = 10, Quantity = 1 },
        new OrderProduct { OrderId = 4, ProductId = 13, Quantity = 1 },

        // Order 5: Sản phẩm 9 (ACC06) và 5 (ACC02)
        new OrderProduct { OrderId = 5, ProductId = 9, Quantity = 1 },
        new OrderProduct { OrderId = 5, ProductId = 5, Quantity = 1 }
    );
        }
    }
}
