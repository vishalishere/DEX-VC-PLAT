// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Serilog;
using System.Text;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// DecVCPlat Gateway logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] DecVCPlat-Gateway: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/decvcplat-gateway-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// DecVCPlat JWT authentication for gateway
var decvcplatJwtKey = "DecVCPlat-JWT-Secret-Key-2024-256bits-SuperSecure-VentureCapital-Platform-Authentication-Token";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("DecVCPlatJWT", options =>
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

        // Support SignalR connections
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hub") || path.StartsWithSegments("/hubs")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add Ocelot with Consul service discovery
builder.Services.AddOcelot()
    .AddConsul();

// Add Redis caching for performance
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "DecVCPlat-Gateway";
});

// Add Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Add CORS for DecVCPlat
builder.Services.AddCors(options =>
{
    options.AddPolicy("DecVCPlatCORS", policy =>
    {
        policy.WithOrigins(
                "https://localhost:3000", 
                "https://decvcplat.com", 
                "https://*.decvcplat.com",
                "https://app.decvcplat.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add rate limiting
builder.Services.AddMemoryCache();

// Add Application Insights for monitoring
builder.Services.AddApplicationInsightsTelemetry();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("DecVCPlat-Gateway", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("DecVCPlat Gateway is running"));

var app = builder.Build();

// Use HTTPS redirection for security
app.UseHttpsRedirection();

// Use CORS
app.UseCors("DecVCPlatCORS");

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; connect-src 'self' wss: ws:; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
    
    await next();
});

// Request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "DecVCPlat Gateway {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// Health check endpoint
app.MapHealthChecks("/health");

// Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
        opt.ReConfigureUpstreamSwaggerJson = AlterUpstreamSwaggerJson;
    });
}

// Authentication middleware
app.UseAuthentication();

// Custom middleware for DecVCPlat specific logging and monitoring
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    var requestId = Guid.NewGuid().ToString("N")[..8];
    
    context.Items["RequestId"] = requestId;
    context.Items["StartTime"] = startTime;
    
    // Add request ID to response headers for debugging
    context.Response.Headers.Add("X-Request-ID", requestId);
    
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "DecVCPlat Gateway error for request {RequestId}: {RequestMethod} {RequestPath}", 
            requestId, context.Request.Method, context.Request.Path);
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred processing your request");
    }
    finally
    {
        var duration = DateTime.UtcNow - startTime;
        Log.Information("DecVCPlat Gateway request {RequestId} completed in {Duration}ms", 
            requestId, duration.TotalMilliseconds);
    }
});

// Use Ocelot middleware
await app.UseOcelot();

Log.Information("DecVCPlat API Gateway started and ready to route traffic to microservices");
app.Run();

// Helper method to modify Swagger JSON for DecVCPlat branding
static string AlterUpstreamSwaggerJson(HttpContext context, string swaggerJson)
{
    var swagger = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(swaggerJson);
    
    if (swagger?.info != null)
    {
        swagger.info.title = "DecVCPlat API Gateway";
        swagger.info.description = "Unified API Gateway for DecVCPlat - Decentralized Venture Capital Platform";
        swagger.info.version = "v1.0";
        swagger.info.contact = new
        {
            name = "DecVCPlat Development Team",
            email = "dev@decvcplat.com",
            url = "https://decvcplat.com"
        };
    }
    
    return Newtonsoft.Json.JsonConvert.SerializeObject(swagger, Newtonsoft.Json.Formatting.Indented);
}
