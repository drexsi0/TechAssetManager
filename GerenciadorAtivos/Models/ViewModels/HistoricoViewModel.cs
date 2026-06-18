using GerenciadorAtivos.Models;

namespace GerenciadorAtivos.Models.ViewModels
{
    public class HistoricoViewModel
    {
        public int Id { get; set; }
        public int AtivoId { get; set; }
        public Ativo? Ativo { get; set; }
        public DateTime DataAcao { get; set; }
        public string TipoAcao { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Usuario { get; set; }
        public string UsuarioDisplay { get; set; } = "Sistema";
    }
}
