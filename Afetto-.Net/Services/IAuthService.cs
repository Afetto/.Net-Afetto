using Afetto_.Net.DTOs;

namespace Afetto_.Net.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
