using System.Security.Claims;
using Afetto_.Net.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Afetto_.Net.Security
{
    /// <summary>Implementação de <see cref="ICurrentUserService"/> baseada nas claims do JWT da requisição.</summary>
    public class HttpCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public HttpCurrentUserService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string Identificador
        {
            get
            {
                var user = _http.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return "anonimo";

                return user.FindFirst(ClaimTypes.Email)?.Value
                       ?? user.FindFirst("email")?.Value
                       ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? "desconhecido";
            }
        }
    }
}
