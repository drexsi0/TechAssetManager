using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models;
using Microsoft.AspNetCore.Authorization;
using X.PagedList; // Importante para o StaticPagedList

namespace GerenciadorAtivos.Controllers
{
    [Authorize]
    public class AtivosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtivosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ativos
        public async Task<IActionResult> Index(int? pageNumber, string searchString, StatusAtivo? statusFilter, SetorAtivo? setorFilter, TipoAtivo? tipoFilter)
        {
            // 1. Guardar os filtros na ViewData para a View conseguir "lembrar" deles
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["SetorFilter"] = setorFilter;
            ViewData["TipoFilter"] = tipoFilter;

            var query = _context.Ativos.AsQueryable();

            // 2. Filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Nome.Contains(searchString) || s.Patrimonio.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(x => x.Status == statusFilter.Value);
            }

            if (setorFilter.HasValue)
            {
                // Agora buscamos pelo NÚMERO (ex: "1"), pois o Dropdown salva o ID
                string setorId = ((int)setorFilter.Value).ToString();
                query = query.Where(x => x.Setor == setorId);
            }

            if (tipoFilter.HasValue)
            {
                query = query.Where(x => x.Tipo == tipoFilter.Value);
            }

            // 3. Ordenação (Do mais novo para o mais antigo)
            query = query.OrderByDescending(x => x.Id);

            // 4. Paginação MANUAL (Resolve o erro CS1061 e é mais leve)
            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;

            // A. Conta quantos itens sobraram após os filtros
            var totalItemCount = await query.CountAsync();

            // B. Pega apenas os 10 itens da página atual
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // C. Cria a lista paginada que a View espera
            var listaPaginada = new StaticPagedList<Ativo>(items, pageIndex, pageSize, totalItemCount);

            return View(listaPaginada);
        }

        // --- MÉTODOS PADRÃO (Create, Edit, Delete, Details) ---
        // (Mantenha os outros métodos Create, Edit, Delete, Details exatamente como estavam no seu código anterior)
        // ...

        // GET: Ativos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ativo = await _context.Ativos.Include(a => a.Historicos).FirstOrDefaultAsync(m => m.Id == id);
            if (ativo == null) return NotFound();
            return View(ativo);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Patrimonio,Tipo,Marca,Modelo,Setor,Status")] Ativo ativo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ativo);
                await _context.SaveChangesAsync();
                await RegistrarHistorico(ativo.Id, "Criação", "Ativo cadastrado inicialmente.");
                return RedirectToAction(nameof(Index));
            }
            return View(ativo);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ativo = await _context.Ativos.FindAsync(id);
            if (ativo == null) return NotFound();
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Patrimonio,Tipo,Marca,Modelo,Setor,Status")] Ativo ativo)
        {
            if (id != ativo.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ativo);
                    await _context.SaveChangesAsync();
                    await RegistrarHistorico(ativo.Id, "Atualização", $"Status atual: {ativo.Status}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AtivoExists(ativo.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ativo);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ativo = await _context.Ativos.FirstOrDefaultAsync(m => m.Id == id);
            if (ativo == null) return NotFound();
            return View(ativo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ativo = await _context.Ativos.FindAsync(id);
            if (ativo != null) _context.Ativos.Remove(ativo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AtivoExists(int id) => _context.Ativos.Any(e => e.Id == id);

        private async Task RegistrarHistorico(int ativoId, string tipoAcao, string descricao)
        {
            var historico = new Historico
            {
                AtivoId = ativoId,
                TipoAcao = tipoAcao,
                Descricao = descricao,
                DataAcao = DateTime.Now,
                Usuario = User.Identity?.Name ?? "Sistema"
            };
            _context.Historicos.Add(historico);
            await _context.SaveChangesAsync();
        }
    }
}