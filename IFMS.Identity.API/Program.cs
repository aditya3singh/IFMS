using System.Text;
using System.Threading.RateLimiting;
using BCrypt.Net;
using IFMS.Identity.Application.Commands;
using IFMS.Identity.Application.Interfaces;
using IFMS.Identity.Infrastructure.Persistence;
using IFMS.Identity.Infrastructure.Repositories;
using IFMS.Identity.Infrastructure.Security;
using IFMS.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISelfRegistrationPolicy, SelfRegistrationPolicy>();

var generalLimit = builder.Configuration.GetValue("Auth:RateLimitGeneralPermitPerMinute", 60);
var otpLimit = builder.Configuration.GetValue("Auth:RateLimitOtpPermitPerMinute", 8);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please wait a moment and try again."
        });
    };

    options.AddPolicy("auth", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value ?? "";
        var isSensitive = path.Contains("otp", StringComparison.OrdinalIgnoreCase)
                          || path.Contains("password/reset", StringComparison.OrdinalIgnoreCase);
        var limit = isSensitive ? otpLimit : generalLimit;
        var key = $"{ip}:{(isSensitive ? "s" : "g")}";
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpChallengeRepository, OtpChallengeRepository>();

// Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpClient("ifms-notify");
builder.Services.AddScoped<IOtpDeliveryService, OtpDeliveryService>();

// Handlers
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RequestLoginOtpCommandHandler>();
builder.Services.AddScoped<VerifyLoginOtpCommandHandler>();
builder.Services.AddScoped<RequestPasswordResetOtpCommandHandler>();
builder.Services.AddScoped<ResetPasswordCommandHandler>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
var app = builder.Build();

// Apply migrations and seed users on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();

    // Seed default users if none exist
    if (!dbContext.Users.Any())
    {
        var users = new[]
        {
            IFMS.Identity.Domain.Entities.User.Create(
                "Admin User", "admin-test@ifms.com",
                BCrypt.Net.BCrypt.HashPassword("Admin@12345"), "Admin"),
            IFMS.Identity.Domain.Entities.User.Create(
                "Dealer User", "dealer-test@ifms.com",
                BCrypt.Net.BCrypt.HashPassword("Dealer@12345"), "Dealer"),
            IFMS.Identity.Domain.Entities.User.Create(
                "Customer User", "customer-test@ifms.com",
                BCrypt.Net.BCrypt.HashPassword("Customer@12345"), "Customer"),
            IFMS.Identity.Domain.Entities.User.Create(
                "John Doe", "john@test.com",
                BCrypt.Net.BCrypt.HashPassword("Test@12345"), "Customer"),
        };
        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("auth");

app.Run();