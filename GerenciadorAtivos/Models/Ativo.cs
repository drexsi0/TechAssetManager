using System.ComponentModel.DataAnnotations;

namespace GerenciadorAtivos.Models
{
    public class Ativo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Por favor, digite o nome do equipamento.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")] // {1} pega o valor 100 automaticamente
        [Display(Name = "Nome do Equipamento")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O número de patrimônio é obrigatório.")]
        [StringLength(20, ErrorMessage = "O patrimônio deve ter no máximo {1} caracteres.")]
        public string Patrimonio { get; set; }

        [Required(ErrorMessage = "Selecione o tipo do ativo.")]
        [Display(Name = "Tipo de Ativo")]
        public TipoAtivo Tipo { get; set; }

        [Required(ErrorMessage = "Por favor, digite o nome da marca.")]
        [StringLength(50, ErrorMessage = "A marca não pode exceder {1} caracteres.")]
        public string Marca { get; set; }

        [Required(ErrorMessage = "Por favor, digite o nome do modelo.")]
        [StringLength(50, ErrorMessage = "O modelo não pode exceder {1} caracteres.")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "Por favor, digite o nome do Setor.")]
        [Display(Name = "Localização / Setor")]
        public string Setor { get; set; }

        [Required(ErrorMessage = "Defina o status inicial.")]
        public StatusAtivo? Status { get; set; }
    }

    public enum TipoAtivo { Notebook, Desktop, Monitor, Periferico, Servidor }
    public enum StatusAtivo
    {
        Disponivel,

        [Display(Name = "Em Uso")] // Isso faz a mágica visual
        EmUso,

        [Display(Name = "Em Manutenção")] // Aproveitando para arrumar esse também
        Manutencao,

        Descartado
    }
}