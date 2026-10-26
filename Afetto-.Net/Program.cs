using Afetto_.Net.Data;
using Afetto_.Net.HealthChecks;
using Afetto_.Net.Repositories;
using Afetto_.Net.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Afetto_.Net.Middleware;

// ── SERILOG ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/petos-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando PetOS API...");

    var builder = WebApplication.CreateBuilder(args);

    // ── SERILOG no pipeline ───────────────────────────────────────────────────
    builder.Host.UseSerilog();

    // ── CONTROLLERS ───────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ── SWAGGER / OPENAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PetOS API",
            Version = "v1",
            Description = "API do sistema PetOS — gerenciamento de saúde contínua de pets."
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

    // ── ORACLE + EF CORE ──────────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

    // ── INJEÇÃO DE DEPENDÊNCIA ────────────────────────────────────────────────
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    // UsuarioService real + decorator que audita escritas no MongoDB
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<UsuarioService>();
    builder.Services.AddScoped<IUsuarioService>(sp => new AuditedUsuarioService(
        sp.GetRequiredService<UsuarioService>(),
        sp.GetRequiredService<IAuditLogRepository>(),
        sp.GetRequiredService<IHttpContextAccessor>(),
        sp.GetRequiredService<ILogger<AuditedUsuarioService>>()));

    // ── MONGODB (NoSQL: auditoria) ────────────────────────────────────────────
    // Connection string em appsettings "MongoDb:ConnectionString" ou variável de ambiente
    // MongoDb__ConnectionString (ex.: Atlas mongodb+srv://...), sem alterar código.
    builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDb"));
    builder.Services.AddSingleton<IMongoClient>(sp =>
    {
        var mongo = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
        var clientSettings = MongoClientSettings.FromConnectionString(mongo.ConnectionString);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(mongo.TimeoutSeconds);
        return new MongoClient(clientSettings);
    });
    builder.Services.AddSingleton<IAuditLogRepository, MongoAuditLogRepository>();
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // ── AUTENTICAÇÃO JWT ──────────────────────────────────────────────────────
    var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
              ?? throw new InvalidOperationException("Seção 'Jwt' ausente no appsettings.json.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    builder.Services.AddAuthorization();

    // ── HEALTH CHECKS ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
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
            failureStatus: HealthStatus.Degraded, // auditoria não é crítica: API segue funcionando
            tags: new[] { "database", "mongo", "ready" });

    // ── OPENTELEMETRY ─────────────────────────────────────────────────────────
    // Só ativa OpenTelemetry fora do ambiente de testes
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "PetOS.API", serviceVersion: "1.0.0"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true)
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddConsoleExporter());
    }

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

    // ── PIPELINE ──────────────────────────────────────────────────────────────
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PetOS API v1");
            options.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000}ms";
    });
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // ── HEALTH CHECK ENDPOINTS ────────────────────────────────────────────────
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds + "ms"
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("PetOS API iniciada com sucesso.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PetOS API encerrada inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}

// Necessário para WebApplicationFactory nos testes de integração
public partial class Program { }