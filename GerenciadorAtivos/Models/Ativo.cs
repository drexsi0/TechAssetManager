using System.ComponentModel.DataAnnotations;

namespace GerenciadorAtivos.Models
{
    public class Ativo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome do Equipamento")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O patrimônio é obrigatório.")]
        public string Patrimonio { get; set; }

        [Display(Name = "Tipo de Ativo")]
        public TipoAtivo Tipo { get; set; }

        public string Marca { get; set; }
        public string Modelo { get; set; }

        [Display(Name = "Localização")]
        public string Setor { get; set; }

        public StatusAtivo Status { get; set; } = StatusAtivo.Disponivel;
    }

    public enum TipoAtivo { Notebook, Desktop, Monitor, Periferico, Servidor }
    public enum StatusAtivo { Disponivel, EmUso, Manutencao, Descartado }
}