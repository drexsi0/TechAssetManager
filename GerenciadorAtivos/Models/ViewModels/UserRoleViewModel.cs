using System.ComponentModel.DataAnnotations;

namespace GerenciadorAtivos.Models.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayEmail { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = "User";

        [Required]
        public string SelectedRole { get; set; } = "User";
    }
}
