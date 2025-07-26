using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DecVCPlat.Common.Security
{
    /// <summary>
    /// Extension methods for configuring security services and middleware
    /// </summary>
    public static class SecurityExtensions
    {
        /// <summary>
        /// Adds security services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register security settings
            services.Configure<SecuritySettings>(configuration.GetSection("Security"));
            
            // Register security service
            services.AddSingleton<ISecurityService, SecurityService>();
            
            // Add HTTP client factory for CAPTCHA verification and IP reputation checks
            services.AddHttpClient();
            
            return services;
        }
        
        /// <summary>
        /// Adds security middleware to the application pipeline
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The application builder for chaining</returns>
        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app, IConfiguration configuration)
        {
            var securitySettings = configuration.GetSection("Security").Get<SecuritySettings>();
            
            // Configure HTTPS redirection
            if (securitySettings?.EnableHttpsRedirection == true)
            {
                app.UseHttpsRedirection();
            }
            
            // Configure HSTS
            if (securitySettings?.EnableHsts == true)
            {
                app.UseHsts();
            }
            
            // Add security headers middleware
            app.Use(async (context, next) =>
            {
                // Add security headers
                var headers = context.Response.Headers;
                
                // X-Content-Type-Options
                headers.Add("X-Content-Type-Options", "nosniff");
                
                // X-Frame-Options
                headers.Add("X-Frame-Options", "DENY");
                
                // X-XSS-Protection
                if (securitySettings?.EnableXssProtection == true)
                {
                    headers.Add("X-XSS-Protection", "1; mode=block");
                }
                
                // Content-Security-Policy
                if (securitySettings?.EnableContentSecurityPolicy == true && !string.IsNullOrEmpty(securitySettings.ContentSecurityPolicy))
                {
                    headers.Add("Content-Security-Policy", securitySettings.ContentSecurityPolicy);
                }
                
                // Referrer-Policy
                headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                
                // Feature-Policy
                headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
                
                await next();
            });
            
            return app;
        }
    }
}
