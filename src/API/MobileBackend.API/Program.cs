using MobileBackend.API.Services;
using MobileBackend.API.Middleware;
using MobileBackend.API.Filters;
using MobileBackend.API.Logging;
using MobileBackend.Application;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Framework;
using MobileBackend.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using NLog;
using NLog.Web;

// Configure NLog before building the application
var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
logger.Info("=== ScanPet API Starting Up ===");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Clear default logging providers and use NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // ?? RAILWAY DEPLOYMENT: Convert DATABASE_URL to connection string format
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try
        {
            var uri = new Uri(databaseUrl);
            var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
            builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            logger.Info($"Database connection configured from DATABASE_URL");
        }
        catch (Exception ex)
        {
            logger.Warn(ex, "Failed to parse DATABASE_URL, using appsettings connection string");
        }
    }

    // ?? RAILWAY DEPLOYMENT: Read JWT keys from environment variables
    var jwtPrivateKey = Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY");
    var jwtPublicKey = Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY");
    if (!string.IsNullOrEmpty(jwtPrivateKey) && !string.IsNullOrEmpty(jwtPublicKey))
    {
        builder.Configuration["JwtSettings:PrivateKey"] = jwtPrivateKey;
        builder.Configuration["JwtSettings:PublicKey"] = jwtPublicKey;
        logger.Info("JWT keys loaded from environment variables");
    }

    // ?? CLEAN ARCHITECTURE: Register services from each layer using extension methods
    // This follows the Service Binder pattern for production-ready applications

    // 1. Application Layer - CQRS, Validation, Mapping
    builder.Services.AddApplication();

    // 2. Infrastructure Layer - Database, Repositories, Unit of Work
    builder.Services.AddInfrastructure(builder.Configuration);

    // 3. Framework Layer - Security Services (JWT, Password, BitManipulation)
    builder.Services.AddFramework(builder.Configuration);

    // 4. API Layer Services
    builder.Services.AddHttpContextAccessor(); // Required for CurrentUserService
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ILoggerService, LoggerService>();

    // 5. JWT Authentication Configuration
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var privateKeyBase64 = jwtSettings["PrivateKey"];
    var publicKeyBase64 = jwtSettings["PublicKey"];

    // Only configure JWT if keys are properly set (not placeholder values)
    if (!string.IsNullOrEmpty(publicKeyBase64) && 
        !publicKeyBase64.Contains("BASE64") && 
        !publicKeyBase64.Contains("PLACEHOLDER"))
    {
        try
        {
            // Decode Base64 to XML format
            var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            var publicKeyXml = System.Text.Encoding.UTF8.GetString(publicKeyBytes);
            
            // Import RSA key from XML
            var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048);
            rsa.FromXmlString(publicKeyXml);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                        IssuerSigningKey = new RsaSecurityKey(rsa),
                        ClockSkew = TimeSpan.Zero // Remove default 5-minute buffer
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Append("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            
                            var result = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                success = false,
                                message = "You are not authorized to access this resource. Please login."
                            });
                            
                            return context.Response.WriteAsync(result);
                        }
                    };
                });
            
            logger.Info("JWT authentication configured successfully");
        }
        catch (Exception ex)
        {
            logger.Warn(ex, "JWT authentication not configured - keys may be invalid. Auth endpoints will not work until keys are generated.");
        }
    }
    else
    {
        logger.Warn("JWT keys not configured (placeholder values detected). Run 'generate-jwt-keys.ps1' to generate keys.");
    }

    builder.Services.AddAuthorization();

    // 6. Controllers with Global Filters
    builder.Services.AddControllers(options =>
    {
        // Apply ValidateModel filter globally to all controllers
        options.Filters.Add<ValidateModelAttribute>();
    });

    // 7. API Documentation (Swagger/OpenAPI)
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "ScanPet Mobile Backend API",
            Version = "v1",
            Description = "Backend API for ScanPet mobile application with Clean Architecture, CQRS, and JWT authentication",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "ScanPet Team",
                Email = "support@scanpet.com"
            }
        });

        // Add JWT Authentication to Swagger
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

    // 8. CORS Configuration
    var corsSettings = builder.Configuration.GetSection("Cors");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" };
            
            if (allowedOrigins.Contains("*"))
                policy.AllowAnyOrigin();
            else
                policy.WithOrigins(allowedOrigins);

            policy.AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // ?? MIDDLEWARE PIPELINE CONFIGURATION
    // Order is important! Middleware executes in the order added.

    // 1. Global Exception Handler (should be first)
    app.UseGlobalExceptionHandler();

    // 2. Enhanced HTML Logging (development and production)
    if (app.Configuration.GetValue<bool>("NLogSettings:EnableHtmlLogging", true))
    {
        app.UseEnhancedLogging();
    }

    // 3. Swagger UI (development only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ScanPet API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "ScanPet API Documentation";
        });
    }

    // 4. HTTPS Redirection
    app.UseHttpsRedirection();

    // 5. CORS
    app.UseCors("AllowAll");

    // 6. Authentication (must be before Authorization)
    app.UseAuthentication();

    // 7. JWT Middleware (extracts user context)
    app.UseJwtMiddleware();

    // 8. Authorization
    app.UseAuthorization();

    // 9. Audit Logging (after authentication)
    app.UseAuditLogging();

    // 10. Map Controllers
    app.MapControllers();

    // ?? AUTOMATIC DATABASE MIGRATION & SEEDING
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<MobileBackend.Infrastructure.Data.ApplicationDbContext>();
            var passwordService = services.GetRequiredService<MobileBackend.Framework.Security.IPasswordService>();
            var dbLogger = services.GetRequiredService<ILogger<Program>>();
            
            dbLogger.LogInformation("Starting database migration and seeding...");
            
            // Apply pending migrations
            await context.Database.MigrateAsync();
            dbLogger.LogInformation("? Database migrations applied successfully");
            
            // Seed initial data
            await MobileBackend.Infrastructure.Data.DbSeeder.SeedAsync(context, passwordService, dbLogger);
            dbLogger.LogInformation("? Database seeded successfully");
            dbLogger.LogInformation("? Admin credentials: admin / Admin@123");
        }
        catch (Exception ex)
        {
            var dbLogger = services.GetRequiredService<ILogger<Program>>();
            dbLogger.LogError(ex, "? An error occurred during database migration or seeding");
            // Don't throw - allow app to start even if seeding fails
        }
    }

    // ?? Health Check Endpoint
    app.MapGet("/health", () => Results.Ok(new 
    { 
        status = "Healthy", 
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName,
        database = builder.Configuration["DatabaseProvider"] ?? "PostgreSQL",
        features = new
        {
            authentication = 3,
            userManagement = 4,
            colorManagement = 5,
            locationManagement = 5,
            itemManagement = 5,
            orderManagement = 5,
            roleManagement = 6
        },
        totalEndpoints = 33
    }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .WithTags("Health")
    .AllowAnonymous();

    logger.Info("=== ScanPet API Started Successfully ===");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "=== Application stopped due to exception ===");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
