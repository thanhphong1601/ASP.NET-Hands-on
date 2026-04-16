using Application;
using ASP.NET_Hands_on.Application.CQRS.Products;
using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Application.IRepository;
using ASP.NET_Hands_on.Application.Service;
using ASP.NET_Hands_on.Data;
using ASP.NET_Hands_on.DatabseContext;
using ASP.NET_Hands_on.Domain.Model;
using ASP.NET_Hands_on.Exceptions;
using ASP.NET_Hands_on.Infrastructure;
using ASP.NET_Hands_on.Persistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Serilog;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var appName = builder.Environment.ApplicationName ?? "ASP.NET_Hands_on";
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File($"Logs/{appName}-{{Date}}.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Add services to the container.

//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//   .AddNegotiate();

//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy.
//    options.FallbackPolicy = options.DefaultPolicy;
//});
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),

            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

            var response = context.Response;
            response.StatusCode = StatusCodes.Status401Unauthorized;
            response.ContentType = "application/json";

            var payload = new
            {
                error = "Unauthorized",
                message = string.IsNullOrEmpty(context.ErrorDescription) ? "Authentication token is missing or invalid." : context.ErrorDescription
            };

                await response.WriteAsync(JsonSerializer.Serialize(payload));
            },

            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var responseObj = new
            {
                Success = false,
                Message = "Input data is not correct",
                Errors = errors
            };

            return new BadRequestObjectResult(responseObj);
        };
    });

//ConfigureApiBehaviorOptions
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOpenApiDocument();

// Register MediatR handlers
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAllProductsQuery).Assembly));

// Register product and order services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDiscountDayRepository, DiscountDayRepository>();
// memory cache
builder.Services.AddMemoryCache();

// background email queue and hosted service
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundEmailQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddScoped<IEmailService, EmailService>();

//Add properties for below projects
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// register health check
builder.Services.AddHealthChecks();

// Add CORS policy to allow requests from any origin (for testing purposes)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add database
var connectionString = "Data Source=MyAppData.db";
builder.Services.AddSqlite<AppDbContext>(connectionString,
    optionsAction: options => options.UseSeeding((context, _) =>
    {
        if (!context.Set<Product>().Any())
        {
            context.Set<Product>().AddRange(
                new Product { Name = "Laptop Asus", ProductId = "LTA01", Price = 17000000 },
                new Product { Name = "Bàn phím cơ", ProductId = "BPC01", Price = 1500000 },
                new Product { Name = "Chuột không dây", ProductId = "PKC01", Price = 500000 },
                new Product { Name = "Màn hình", ProductId = "MH01", Price = 3000000 }
            );
            context.SaveChanges();
        }
    }));

// Add Refit
//Polly retry config
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Tự động bắt lỗi 5xx, 408 Timeout, rớt mạng
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            // Log ra để biết nó đang thử lại (có thể dùng Serilog ở đây)
            Console.WriteLine($"Đang thử lại lần thứ {retryAttempt} sau {timespan.TotalSeconds} giây...");
        });

var mockBase = builder.Configuration.GetValue<string>("MockApi:BaseUrl") ?? "http://localhost:5225";
builder.Services
    .AddRefitClient<IProductsFetchingApiByUrl>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(mockBase))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        // allow multiple concurrent connections to the same server (useful when calling local endpoints)
        MaxConnectionsPerServer = 10
    })
    .AddPolicyHandler(retryPolicy);
    //.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://dummyjson.com/"));

// Enable Serilog integration with the generic host
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseOpenApi();

    app.UseSwaggerUi();

    app.UseCors("AllowAll");
}

//config this CORS when production, only allow specific origins
app.UseCors("AllowAll");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        //construct the body
        var response = new
        {
            Status = report.Status.ToString(), // "Healthy", "Degraded", or "Unhealthy"

            // Get detailed result from health check tests before
            HealthChecks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description ?? "OK"
            }),

            // Showing the total duration of health check execution
            HealthCheckDuration = report.TotalDuration
        };

        // Serialize to json to return to client
        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    
    app.UseHsts();
}
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();

//app.UseMiddleware<ApiKeyCheckMiddleware>();

// Add global exception handler middleware
app.AddExceptionHandler<GlobalExceptionHandler>();

app.UseAuthorization();

app.MapControllers();

Log.Information("Migrating database");
app.MigrateDb();

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}