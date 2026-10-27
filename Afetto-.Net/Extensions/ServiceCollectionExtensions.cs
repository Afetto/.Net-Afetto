using System.Text;
using Afetto_.Net.Application.Interfaces;
using Afetto_.Net.Application.Services;
using Afetto_.Net.Domain.Interfaces;
using Afetto_.Net.Infrastructure.HealthChecks;
using Afetto_.Net.Infrastructure.Mongo;
using Afetto_.Net.Infrastructure.Persistence;
using Afetto_.Net.Infrastructure.Repositories;
using Afetto_.Net.Infrastructure.Security;
using Afetto_.Net.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace Afetto_.Net.Extensions
{
    /// <summary>
    /// Composition root: cada método registra as dependências de uma camada,
    /// mantendo o Program.cs enxuto e deixando explícita a direção das dependências
    /// (API → Application → Domain; Infrastructure implementa as interfaces do Domain/Application).
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        // ── APPLICATION ───────────────────────────────────────────────────────
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // UsuarioService real + decorator que audita as escritas
            services.AddScoped<UsuarioService>();
            services.AddScoped<IUsuarioService>(sp => new AuditedUsuarioService(
                sp.GetRequiredService<UsuarioService>(),
                sp.GetRequiredService<IAuditLogRepository>(),
                sp.GetRequiredService<ICurrentUserService>(),
                sp.GetRequiredService<ILogger<AuditedUsuarioService>>()));

            services.AddScoped<IAuthService, AuthService>();
            return services;
        }

        // ── INFRASTRUCTURE ────────────────────────────────────────────────────
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Oracle + EF Core
            services.AddDbContext<AppDbContext>(options =>
                options.UseOracle(configuration.GetConnectionString("OracleConnection")));
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            // MongoDB (NoSQL: auditoria). Connection string em "MongoDb:ConnectionString" ou na
            // variável de ambiente MongoDb__ConnectionString (ex.: Atlas mongodb+srv://...).
            services.Configure<MongoSettings>(configuration.GetSection("MongoDb"));
            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongo = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
                var clientSettings = MongoClientSettings.FromConnectionString(mongo.ConnectionString);
                clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(mongo.TimeoutSeconds);
                return new MongoClient(clientSettings);
            });
            services.AddSingleton<IAuditLogRepository, MongoAuditLogRepository>();

            // JWT
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

        // ── API: usuário corrente (HttpContext → Application) ────────────────
        public static IServiceCollection AddCurrentUser(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
            return services;
        }

        // ── API: autenticação JWT ─────────────────────────────────────────────
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("Jwt").Get<JwtSettings>()
                      ?? throw new InvalidOperationException("Seção 'Jwt' ausente no appsettings.json.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            services.AddAuthorization();
            return services;
        }

        // ── API: Swagger / OpenAPI ────────────────────────────────────────────
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Afetto API",
                    Version = "v1",
                    Description = "API do sistema Afetto — gerenciamento de saúde contínua de pets."
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Informe apenas o token JWT (sem a palavra Bearer)."
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            return services;
        }

        // ── Health Checks ─────────────────────────────────────────────────────
        public static IServiceCollection AddAfettoHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<ApiHealthCheck>(
                    name: "api",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "api", "live" })
                .AddCheck<OracleHealthCheck>(
                    name: "oracle",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database", "oracle", "ready" })
                .AddCheck<MongoHealthCheck>(
                    name: "mongodb",
                    failureStatus: HealthStatus.Degraded, // auditoria não é crítica: a API segue funcionando
                    tags: new[] { "database", "mongo", "ready" });
            return services;
        }
    }
}
