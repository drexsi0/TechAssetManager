using System.Globalization;

namespace GerenciadorAtivos.Services
{
    public interface IUiTextService
    {
        string this[string key] { get; }
        string CultureName { get; }
        bool IsEnglish { get; }
    }

    public class UiTextService : IUiTextService
    {
        private static readonly Dictionary<string, string> PtBr = new()
        {
            ["Nav.Home"] = "Início",
            ["Nav.Inventory"] = "Inventário",
            ["Nav.Audit"] = "Auditoria",
            ["Nav.Reports"] = "Relatórios",
            ["Nav.Users"] = "Usuários",
            ["Nav.Privacy"] = "Privacidade",
            ["Nav.Language"] = "Idioma",
            ["Nav.Theme"] = "Tema",
            ["Nav.Light"] = "Claro",
            ["Nav.Dark"] = "Escuro",
            ["Dashboard.Title"] = "Painel de controle",
            ["Dashboard.Subtitle"] = "Resumo operacional do inventário de ativos de TI",
            ["Dashboard.InventoryHealth"] = "Saúde do inventário",
            ["Dashboard.TotalAssets"] = "Total de ativos",
            ["Dashboard.Available"] = "Disponíveis",
            ["Dashboard.InUse"] = "Em uso",
            ["Dashboard.Maintenance"] = "Manutenção",
            ["Dashboard.Registered"] = "Cadastrados",
            ["Dashboard.InStock"] = "No estoque",
            ["Dashboard.WithUsers"] = "Com colaboradores",
            ["Dashboard.RequiresAttention"] = "Requer atenção",
            ["Dashboard.TotalInvested"] = "Investimento total",
            ["Dashboard.CurrentValue"] = "Valor atual estimado",
            ["Dashboard.Depreciation"] = "Depreciação estimada",
            ["Dashboard.BySector"] = "Ativos por setor",
            ["Dashboard.ByStatus"] = "Distribuição por status",
            ["Dashboard.ByType"] = "Ativos por tipo",
            ["Dashboard.ValueSummary"] = "Resumo patrimonial",
            ["Dashboard.OperationalAlerts"] = "Alertas operacionais",
            ["Dashboard.NoAlerts"] = "Nenhum alerta crítico no momento.",
            ["Dashboard.ExportReports"] = "Exportar relatórios",
            ["Dashboard.ManageInventory"] = "Gerenciar inventário",
            ["Reports.Title"] = "Central de relatórios",
            ["Reports.Subtitle"] = "Exporte dados para análise, auditoria e apresentação",
            ["Reports.ExcelTitle"] = "Relatório geral em Excel",
            ["Reports.ExcelDescription"] = "Exporta a lista completa de ativos com status, setor, responsável e valores patrimoniais.",
            ["Reports.ExcelButton"] = "Baixar Excel",
            ["Reports.PdfTitle"] = "Relatório executivo em PDF",
            ["Reports.PdfDescription"] = "Gera um documento pronto para impressão com resumo financeiro e inventário detalhado.",
            ["Reports.PdfButton"] = "Baixar PDF",
            ["Privacy.Title"] = "Política de privacidade",
            ["Error.Title"] = "Erro",
            ["Error.Message"] = "Ocorreu um erro ao processar sua solicitação.",
            ["Error.RequestId"] = "ID da requisição",
            ["Common.GeneratedAt"] = "Gerado em",
            ["Common.Status"] = "Status",
            ["Common.Sector"] = "Setor",
            ["Common.Type"] = "Tipo",
            ["Common.Responsible"] = "Responsável",
            ["Common.NoResponsible"] = "Sem responsável",
            ["Common.PurchaseValue"] = "Valor de compra",
            ["Common.CurrentValue"] = "Valor atual",
            ["Common.Patrimony"] = "Patrimônio",
            ["Common.Name"] = "Nome",
            ["Common.BrandModel"] = "Marca/Modelo",
            ["Common.Total"] = "Total",
            ["Common.Invested"] = "Investido",
            ["Common.Current"] = "Atual",
            ["Common.Assets"] = "Ativos"
        };

        private static readonly Dictionary<string, string> EnUs = new()
        {
            ["Nav.Home"] = "Home",
            ["Nav.Inventory"] = "Inventory",
            ["Nav.Audit"] = "Audit",
            ["Nav.Reports"] = "Reports",
            ["Nav.Users"] = "Users",
            ["Nav.Privacy"] = "Privacy",
            ["Nav.Language"] = "Language",
            ["Nav.Theme"] = "Theme",
            ["Nav.Light"] = "Light",
            ["Nav.Dark"] = "Dark",
            ["Dashboard.Title"] = "Control dashboard",
            ["Dashboard.Subtitle"] = "Operational summary of the IT asset inventory",
            ["Dashboard.InventoryHealth"] = "Inventory health",
            ["Dashboard.TotalAssets"] = "Total assets",
            ["Dashboard.Available"] = "Available",
            ["Dashboard.InUse"] = "In use",
            ["Dashboard.Maintenance"] = "Maintenance",
            ["Dashboard.Registered"] = "Registered",
            ["Dashboard.InStock"] = "In stock",
            ["Dashboard.WithUsers"] = "Assigned to users",
            ["Dashboard.RequiresAttention"] = "Requires attention",
            ["Dashboard.TotalInvested"] = "Total invested",
            ["Dashboard.CurrentValue"] = "Estimated current value",
            ["Dashboard.Depreciation"] = "Estimated depreciation",
            ["Dashboard.BySector"] = "Assets by sector",
            ["Dashboard.ByStatus"] = "Status distribution",
            ["Dashboard.ByType"] = "Assets by type",
            ["Dashboard.ValueSummary"] = "Asset value summary",
            ["Dashboard.OperationalAlerts"] = "Operational alerts",
            ["Dashboard.NoAlerts"] = "No critical alerts at the moment.",
            ["Dashboard.ExportReports"] = "Export reports",
            ["Dashboard.ManageInventory"] = "Manage inventory",
            ["Reports.Title"] = "Reports center",
            ["Reports.Subtitle"] = "Export data for analysis, audit and presentation",
            ["Reports.ExcelTitle"] = "General Excel report",
            ["Reports.ExcelDescription"] = "Exports the full asset list with status, sector, responsible user and values.",
            ["Reports.ExcelButton"] = "Download Excel",
            ["Reports.PdfTitle"] = "Executive PDF report",
            ["Reports.PdfDescription"] = "Generates a print-ready document with financial summary and detailed inventory.",
            ["Reports.PdfButton"] = "Download PDF",
            ["Privacy.Title"] = "Privacy policy",
            ["Error.Title"] = "Error",
            ["Error.Message"] = "An error occurred while processing your request.",
            ["Error.RequestId"] = "Request ID",
            ["Common.GeneratedAt"] = "Generated at",
            ["Common.Status"] = "Status",
            ["Common.Sector"] = "Sector",
            ["Common.Type"] = "Type",
            ["Common.Responsible"] = "Responsible",
            ["Common.NoResponsible"] = "No responsible user",
            ["Common.PurchaseValue"] = "Purchase value",
            ["Common.CurrentValue"] = "Current value",
            ["Common.Patrimony"] = "Asset tag",
            ["Common.Name"] = "Name",
            ["Common.BrandModel"] = "Brand/Model",
            ["Common.Total"] = "Total",
            ["Common.Invested"] = "Invested",
            ["Common.Current"] = "Current",
            ["Common.Assets"] = "Assets"
        };

        public string CultureName => CultureInfo.CurrentUICulture.Name;

        public bool IsEnglish => CultureName.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        public string this[string key]
        {
            get
            {
                var source = IsEnglish ? EnUs : PtBr;
                return source.TryGetValue(key, out var value) ? value : key;
            }
        }
    }
}
