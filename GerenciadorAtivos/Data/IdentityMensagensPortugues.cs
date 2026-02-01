using Microsoft.AspNetCore.Identity;

namespace GerenciadorAtivos.Data
{
    public class IdentityMensagensPortugues : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
            => new IdentityError { Code = nameof(DefaultError), Description = $"Um erro desconhecido ocorreu." };

        public override IdentityError ConcurrencyFailure()
            => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Falha de concorrência otimista, o objeto foi modificado." };

        public override IdentityError PasswordMismatch()
            => new IdentityError { Code = nameof(PasswordMismatch), Description = "Senha incorreta." };

        public override IdentityError InvalidToken()
            => new IdentityError { Code = nameof(InvalidToken), Description = "Token inválido." };

        public override IdentityError LoginAlreadyAssociated()
            => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Já existe um usuário com este login." };

        public override IdentityError InvalidUserName(string? userName)
            => new IdentityError { Code = nameof(InvalidUserName), Description = $"O nome de usuário '{userName}' é inválido, pode conter apenas letras ou dígitos." };

        public override IdentityError InvalidEmail(string? email)
            => new IdentityError { Code = nameof(InvalidEmail), Description = $"O e-mail '{email}' é inválido." };

        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError { Code = nameof(DuplicateUserName), Description = $"O nome de usuário '{userName}' já está sendo utilizado." };

        public override IdentityError DuplicateEmail(string email)
            => new IdentityError { Code = nameof(DuplicateEmail), Description = $"O e-mail '{email}' já está sendo utilizado." };

        public override IdentityError InvalidRoleName(string? role)
            => new IdentityError { Code = nameof(InvalidRoleName), Description = $"O nome da permissão '{role}' é inválido." };

        public override IdentityError DuplicateRoleName(string role)
            => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"A permissão '{role}' já está sendo utilizada." };

        public override IdentityError UserAlreadyHasPassword()
            => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "O usuário já possui uma senha definida." };

        public override IdentityError UserLockoutNotEnabled()
            => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "O bloqueio não está habilitado para este usuário." };

        public override IdentityError UserAlreadyInRole(string role)
            => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"O usuário já possui a permissão '{role}'." };

        public override IdentityError UserNotInRole(string role)
            => new IdentityError { Code = nameof(UserNotInRole), Description = $"O usuário não tem a permissão '{role}'." };

        public override IdentityError PasswordTooShort(int length)
            => new IdentityError { Code = nameof(PasswordTooShort), Description = $"A senha deve ter no mínimo {length} caracteres." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "A senha deve conter pelo menos um caractere especial (! @ # $ %)." };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "A senha deve conter pelo menos um número (0-9)." };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "A senha deve conter pelo menos uma letra minúscula." };

        public override IdentityError PasswordRequiresUpper()
            => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "A senha deve conter pelo menos uma letra maiúscula." };
    }
}