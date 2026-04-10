using ASP.NET_Hands_on.Middlewares;
using ASP.NET_Hands_on.Interface;
using ASP.NET_Hands_on.Service;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ASP.NET_Hands_on.Exceptions;
using Serilog;

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

            ClockSkew = TimeSpan.Zero // Optional: Set clock skew to zero for testing purposes
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
//ConfigureApiBehaviorOptions
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOpenApiDocument();

// Register product and order services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

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

// Enable Serilog integration with the generic host
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseOpenApi();

    app.UseSwaggerUi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
//app.UseMiddleware<ApiKeyCheckMiddleware>();

// Add global exception handler middleware
app.AddExceptionHandler<GlobalExceptionHandler>();

app.UseAuthorization();

app.MapControllers();

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
