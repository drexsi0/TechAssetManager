using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace GerenciadorAtivos.Data
{
    public class IdentityMensagensPortugues : IdentityErrorDescriber
    {
        private static bool En => CultureInfo.CurrentUICulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        public override IdentityError DefaultError()
            => En ? base.DefaultError() : new IdentityError { Code = nameof(DefaultError), Description = "Um erro desconhecido ocorreu." };

        public override IdentityError ConcurrencyFailure()
            => En ? base.ConcurrencyFailure() : new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Falha de concorrência otimista. O registro foi modificado por outra operação." };

        public override IdentityError PasswordMismatch()
            => En ? base.PasswordMismatch() : new IdentityError { Code = nameof(PasswordMismatch), Description = "Senha incorreta." };

        public override IdentityError InvalidToken()
            => En ? base.InvalidToken() : new IdentityError { Code = nameof(InvalidToken), Description = "Token inválido." };

        public override IdentityError LoginAlreadyAssociated()
            => En ? base.LoginAlreadyAssociated() : new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Já existe um usuário com este login." };

        public override IdentityError InvalidUserName(string? userName)
            => En ? base.InvalidUserName(userName) : new IdentityError { Code = nameof(InvalidUserName), Description = $"O nome de usuário '{userName}' é inválido." };

        public override IdentityError InvalidEmail(string? email)
            => En ? base.InvalidEmail(email) : new IdentityError { Code = nameof(InvalidEmail), Description = $"O e-mail '{email}' é inválido." };

        public override IdentityError DuplicateUserName(string userName)
            => En ? base.DuplicateUserName(userName) : new IdentityError { Code = nameof(DuplicateUserName), Description = $"O usuário '{userName}' já está em uso." };

        public override IdentityError DuplicateEmail(string email)
            => En ? base.DuplicateEmail(email) : new IdentityError { Code = nameof(DuplicateEmail), Description = $"O e-mail '{email}' já está em uso." };

        public override IdentityError InvalidRoleName(string? role)
            => En ? base.InvalidRoleName(role) : new IdentityError { Code = nameof(InvalidRoleName), Description = $"O perfil '{role}' é inválido." };

        public override IdentityError DuplicateRoleName(string role)
            => En ? base.DuplicateRoleName(role) : new IdentityError { Code = nameof(DuplicateRoleName), Description = $"O perfil '{role}' já está em uso." };

        public override IdentityError UserAlreadyHasPassword()
            => En ? base.UserAlreadyHasPassword() : new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "O usuário já possui uma senha definida." };

        public override IdentityError UserLockoutNotEnabled()
            => En ? base.UserLockoutNotEnabled() : new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "O bloqueio não está habilitado para este usuário." };

        public override IdentityError UserAlreadyInRole(string role)
            => En ? base.UserAlreadyInRole(role) : new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"O usuário já possui o perfil '{role}'." };

        public override IdentityError UserNotInRole(string role)
            => En ? base.UserNotInRole(role) : new IdentityError { Code = nameof(UserNotInRole), Description = $"O usuário não possui o perfil '{role}'." };

        public override IdentityError PasswordTooShort(int length)
            => En ? base.PasswordTooShort(length) : new IdentityError { Code = nameof(PasswordTooShort), Description = $"A senha deve ter no mínimo {length} caracteres." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => En ? base.PasswordRequiresNonAlphanumeric() : new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "A senha deve conter pelo menos um caractere especial." };

        public override IdentityError PasswordRequiresDigit()
            => En ? base.PasswordRequiresDigit() : new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "A senha deve conter pelo menos um número." };

        public override IdentityError PasswordRequiresLower()
            => En ? base.PasswordRequiresLower() : new IdentityError { Code = nameof(PasswordRequiresLower), Description = "A senha deve conter pelo menos uma letra minúscula." };

        public override IdentityError PasswordRequiresUpper()
            => En ? base.PasswordRequiresUpper() : new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "A senha deve conter pelo menos uma letra maiúscula." };
    }
}
