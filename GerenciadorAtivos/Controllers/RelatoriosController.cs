using ClosedXML.Excel;
using GerenciadorAtivos.Data;
using GerenciadorAtivos.Models; // Importante para os Enums
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        public async Task<IActionResult> ExportarAtivosPdf()
        {
            var ativos = await BuscarAtivosRelatorioAsync();
            var emitidoEm = DateTime.Now;
            var totalInvestido = ativos.Sum(a => a.ValorCompra);
            var valorAtual = ativos.Sum(a => a.ValorAtual);

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(28);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("TechAsset Manager").FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                                col.Item().Text("Relatório executivo de ativos").FontSize(11).FontColor(Colors.Grey.Darken2);
                            });
                            row.ConstantItem(190).AlignRight().Text($"Gerado em {emitidoEm:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(12).Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => SummaryCard(c, "Total de ativos", ativos.Count.ToString(), Colors.Blue.Darken2));
                            row.RelativeItem().Element(c => SummaryCard(c, "Investimento total", totalInvestido.ToString("C"), Colors.Green.Darken2));
                            row.RelativeItem().Element(c => SummaryCard(c, "Valor atual", valorAtual.ToString("C"), Colors.Teal.Darken2));
                            row.RelativeItem().Element(c => SummaryCard(c, "Depreciação", (totalInvestido - valorAtual).ToString("C"), Colors.Red.Darken2));
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.4f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                HeaderCell(header, "Nome");
                                HeaderCell(header, "Patrimônio");
                                HeaderCell(header, "Tipo");
                                HeaderCell(header, "Setor");
                                HeaderCell(header, "Status");
                                HeaderCell(header, "Responsável");
                                HeaderCell(header, "Compra");
                                HeaderCell(header, "Atual");
                            });

                            foreach (var ativo in ativos)
                            {
                                BodyCell(table, ativo.Nome);
                                BodyCell(table, ativo.Patrimonio);
                                BodyCell(table, ativo.Tipo.ToString());
                                BodyCell(table, ObterSetor(ativo.Setor));
                                BodyCell(table, ativo.Status?.ToString() ?? "Sem status");
                                BodyCell(table, ativo.Responsavel?.Email ?? "Sem responsável");
                                BodyCell(table, ativo.ValorCompra.ToString("C"));
                                BodyCell(table, ativo.ValorAtual.ToString("C"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("TechAsset Manager · ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Relatorio_Ativos_{emitidoEm:yyyyMMdd_HHmm}.pdf");
        }

        // A Mágica do Excel
        public async Task<IActionResult> ExportarAtivosExcel()
        {
            // 1. Busca os dados no banco
            var ativos = await BuscarAtivosRelatorioAsync();

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

        private Task<List<Ativo>> BuscarAtivosRelatorioAsync()
        {
            return _context.Ativos
                .Include(a => a.Responsavel)
                .OrderBy(a => a.Nome)
                .ToListAsync();
        }

        private static string ObterSetor(string setor)
        {
            return Enum.TryParse(setor, out SetorAtivo setorEnum) ? setorEnum.ToString() : setor;
        }

        private static void SummaryCard(IContainer container, string label, string value, string color)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(4).Text(value).FontSize(13).Bold().FontColor(color);
            });
        }

        private static void HeaderCell(TableCellDescriptor header, string text)
        {
            header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text(text).FontColor(Colors.White).Bold();
        }

        private static void BodyCell(TableDescriptor table, string text)
        {
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(text);
        }
    }
}
