using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GerenciadorAtivos.Models
{
    public class Historico
    {
        [Key]
        public int Id { get; set; }

        // Relação com o Ativo (Foreign Key)
        [Required]
        public int AtivoId { get; set; }

        // Propriedade de Navegação (para o Entity Framework entender o link)
        [ForeignKey("AtivoId")]
        public virtual Ativo? Ativo { get; set; }

        [Required]
        [Display(Name = "Data da Ação")]
        public DateTime DataAcao { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Tipo de Ação")]
        public string TipoAcao { get; set; } = string.Empty; // Ex: "Criação", "Atualização", "Exclusão"

        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty; // Ex: "Mudou status de Disponível para Em Uso"

        public string? Usuario { get; set; }
    }
}