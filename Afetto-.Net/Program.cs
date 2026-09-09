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
using System.Text.Json;

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
    });

    // ── ORACLE + EF CORE ──────────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

    // ── INJEÇÃO DE DEPENDÊNCIA ────────────────────────────────────────────────
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();

    // ── HEALTH CHECKS ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddCheck<ApiHealthCheck>(
            name: "api",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "api", "live" })
        .AddCheck<OracleHealthCheck>(
            name: "oracle",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "database", "oracle", "ready" });

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