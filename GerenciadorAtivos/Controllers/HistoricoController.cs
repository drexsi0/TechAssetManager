using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GerenciadorAtivos.Data;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace GerenciadorAtivos.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HistoricoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistoricoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? pageNumber, string searchString, string tipoAcao)
        {
            // Armazena filtros para a View manter os campos preenchidos
            ViewData["CurrentFilter"] = searchString;
            ViewData["TipoAcaoFilter"] = tipoAcao;

            var query = _context.Historicos
                .Include(h => h.Ativo)
                .OrderByDescending(h => h.DataAcao)
                .AsQueryable();

            // 1. Filtro por Texto (Usuário, Nome do Ativo ou Descrição)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => (h.Usuario != null && h.Usuario.Contains(searchString))
                                      || h.Descricao.Contains(searchString)
                                      || (h.Ativo != null && h.Ativo.Nome.Contains(searchString)));
            }

            // 2. Filtro por Tipo de Ação (Dropdown)
            if (!string.IsNullOrEmpty(tipoAcao))
            {
                query = query.Where(h => h.TipoAcao == tipoAcao);
            }

            // Paginação Manual
            int pageSize = 20;
            int pageIndex = pageNumber ?? 1;

            var totalItemCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listaPaginada = new StaticPagedList<GerenciadorAtivos.Models.Historico>(items, pageIndex, pageSize, totalItemCount);

            return View(listaPaginada);
        }
    }
}