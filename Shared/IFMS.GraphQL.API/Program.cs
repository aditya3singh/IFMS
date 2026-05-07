using System.Text;
using IFMS.Admin.Infrastructure.Persistence;
using IFMS.Booking.Infrastructure.Persistence;
using IFMS.GraphQL.API.Queries;
using IFMS.Inventory.Infrastructure.Persistence;
using IFMS.Sales.Infrastructure.Persistence;
using IFMS.Station.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Databases (read-only access to all service DBs) ───────────────────────────
builder.Services.AddDbContext<BookingDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("BookingConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddDbContext<SalesDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("SalesConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddDbContext<InventoryDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("InventoryConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddDbContext<StationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("StationConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddDbContext<AdminDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("SalesConnection"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// ── JWT Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Hot Chocolate GraphQL ─────────────────────────────────────────────────────
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddAuthorization()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

// GraphQL endpoint — http://localhost:5011/graphql
// Banana Cake Pop UI — http://localhost:5011/graphql (browser)
app.MapGraphQL();

app.Run();
