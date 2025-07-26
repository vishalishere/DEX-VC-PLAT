// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using FundingService.API.Data;
using Shared.Security.Services;

var builder = WebApplication.CreateBuilder(args);

// DecVCPlat Funding Service logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] DecVCPlat-FundingService: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/decvcplat-funding-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();

// DecVCPlat Funding Service database
builder.Services.AddDbContext<FundingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DecVCPlatConnection") ?? 
        "Server=localhost;Database=DecVCPlat_FundingService;Trusted_Connection=true;TrustServerCertificate=true;"));

// DecVCPlat JWT authentication
var decvcplatJwtKey = "DecVCPlat-JWT-Secret-Key-2024-256bits-SuperSecure-VentureCapital-Platform-Authentication-Token";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "DecVCPlat-Platform",
        ValidAudience = "DecVCPlat-Users",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(decvcplatJwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// DecVCPlat authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DecVCPlat-Founder", policy => policy.RequireClaim("role", "Founder"));
    options.AddPolicy("DecVCPlat-Investor", policy => policy.RequireClaim("role", "Investor"));
    options.AddPolicy("DecVCPlat-Luminary", policy => policy.RequireClaim("role", "Luminary"));
    options.AddPolicy("DecVCPlat-WalletVerified", policy => policy.RequireClaim("wallet_verified", "True"));
});

// DecVCPlat services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// DecVCPlat API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DecVCPlat Funding Service API",
        Version = "v1.0",
        Description = "Decentralized Venture Capital Platform - Funding Service with Tranche-based Disbursement",
        Contact = new OpenApiContact
        {
            Name = "DecVCPlat Team",
            Email = "support@decvcplat.com",
            Url = new Uri("https://github.com/vishalishere/DEX-VC-PLAT")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("DecVCPlatBearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "DecVCPlat JWT Authorization"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "DecVCPlatBearer" }
            },
            new string[] {}
        }
    });
});

// DecVCPlat CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DecVCPlatCORS", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://decvcplat.com", "https://*.decvcplat.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("DecVCPlatCORS");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DecVCPlat Funding Service v1"));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// DecVCPlat database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FundingDbContext>();
    await context.Database.EnsureCreatedAsync();
    Log.Information("DecVCPlat Funding Service database initialized");
}

Log.Information("DecVCPlat Funding Service started");
app.Run();
