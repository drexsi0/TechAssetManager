using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models;
using GerenciadorAtivos.Models.ViewModels;
using GerenciadorAtivos.Services;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace GerenciadorAtivos.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class HistoricoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserDisplayService _userDisplayService;

        public HistoricoController(ApplicationDbContext context, IUserDisplayService userDisplayService)
        {
            _context = context;
            _userDisplayService = userDisplayService;
        }

        public async Task<IActionResult> Index(int? pageNumber, string searchString, string tipoAcao)
        {
            // Armazena filtros para a View manter os campos preenchidos
            ViewData["CurrentFilter"] = searchString;
            ViewData["TipoAcaoFilter"] = tipoAcao;

            var query = CriarConsultaHistorico(searchString, tipoAcao);

            // Paginação Manual
            int pageSize = 20;
            int pageIndex = pageNumber ?? 1;

            var totalItemCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewItems = await MapearHistoricosAsync(items);
            var listaPaginada = new StaticPagedList<HistoricoViewModel>(viewItems, pageIndex, pageSize, totalItemCount);

            return View(listaPaginada);
        }

        public async Task<IActionResult> ExportarHistoricoTxt(string searchString, string tipoAcao)
        {
            var emitidoEm = DateTime.Now;
            var registros = await CriarConsultaHistorico(searchString, tipoAcao).ToListAsync();
            var builder = new StringBuilder();

            builder.AppendLine("TechAsset Manager");
            builder.AppendLine("Relatorio de auditoria");
            builder.AppendLine($"Gerado em: {emitidoEm:dd/MM/yyyy HH:mm}");
            builder.AppendLine($"Filtro de busca: {(string.IsNullOrWhiteSpace(searchString) ? "Todos" : searchString)}");
            builder.AppendLine($"Tipo de acao: {(string.IsNullOrWhiteSpace(tipoAcao) ? "Todas" : tipoAcao)}");
            builder.AppendLine($"Total de registros: {registros.Count}");
            builder.AppendLine(new string('-', 100));

            foreach (var item in registros)
            {
                var usuarioDisplay = await _userDisplayService.GetAuditUserDisplayNameAsync(item.Usuario);
                builder.AppendLine($"Data/Hora: {item.DataAcao:dd/MM/yyyy HH:mm}");
                builder.AppendLine($"Usuario: {usuarioDisplay}");
                builder.AppendLine($"Acao: {item.TipoAcao}");
                builder.AppendLine($"Ativo: {(item.Ativo == null ? "Ativo excluido" : $"{item.Ativo.Nome} ({item.Ativo.Patrimonio})")}");
                builder.AppendLine($"Detalhes: {item.Descricao}");
                builder.AppendLine(new string('-', 100));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            return File(bytes, "text/plain; charset=utf-8", $"Auditoria_TechAsset_{emitidoEm:yyyyMMdd_HHmm}.txt");
        }

        private IQueryable<GerenciadorAtivos.Models.Historico> CriarConsultaHistorico(string searchString, string tipoAcao)
        {
            var query = _context.Historicos
                .Include(h => h.Ativo)
                .OrderByDescending(h => h.DataAcao)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(h => (h.Usuario != null && h.Usuario.Contains(searchString))
                                      || h.Descricao.Contains(searchString)
                                      || (h.Ativo != null && h.Ativo.Nome.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(tipoAcao))
            {
                query = query.Where(h => h.TipoAcao == tipoAcao);
            }

            return query;
        }

        private async Task<List<HistoricoViewModel>> MapearHistoricosAsync(IEnumerable<Historico> historicos)
        {
            var resultado = new List<HistoricoViewModel>();

            foreach (var historico in historicos)
            {
                resultado.Add(new HistoricoViewModel
                {
                    Id = historico.Id,
                    AtivoId = historico.AtivoId,
                    Ativo = historico.Ativo,
                    DataAcao = historico.DataAcao,
                    TipoAcao = historico.TipoAcao,
                    Descricao = historico.Descricao,
                    Usuario = historico.Usuario,
                    UsuarioDisplay = await _userDisplayService.GetAuditUserDisplayNameAsync(historico.Usuario)
                });
            }

            return resultado;
        }
    }
}
