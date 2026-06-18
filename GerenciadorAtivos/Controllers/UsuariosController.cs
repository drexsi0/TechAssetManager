using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models.ViewModels;
using GerenciadorAtivos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorAtivos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private static readonly string[] SupportedRoles = { "Admin", "Manager", "User" };

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserDisplayService _userDisplayService;

        public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IUserDisplayService userDisplayService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _userDisplayService = userDisplayService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var model = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? user.Id,
                    DisplayEmail = _userDisplayService.GetUserListEmail(user, roles),
                    DisplayName = _userDisplayService.GetUserDisplayName(user, roles),
                    CurrentRole = roles.FirstOrDefault() ?? "User",
                    SelectedRole = roles.FirstOrDefault() ?? "User"
                });
            }

            ViewBag.Roles = SupportedRoles.Select(role => new SelectListItem(role, role)).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string selectedRole)
        {
            if (!SupportedRoles.Contains(selectedRole))
            {
                return BadRequest("Perfil invalido.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("Admin") && selectedRole != "Admin" && await EhUltimoAdminAsync(user.Id))
            {
                TempData["StatusMessage"] = "Nao e permitido remover o perfil do ultimo administrador.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _roleManager.RoleExistsAsync(selectedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(selectedRole));
            }

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, selectedRole);
            TempData["StatusMessage"] = $"Perfil de {_userDisplayService.GetUserDisplayName(user, currentRoles)} atualizado para {selectedRole}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["StatusMessage"] = "Nao e permitido excluir a propria conta logada.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["StatusMessage"] = "Usuario nao encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin") && await EhUltimoAdminAsync(user.Id))
            {
                TempData["StatusMessage"] = "Nao e permitido excluir o ultimo administrador do sistema.";
                return RedirectToAction(nameof(Index));
            }

            var ativosDoUsuario = await _context.Ativos
                .Where(a => a.ResponsavelId == userId)
                .ToListAsync();

            foreach (var ativo in ativosDoUsuario)
            {
                ativo.ResponsavelId = null;
            }

            var displayName = _userDisplayService.GetUserDisplayName(user, await _userManager.GetRolesAsync(user));
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = "Nao foi possivel excluir o usuario.";
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Usuario {displayName} excluido com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> EhUltimoAdminAsync(string userId)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return admins.Count == 1 && admins[0].Id == userId;
        }
    }
}
