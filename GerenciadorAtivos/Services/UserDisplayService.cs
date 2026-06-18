using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace GerenciadorAtivos.Services
{
    public class UserDisplayService : IUserDisplayService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserDisplayService(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public bool IsDemoMode =>
            _configuration.GetValue<bool>("DEMO_MODE")
            || string.Equals(Environment.GetEnvironmentVariable("DEMO_MODE"), "true", StringComparison.OrdinalIgnoreCase);

        public async Task<string> GetHeaderDisplayNameAsync(ClaimsPrincipal principal)
        {
            var claimName = GetClaimName(principal);
            if (!string.IsNullOrWhiteSpace(claimName))
            {
                return claimName;
            }

            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return IsDemoMode ? "Usuário" : "Usuário";
            }

            if (!IsDemoMode)
            {
                return user.Email ?? user.UserName ?? "Usuário";
            }

            return await _userManager.IsInRoleAsync(user, "Admin") ? "Administrador" : "Usuário";
        }

        public string GetUserListEmail(IdentityUser user, IEnumerable<string> roles)
        {
            if (!IsDemoMode)
            {
                return user.Email ?? user.UserName ?? user.Id;
            }

            return roles.Contains("Admin") ? "admin@techasset.local" : "usuario@techasset.local";
        }

        public string GetUserDisplayName(IdentityUser user, IEnumerable<string> roles)
        {
            var userName = GetIdentityUserName(user);
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName;
            }

            if (!IsDemoMode)
            {
                return user.Email ?? user.UserName ?? user.Id;
            }

            return roles.Contains("Admin") ? "Administrador" : "Usuário Demo";
        }

        public async Task<string> GetAuditUserDisplayNameAsync(string? userIdentifier)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                return "Sistema";
            }

            if (!IsDemoMode)
            {
                return userIdentifier;
            }

            if (string.Equals(userIdentifier, "Sistema", StringComparison.OrdinalIgnoreCase))
            {
                return "Sistema";
            }

            var user = await _userManager.FindByEmailAsync(userIdentifier)
                       ?? await _userManager.FindByNameAsync(userIdentifier);

            if (user == null)
            {
                return LooksLikeEmail(userIdentifier) ? "Usuário Demo" : userIdentifier;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return GetUserDisplayName(user, roles);
        }

        private static string? GetClaimName(ClaimsPrincipal principal)
        {
            var candidates = new[]
            {
                principal.FindFirstValue(ClaimTypes.GivenName),
                principal.FindFirstValue("given_name"),
                principal.FindFirstValue(ClaimTypes.Name),
                principal.FindFirstValue("name")
            };

            return candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && !LooksLikeEmail(value));
        }

        private static string? GetIdentityUserName(IdentityUser user)
        {
            return !LooksLikeEmail(user.UserName) ? user.UserName : null;
        }

        private static bool LooksLikeEmail(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Contains('@');
        }
    }
}
