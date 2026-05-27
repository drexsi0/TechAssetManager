namespace GerenciadorAtivos.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Apenas contadores e dicionários para gráficos
        public int TotalAtivos { get; set; }
        public int EmUso { get; set; }
        public int Disponiveis { get; set; }
        public int EmManutencao { get; set; }

        public Dictionary<string, int> AtivosPorStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AtivosPorSetor { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AtivosPorTipo { get; set; } = new Dictionary<string, int>();

        // --- NOVOS CAMPOS FINANCEIROS ---
        public decimal ValorTotalInvestido { get; set; } // Quanto pagou
        public decimal ValorTotalAtual { get; set; }     // Quanto vale hoje
    }
}
