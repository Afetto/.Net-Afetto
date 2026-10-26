using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Afetto_.Net.Middleware
{
    /// <summary>
    /// Middleware global de tratamento de exceções.
    /// Converte qualquer exceção não tratada em uma resposta ProblemDetails (RFC 7807)
    /// com formato consistente, evitando try/catch espalhado pelos controllers.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Exceção após o início da resposta; não é possível reescrevê-la.");
                    throw;
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (status, title) = ex switch
            {
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Argumento inválido"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
                DbUpdateException => (StatusCodes.Status409Conflict, "Conflito ao salvar os dados"),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
            };

            if (status >= 500)
                _logger.LogError(ex, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("Erro de negócio em {Method} {Path}: {Message}",
                    context.Request.Method, context.Request.Path, ex.Message);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                // Em produção não vazamos detalhes internos em erros 500
                Detail = status >= 500 && !_env.IsDevelopment()
                               ? "Ocorreu um erro inesperado. Tente novamente mais tarde."
                               : ex.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
        }
    }
}