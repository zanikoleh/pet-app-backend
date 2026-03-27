using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using FileService.Application;
using FileService.Infrastructure;
using InfrastructureWeb.Middleware;
using InfrastructureWeb;
using SharedKernel.Infrastructure;
using Observability;
using System.Text;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add observability
builder.AddObservability("file-service");

// Add structured logging
builder.AddStructuredLogging("file-service");

// Add services
builder.Services.AddControllers();

// Add API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File Service API",
        Version = "v1",
        Description = "File upload, storage, and management service"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter 'Bearer' followed by space and JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer")
            {
                Reference = new OpenApiReferenceWithDescription
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                },
            },
            Array.Empty<string>().ToList()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// Add application services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey") ?? "";
var issuer = jwtSettings.GetValue<string>("Issuer") ?? "";
var audience = jwtSettings.GetValue<string>("Audience") ?? "";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

// Add secure CORS policies
builder.Services.AddSecureCors(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCorrelationIdMiddleware();
app.UseExceptionHandling();
app.UseTraceContextPropagation();

// Apply security middleware in order
app.UseSecurityHeaders();
app.UseRateLimiting();
app.UseInputValidation();
app.UseCorsPolicyValidation();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable HTTPS redirect in Docker - internal communication uses HTTP
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseSecureCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<FileService.Infrastructure.Persistence.FileServiceDbContext>().Database.Migrate();
}

// Log service startup
Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] File Service instance started successfully");
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("File Service instance started successfully at {StartTime}", DateTime.UtcNow);

app.Run();

#pragma warning disable CS1591
namespace FileService.Api { public class FileApiMarker { } }
