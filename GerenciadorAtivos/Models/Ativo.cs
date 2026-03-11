using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GerenciadorAtivos.Models
{
    public class Ativo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Por favor, digite o nome do equipamento.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo {1} caracteres.")] // {1} pega o valor 100 automaticamente
        [Display(Name = "Nome do Equipamento")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número de patrimônio é obrigatório.")]
        [StringLength(20, ErrorMessage = "O patrimônio deve ter no máximo {1} caracteres.")]
        public string Patrimonio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione o tipo do ativo.")]
        [Display(Name = "Tipo de Ativo")]
        public TipoAtivo Tipo { get; set; }

        [Required(ErrorMessage = "Por favor, digite o nome da marca.")]
        [StringLength(50, ErrorMessage = "A marca não pode exceder {1} caracteres.")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "Por favor, digite o nome do modelo.")]
        [StringLength(50, ErrorMessage = "O modelo não pode exceder {1} caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Por favor, digite o nome do Setor.")]
        [Display(Name = "Localização / Setor")]
        public string Setor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Defina o status inicial.")]
        public StatusAtivo? Status { get; set; }

        // Cria a lista vazia para evitar erro de "NullReference" ao tentar adicionar algo
        public virtual ICollection<Historico> Historicos { get; set; } = new List<Historico>();

        // --- NOVOS CAMPOS FINANCEIROS ---
        [Display(Name = "Valor de Compra")]
        [Required(ErrorMessage = "O valor é obrigatório")]
        [Column(TypeName = "decimal(18,2)")] // Define precisão monetária no banco
        public decimal ValorCompra { get; set; }

        [Display(Name = "Data de Compra")]
        [Required(ErrorMessage = "A data é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime DataCompra { get; set; }

        // --- LÓGICA DE DEPRECIAÇÃO (BI) ---
        // Propriedade calculada: Não vai pro banco ([NotMapped]), o C# calcula na hora.
        // Regra: Deprecia 20% ao ano (vida útil de 5 anos).
        [NotMapped]
        public decimal ValorAtual
        {
            get
            {
                var anosDeUso = (DateTime.Now - DataCompra).TotalDays / 365.0;
                var taxaDepreciacaoAnual = 0.20m; // 20%
                var valorDepreciado = ValorCompra * taxaDepreciacaoAnual * (decimal)anosDeUso;

                var valorFinal = ValorCompra - valorDepreciado;

                // O valor nunca pode ser menor que zero (sucata)
                return valorFinal < 0 ? 0 : valorFinal;
            }
        }
        // Flag para o Soft Delete
        public bool IsDeleted { get; set; } = false;
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