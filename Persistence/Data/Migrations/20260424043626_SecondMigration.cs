using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "Email", "Gender", "Name", "Username" },
                values: new object[,]
                {
                    { 1, "Ho Chi Minh City", "phong.nguyen@example.com", "Male", "Phong Nguyen", "phong.nguyen" },
                    { 2, "Ha Noi", "minh.tran@example.com", "Male", "Minh Tran", "minh.tran" },
                    { 3, "Da Nang", "linh.vo@example.com", "Female", "Linh Vo", "linh.vo" },
                    { 4, "Can Tho", "hoang.le@example.com", "Male", "Hoang Le", "hoang.le" },
                    { 5, "Hue", "an.dang@example.com", "Female", "An Dang", "an.dang" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price", "ProductId" },
                values: new object[,]
                {
                    { 1, "Laptop Asus ROG Strix", 35000000m, "LAP01" },
                    { 2, "Laptop Dell XPS 15", 45000000m, "LAP02" },
                    { 3, "Laptop Lenovo ThinkPad", 28000000m, "LAP03" },
                    { 4, "Bàn phím cơ Keychron K2", 2500000m, "ACC01" },
                    { 5, "Chuột Logitech MX Master 3S", 2200000m, "ACC02" },
                    { 6, "Màn hình LG UltraGear 27\"", 8500000m, "ACC03" },
                    { 7, "Tai nghe Sony WH-1000XM5", 6500000m, "ACC04" },
                    { 8, "Ổ cứng SSD Samsung 1TB", 3200000m, "ACC05" },
                    { 9, "Ghế Ergonomic Herman Miller", 45000000m, "ACC06" },
                    { 10, "Sách C# 12 and .NET 8", 650000m, "BOK01" },
                    { 11, "Sách Clean Architecture", 550000m, "BOK02" },
                    { 12, "Sách SQL Optimization", 480000m, "BOK03" },
                    { 13, "Sách Angular & TypeScript", 520000m, "BOK04" },
                    { 14, "Sách Python Data Science", 600000m, "BOK05" },
                    { 15, "Gói NovelAI Premium (Tháng)", 600000m, "SUB01" },
                    { 16, "Gói Last Origin Monthly Pass", 350000m, "SUB02" },
                    { 17, "Bản quyền JetBrains Rider", 3800000m, "SFT01" },
                    { 18, "OpenAI API Credits", 1200000m, "SFT02" },
                    { 19, "PaddleOCR Pro License", 5000000m, "SFT03" },
                    { 20, "GitHub Copilot (Năm)", 2500000m, "SFT04" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "CustomerId", "OrderDate", "TotalPrice" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), 37500000m },
                    { 2, 2, new DateTime(2026, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 45000000m },
                    { 3, 3, new DateTime(2026, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), 1200000m },
                    { 4, 1, new DateTime(2026, 4, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 1170000m },
                    { 5, 5, new DateTime(2026, 4, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 47200000m }
                });

            migrationBuilder.InsertData(
                table: "OrderProducts",
                columns: new[] { "OrderId", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 1, 4, 1 },
                    { 2, 2, 1 },
                    { 3, 15, 2 },
                    { 4, 10, 1 },
                    { 4, 13, 1 },
                    { 5, 5, 1 },
                    { 5, 9, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 1, 4 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 3, 15 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 4, 10 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 4, 13 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 5, 5 });

            migrationBuilder.DeleteData(
                table: "OrderProducts",
                keyColumns: new[] { "OrderId", "ProductId" },
                keyValues: new object[] { 5, 9 });

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
