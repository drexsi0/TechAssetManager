using ClosedXML.Excel;
using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models; // Importante para os Enums
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorAtivos.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var ativos = await _context.Ativos.ToListAsync();

            // 2. Cria o arquivo Excel na memória
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ativos");

                // 3. Cria o Cabeçalho
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "Patrimônio";
                worksheet.Cell(1, 4).Value = "Tipo";
                worksheet.Cell(1, 5).Value = "Setor"; // Vamos traduzir o ID para Nome
                worksheet.Cell(1, 6).Value = "Status";
                worksheet.Cell(1, 7).Value = "Marca/Modelo";

                // Estiliza o cabeçalho (Negrito e Fundo Cinza)
                var header = worksheet.Range("A1:G1");
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

                    // TRADUÇÃO DO SETOR (ID -> Nome)
                    // Como não temos o HtmlHelper aqui, fazemos no braço ou convertemos o Enum
                    // Assumindo que o ID salvo bate com o Enum:
                    if (Enum.TryParse(item.Setor, out SetorAtivo setorEnum))
                    {
                        // Pega o nome do Enum (Ex: "Desenvolvimento")
                        // Se quiser o DisplayName (com espaços), precisaria de um Helper extra, 
                        // mas para Excel rápido, o .ToString() resolve 90%
                        worksheet.Cell(linha, 5).Value = setorEnum.ToString();
                    }
                    else
                    {
                        worksheet.Cell(linha, 5).Value = item.Setor;
                    }

                    worksheet.Cell(linha, 6).Value = item.Status.ToString();
                    worksheet.Cell(linha, 7).Value = $"{item.Marca} {item.Modelo}";

                    // Pinta a célula de status se estiver em Manutenção (Exemplo de condicional)
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