using System.Text;
using IFMS.Booking.API.Services;
using IFMS.Booking.Application.Commands;
using IFMS.Booking.Application.Interfaces;
using IFMS.Booking.Application.Options;
using IFMS.Booking.Infrastructure.Persistence;
using IFMS.Booking.Infrastructure.Repositories;
using IFMS.Booking.Infrastructure.Services;
using IFMS.Station.Application.Interfaces;
using IFMS.Station.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Redis
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection));

// Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

var stationConnection = builder.Configuration.GetConnectionString("StationConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:StationConnection is required (IFMS_StationDB) for dealer station bookings.");

builder.Services.AddDbContext<StationDbContext>(options =>
    options.UseSqlServer(
        stationConnection,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services.AddScoped<IDealerAssignmentRepository, DealerAssignmentRepository>();

builder.Services.Configure<BookingFlowOptions>(builder.Configuration.GetSection(BookingFlowOptions.SectionName));
builder.Services.Configure<KycOptions>(builder.Configuration.GetSection(KycOptions.SectionName));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IKycSessionStore, MemoryKycSessionStore>();
builder.Services.AddSingleton<IKycVerificationProvider, StubKycVerificationProvider>();

// Services
builder.Services.AddScoped<ITokenCacheService, RedisTokenCacheService>();

// Handlers
builder.Services.AddScoped<BookingCommandHandler>();
builder.Services.AddScoped<KycVerificationHandler>();

// HTTP Client for Sales API (kept for token validation fallback)
builder.Services.AddHttpClient("SalesAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:SalesApiUrl"] ?? "http://sales-api:8080");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// HTTP Client for Notification API (kept for in-app push fallback)
builder.Services.AddHttpClient("NotificationAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationApiUrl"] ?? "http://notification-api:8080");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddScoped<INotificationPublisher, HttpNotificationPublisher>();

// MassTransit — RabbitMQ publisher
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "ifms";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "ifms@12345";
        cfg.Host(host, "/", h => { h.Username(user); h.Password(pass); });
        cfg.ConfigureEndpoints(ctx);
    });
});

// Background Services
builder.Services.AddHostedService<TokenExpiryBackgroundService>();

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
