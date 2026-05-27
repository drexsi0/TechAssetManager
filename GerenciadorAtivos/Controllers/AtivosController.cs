using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace GerenciadorAtivos.Controllers
{
    [Authorize]
    public class AtivosController : Controller
    {
        private const string InventoryManagerRoles = "Admin,Manager";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AtivosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? pageNumber, string? searchString, StatusAtivo? statusFilter, SetorAtivo? setorFilter, TipoAtivo? tipoFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["SetorFilter"] = setorFilter;
            ViewData["TipoFilter"] = tipoFilter;

            var query = _context.Ativos
                .Include(a => a.Responsavel)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(s => s.Nome.Contains(searchString) || s.Patrimonio.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(x => x.Status == statusFilter.Value);
            }

            if (setorFilter.HasValue)
            {
                var setorId = ((int)setorFilter.Value).ToString();
                query = query.Where(x => x.Setor == setorId);
            }

            if (tipoFilter.HasValue)
            {
                query = query.Where(x => x.Tipo == tipoFilter.Value);
            }

            query = query.OrderByDescending(x => x.Id);

            const int pageSize = 10;
            var pageIndex = pageNumber ?? 1;
            var totalItemCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(new StaticPagedList<Ativo>(items, pageIndex, pageSize, totalItemCount));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ativo = await _context.Ativos
                .Include(a => a.Responsavel)
                .Include(a => a.Historicos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ativo == null) return NotFound();
            return View(ativo);
        }

        [Authorize(Roles = InventoryManagerRoles)]
        public async Task<IActionResult> Create()
        {
            await PopularResponsaveisAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = InventoryManagerRoles)]
        public async Task<IActionResult> Create([Bind("Id,Nome,Patrimonio,Tipo,Marca,Modelo,Setor,Status,ResponsavelId,ValorCompra,DataCompra")] Ativo ativo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ativo);
                await _context.SaveChangesAsync();

                await RegistrarHistorico(ativo.Id, "Criação", "Ativo cadastrado inicialmente.");

                if (!string.IsNullOrWhiteSpace(ativo.ResponsavelId))
                {
                    var responsavel = await _userManager.FindByIdAsync(ativo.ResponsavelId);
                    await RegistrarHistorico(ativo.Id, "Atribuição", $"Responsável definido como {responsavel?.Email ?? ativo.ResponsavelId}.");
                }

                return RedirectToAction(nameof(Index));
            }

            await PopularResponsaveisAsync(ativo.ResponsavelId);
            return View(ativo);
        }

        [Authorize(Roles = InventoryManagerRoles)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ativo = await _context.Ativos.FindAsync(id);
            if (ativo == null) return NotFound();

            await PopularResponsaveisAsync(ativo.ResponsavelId);
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = InventoryManagerRoles)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Patrimonio,Tipo,Marca,Modelo,Setor,Status,ResponsavelId,ValorCompra,DataCompra")] Ativo ativo)
        {
            if (id != ativo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var responsavelAnterior = await _context.Ativos
                    .AsNoTracking()
                    .Where(a => a.Id == id)
                    .Select(a => a.ResponsavelId)
                    .FirstOrDefaultAsync();

                try
                {
                    _context.Update(ativo);
                    await _context.SaveChangesAsync();
                    await RegistrarHistorico(ativo.Id, "Atualização", $"Status atual: {ativo.Status}");

                    if (responsavelAnterior != ativo.ResponsavelId)
                    {
                        var responsavel = string.IsNullOrWhiteSpace(ativo.ResponsavelId)
                            ? null
                            : await _userManager.FindByIdAsync(ativo.ResponsavelId);

                        var descricao = responsavel == null
                            ? "Responsável removido."
                            : $"Responsável alterado para {responsavel.Email}.";

                        await RegistrarHistorico(ativo.Id, "Atribuição", descricao);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtivoExists(ativo.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            await PopularResponsaveisAsync(ativo.ResponsavelId);
            return View(ativo);
        }

        [Authorize(Roles = InventoryManagerRoles)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ativo = await _context.Ativos
                .Include(a => a.Responsavel)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ativo == null) return NotFound();
            return View(ativo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = InventoryManagerRoles)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ativo = await _context.Ativos.FindAsync(id);
            if (ativo != null)
            {
                _context.Ativos.Remove(ativo);
                await _context.SaveChangesAsync();
                await RegistrarHistorico(id, "Exclusão", "Ativo removido por exclusão lógica.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AtivoExists(int id) => _context.Ativos.Any(e => e.Id == id);

        private async Task PopularResponsaveisAsync(string? selectedUserId = null)
        {
            var usuarios = await _userManager.Users
                .OrderBy(u => u.Email)
                .Select(u => new { u.Id, Nome = u.Email ?? u.UserName ?? u.Id })
                .ToListAsync();

            ViewBag.ResponsavelId = new SelectList(usuarios, "Id", "Nome", selectedUserId);
        }

        private async Task RegistrarHistorico(int ativoId, string tipoAcao, string descricao)
        {
            var historico = new Historico
            {
                AtivoId = ativoId,
                TipoAcao = tipoAcao,
                Descricao = descricao,
                DataAcao = DateTime.UtcNow,
                Usuario = User.Identity?.Name ?? "Sistema"
            };

            _context.Historicos.Add(historico);
            await _context.SaveChangesAsync();
        }
    }
}
