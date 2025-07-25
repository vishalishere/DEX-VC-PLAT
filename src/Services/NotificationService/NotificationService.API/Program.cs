// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using NotificationService.API.Data;
using NotificationService.API.Services;
using NotificationService.API.Hubs;
using Shared.Security.Services;
using SendGrid.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// DecVCPlat Notification Service logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] DecVCPlat-NotificationService: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/decvcplat-notifications-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();

// DecVCPlat Notification Service database
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DecVCPlatConnection") ?? 
        "Server=localhost;Database=DecVCPlat_NotificationService;Trusted_Connection=true;TrustServerCertificate=true;"));

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

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
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

// SendGrid email service
var sendGridApiKey = builder.Configuration["SendGrid:ApiKey"] ?? "SG.DecVCPlat-SendGrid-API-Key-Demo";
builder.Services.AddSendGrid(options => options.ApiKey = sendGridApiKey);
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// SignalR for real-time notifications
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

// Background services for notification processing
builder.Services.AddHostedService<NotificationProcessingService>();

// DecVCPlat API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DecVCPlat Notification Service API",
        Version = "v1.0",
        Description = "Decentralized Venture Capital Platform - Notification Service with Real-time SignalR and Email Integration"
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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DecVCPlat Notification Service v1"));
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationhub");

// DecVCPlat database initialization
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await context.Database.EnsureCreatedAsync();
    Log.Information("DecVCPlat Notification Service database initialized");
}

Log.Information("DecVCPlat Notification Service started with SignalR and email integration");
app.Run();

// Background service for processing notifications
public class NotificationProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationProcessingService> _logger;

    public NotificationProcessingService(IServiceScopeFactory scopeFactory, ILogger<NotificationProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                // Process pending notifications
                var pendingNotifications = await context.Notifications
                    .Where(n => n.Status == NotificationService.API.Models.Entities.NotificationStatus.Pending)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var notification in pendingNotifications)
                {
                    // Mark as sent (simplified processing)
                    notification.Status = NotificationService.API.Models.Entities.NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                }

                if (pendingNotifications.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("DecVCPlat notification processing: {Count} notifications processed", pendingNotifications.Count);
                }

                // Clean up expired notifications
                var expiredNotifications = await context.Notifications
                    .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow && n.Status != NotificationService.API.Models.Entities.NotificationStatus.Expired)
                    .ToListAsync(stoppingToken);

                foreach (var notification in expiredNotifications)
                {
                    notification.Status = NotificationService.API.Models.Entities.NotificationStatus.Expired;
                }

                if (expiredNotifications.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("DecVCPlat notification cleanup: {Count} expired notifications processed", expiredNotifications.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DecVCPlat notification processing service");
            }

            // Wait 30 seconds before next processing cycle
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
