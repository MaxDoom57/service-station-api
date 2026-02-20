using Application.Interfaces;
using Application.Services;
using Infrastructure.Context;
using Infrastructure.Helpers;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Scoped Services
builder.Services.AddScoped<IUserRequestContext, UserRequestContext>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDynamicDbContextFactory, DynamicDbContextFactory>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserKeyService, UserKeyService>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<CustomerAccountService>();
builder.Services.AddScoped<SalesAccountService>();
builder.Services.AddScoped<PaymentTermService>();
builder.Services.AddScoped<InvoiceDetailsService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<CommonLookupService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<CodeService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<PackageService>();
builder.Services.AddScoped<VehicleTypeService>();
builder.Services.AddScoped<BayService>();
builder.Services.AddScoped<BayControlService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<ServiceOrderService>();
builder.Services.AddScoped<BayWorkerService>();
builder.Services.AddScoped<OrderManagementService>();

builder.Services.AddMemoryCache();


// CORS Policy (Allow Frontend Only)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://vehicle-service-web-eight.vercel.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();  // Uncomment if using cookies
        });
});


// DB Context
builder.Services.AddDbContext<MainDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

// AUTHENTICATION MUST BE BEFORE builder.Build()
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Middleware order
app.UseMiddleware<Api.Middlewares.JwtSessionMiddleware>();
app.UseMiddleware<Api.Middlewares.MockResponseMiddleware>();
app.UseMiddleware<Infrastructure.Middleware.RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Enable CORS here (BEFORE Authentication)
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Run
app.Run();

public partial class Program { }