using Afetto_.Net.Application.DTOs;

namespace Afetto_.Net.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
