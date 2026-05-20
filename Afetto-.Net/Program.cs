using Microsoft.EntityFrameworkCore;
using Afetto_.Net.Data;
using Afetto_.Net.Repositories;
using Afetto_.Net.Services;

var builder = WebApplication.CreateBuilder(args);

// ── CONTROLLERS ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── SWAGGER / OPENAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── ORACLE + EF CORE ──────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// ── INJEÇÃO DE DEPENDÊNCIA ────────────────────────────────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// ── PIPELINE ──────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Afetto API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();