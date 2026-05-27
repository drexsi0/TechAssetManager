using ClosedXML.Excel;
using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models; // Importante para os Enums
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorAtivos.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class RelatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RelatoriosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // A Mágica do Excel
        public async Task<IActionResult> ExportarAtivosExcel()
        {
            // 1. Busca os dados no banco
            var ativos = await _context.Ativos
                .Include(a => a.Responsavel)
                .ToListAsync();

            // 2. Cria o arquivo Excel na memória
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ativos");

                // 3. Cria o Cabeçalho (Adicionando colunas novas)
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "Patrimônio";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Setor";
                worksheet.Cell(1, 6).Value = "Status";
                worksheet.Cell(1, 7).Value = "Marca/Modelo";
                worksheet.Cell(1, 8).Value = "Responsável";
                worksheet.Cell(1, 9).Value = "Data Compra";
                worksheet.Cell(1, 10).Value = "Valor Pago";
                worksheet.Cell(1, 11).Value = "Valor Atual";

                // Estiliza o cabeçalho
                var header = worksheet.Range("A1:K1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;

                // 4. Preenche as linhas
                int linha = 2;
                foreach (var item in ativos)
                {
                    worksheet.Cell(linha, 1).Value = item.Id;
                    worksheet.Cell(linha, 2).Value = item.Nome;
                    worksheet.Cell(linha, 3).Value = item.Patrimonio;
                    worksheet.Cell(linha, 4).Value = item.Tipo.ToString();

                    // Lógica do Setor (mantém a que você já tem)
                    if (Enum.TryParse(item.Setor, out SetorAtivo setorEnum))
                    {
                        worksheet.Cell(linha, 5).Value = setorEnum.ToString();
                    }
                    else
                    {
                        worksheet.Cell(linha, 5).Value = item.Setor;
                    }

                    worksheet.Cell(linha, 6).Value = item.Status.ToString();
                    worksheet.Cell(linha, 7).Value = $"{item.Marca} {item.Modelo}";
                    worksheet.Cell(linha, 8).Value = item.Responsavel?.Email ?? "Sem responsável";

                    worksheet.Cell(linha, 9).Value = item.DataCompra;
                    worksheet.Cell(linha, 10).Value = item.ValorCompra;
                    worksheet.Cell(linha, 10).Style.NumberFormat.Format = "R$ #,##0.00";

                    worksheet.Cell(linha, 11).Value = item.ValorAtual;
                    worksheet.Cell(linha, 11).Style.NumberFormat.Format = "R$ #,##0.00";

                    // Pinta status Manutenção (mantém sua lógica)
                    if (item.Status == StatusAtivo.Manutencao)
                    {
                        worksheet.Cell(linha, 6).Style.Font.FontColor = XLColor.Red;
                    }

                    linha++;
                }

                // Ajusta a largura das colunas automaticamente
                worksheet.Columns().AdjustToContents();

                // 5. Prepara o download
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Relatorio_Ativos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
    }
}
