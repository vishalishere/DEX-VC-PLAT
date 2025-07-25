// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using Serilog;
using System.Text;
using UserManagement.API.Data;
using UserManagement.API.Models.DTOs;
using UserManagement.API.Validators;
using Shared.Security.Models;
using Shared.Security.Services;

var builder = WebApplication.CreateBuilder(args);

// DecVCPlat specific logging configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] DecVCPlat-UserMgmt: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/decvcplat-usermgmt-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

// DecVCPlat database configuration
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DecVCPlatConnection") ?? 
        "Server=localhost;Database=DecVCPlat_UserManagement;Trusted_Connection=true;TrustServerCertificate=true;"));

// DecVCPlat Identity configuration
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UserDbContext>()
.AddDefaultTokenProviders();

// DecVCPlat JWT configuration
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
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

// DecVCPlat API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DecVCPlat User Management API",
        Version = "v1.0",
        Description = "Decentralized Venture Capital Platform - User Management Microservice"
    });

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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DecVCPlat User Management v1"));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// DecVCPlat database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await context.Database.EnsureCreatedAsync();
    Log.Information("DecVCPlat User Management database initialized");
}

Log.Information("DecVCPlat User Management Service started on port {Port}", app.Urls);
app.Run();
