using Microsoft.AspNetCore.Identity;

namespace GerenciadorAtivos.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roles = { "Admin", "Manager", "User" };

            // 1. Cria os Perfis (Roles) se não existirem
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            // 2. Cria um usuário Admin inicial somente quando as credenciais vierem do ambiente.
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var novoAdmin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(novoAdmin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(novoAdmin, "Admin");
                }
            }
        }
    }
}
