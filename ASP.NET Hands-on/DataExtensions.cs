using ASP.NET_Hands_on.DatabseContext;
using Microsoft.EntityFrameworkCore;

namespace ASP.NET_Hands_on.Data
{
    public static class DataExtensions
    {
        public static void MigrateDb(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<AppDbContext>();
            
            dbContext.Database.Migrate();
        }
    }
}
