using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace GerenciadorAtivos.Services
{
    public interface IUserDisplayService
    {
        bool IsDemoMode { get; }
        Task<string> GetHeaderDisplayNameAsync(ClaimsPrincipal principal);
        string GetUserListEmail(IdentityUser user, IEnumerable<string> roles);
        string GetUserDisplayName(IdentityUser user, IEnumerable<string> roles);
        Task<string> GetAuditUserDisplayNameAsync(string? userIdentifier);
    }
}
