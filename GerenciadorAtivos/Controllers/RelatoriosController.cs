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

        public async Task<IActionResult> ExportarAtivosExcel()
        {
            var ativos = await BuscarAtivosRelatorioAsync();
            var emitidoEm = DateTime.Now;
            var totalInvestido = ativos.Sum(a => a.ValorCompra);
            var valorAtual = ativos.Sum(a => a.ValorAtual);
            var depreciacao = totalInvestido - valorAtual;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventario");

                worksheet.Cell("A1").Value = "TechAsset Manager";
                worksheet.Cell("A2").Value = "Relatorio geral de ativos";
                worksheet.Cell("A3").Value = $"Emitido em {emitidoEm:dd/MM/yyyy HH:mm}";

                worksheet.Range("A1:K1").Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(18)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#0B5ED7"));

                worksheet.Range("A2:K2").Merge().Style
                    .Font.SetBold()
                    .Font.SetFontSize(12)
                    .Font.SetFontColor(XLColor.FromHtml("#172033"))
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#EAF2FF"));

                worksheet.Range("A3:K3").Merge().Style
                    .Font.SetFontColor(XLColor.FromHtml("#637083"))
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#EAF2FF"));

                worksheet.Cell("A5").Value = "Total de ativos";
                worksheet.Cell("B5").Value = ativos.Count;
                worksheet.Cell("D5").Value = "Investimento total";
                worksheet.Cell("E5").Value = totalInvestido;
                worksheet.Cell("G5").Value = "Valor atual";
                worksheet.Cell("H5").Value = valorAtual;
                worksheet.Cell("J5").Value = "Depreciacao";
                worksheet.Cell("K5").Value = depreciacao;

                var summaryRange = worksheet.Range("A5:K5");
                summaryRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F7FB");
                summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                summaryRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D9E2EC");
                summaryRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D9E2EC");
                worksheet.Range("E5:E5").Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range("H5:H5").Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range("K5:K5").Style.NumberFormat.Format = "R$ #,##0.00";
                worksheet.Range("A5,D5,G5,J5").Style.Font.Bold = true;

                var headers = new[]
                {
                    "ID",
                    "Nome",
                    "Patrimonio",
                    "Tipo",
                    "Setor",
                    "Status",
                    "Marca/Modelo",
                    "Responsavel",
                    "Data Compra",
                    "Valor Pago",
                    "Valor Atual"
                };

                for (var i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(7, i + 1).Value = headers[i];
                }

                var header = worksheet.Range("A7:K7");
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.White;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0B5ED7");
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int linha = 8;
                foreach (var item in ativos)
                {
                    worksheet.Cell(linha, 1).Value = item.Id;
                    worksheet.Cell(linha, 2).Value = item.Nome;
                    worksheet.Cell(linha, 3).Value = item.Patrimonio;
                    worksheet.Cell(linha, 4).Value = item.Tipo.ToString();
                    worksheet.Cell(linha, 5).Value = ObterSetor(item.Setor);
                    worksheet.Cell(linha, 6).Value = item.Status?.ToString() ?? "Sem status";
                    worksheet.Cell(linha, 7).Value = $"{item.Marca} {item.Modelo}";
                    worksheet.Cell(linha, 8).Value = item.Responsavel?.Email ?? "Sem responsável";
                    worksheet.Cell(linha, 9).Value = item.DataCompra;
                    worksheet.Cell(linha, 10).Value = item.ValorCompra;
                    worksheet.Cell(linha, 11).Value = item.ValorAtual;

                    worksheet.Cell(linha, 9).Style.DateFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(linha, 10).Style.NumberFormat.Format = "R$ #,##0.00";
                    worksheet.Cell(linha, 11).Style.NumberFormat.Format = "R$ #,##0.00";

                    var row = worksheet.Range(linha, 1, linha, 11);
                    row.Style.Fill.BackgroundColor = linha % 2 == 0 ? XLColor.White : XLColor.FromHtml("#F8FAFC");

                    if (item.Status == StatusAtivo.Manutencao)
                    {
                        worksheet.Cell(linha, 6).Style.Font.FontColor = XLColor.FromHtml("#B45309");
                        worksheet.Cell(linha, 6).Style.Font.Bold = true;
                    }
                    else if (item.Status == StatusAtivo.Disponivel)
                    {
                        worksheet.Cell(linha, 6).Style.Font.FontColor = XLColor.FromHtml("#198754");
                        worksheet.Cell(linha, 6).Style.Font.Bold = true;
                    }
                    else if (item.Status == StatusAtivo.Descartado)
                    {
                        worksheet.Cell(linha, 6).Style.Font.FontColor = XLColor.FromHtml("#DC3545");
                        worksheet.Cell(linha, 6).Style.Font.Bold = true;
                    }

                    linha++;
                }

                var tableRange = worksheet.Range(7, 1, Math.Max(linha - 1, 7), 11);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D9E2EC");
                tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D9E2EC");
                tableRange.SetAutoFilter();

                worksheet.SheetView.FreezeRows(7);
                worksheet.Columns().AdjustToContents();
                worksheet.Columns(1, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 24);
                worksheet.Column(8).Width = Math.Max(worksheet.Column(8).Width, 28);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Relatorio_Ativos_{emitidoEm:yyyyMMdd_HHmm}.xlsx";

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
