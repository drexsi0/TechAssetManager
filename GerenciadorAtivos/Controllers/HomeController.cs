using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models;
using GerenciadorAtivos.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GerenciadorAtivos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalAtivos = await _context.Ativos.CountAsync();
            var disponiveis = await _context.Ativos.CountAsync(x => x.Status == StatusAtivo.Disponivel);
            var emUso = await _context.Ativos.CountAsync(x => x.Status == StatusAtivo.EmUso);
            var emManutencao = await _context.Ativos.CountAsync(x => x.Status == StatusAtivo.Manutencao);

            var ativosPorStatus = await _context.Ativos
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToDictionaryAsync(g => g.Status.HasValue ? g.Status.Value.ToString() : "Sem status", g => g.Total);

            var ativosPorSetor = await _context.Ativos
                .GroupBy(x => x.Setor)
                .Select(g => new { Setor = g.Key, Total = g.Count() })
                .ToDictionaryAsync(g => string.IsNullOrWhiteSpace(g.Setor) ? "Sem setor" : g.Setor, g => g.Total);

            var ativosPorTipo = await _context.Ativos
                .GroupBy(x => x.Tipo)
                .Select(g => new { Tipo = g.Key, Total = g.Count() })
                .ToDictionaryAsync(g => g.Tipo.ToString(), g => g.Total);

            var valorTotalInvestido = await _context.Ativos
                .Select(x => (decimal?)x.ValorCompra)
                .SumAsync() ?? 0m;
            var dadosDepreciacao = await _context.Ativos
                .Select(x => new { x.ValorCompra, x.DataCompra })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalAtivos = totalAtivos,
                EmUso = emUso,
                Disponiveis = disponiveis,
                EmManutencao = emManutencao,
                AtivosPorStatus = ativosPorStatus,
                AtivosPorSetor = ativosPorSetor,
                AtivosPorTipo = ativosPorTipo,
                ValorTotalInvestido = valorTotalInvestido,
                ValorTotalAtual = dadosDepreciacao.Sum(x => CalcularValorAtual(x.ValorCompra, x.DataCompra))
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static decimal CalcularValorAtual(decimal valorCompra, DateTime dataCompra)
        {
            var anosDeUso = (DateTime.Now - dataCompra).TotalDays / 365.0;
            var valorDepreciado = valorCompra * 0.20m * (decimal)anosDeUso;
            var valorFinal = valorCompra - valorDepreciado;
            return valorFinal < 0 ? 0 : valorFinal;
        }
    }
}
