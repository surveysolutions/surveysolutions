using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public class AccountViewFactory : IViewFactory<AccountViewInputModel, AccountView>
    {
        private readonly IQueryableReadSideRepositoryReader<AccountDocument> accounts;

        public AccountViewFactory(IQueryableReadSideRepositoryReader<AccountDocument> accounts)
        {
            this.accounts = accounts;
        }

        public AccountView Load(AccountViewInputModel input)
        {
            AccountDocument user = null;
            if (input.ProviderUserKey != null)
            {
                user = accounts.Query(_ => _.FirstOrDefault((x) => x.ProviderUserKey == input.ProviderUserKey));
            }
            else if (!string.IsNullOrEmpty(input.AccountName))
            {
                var normalizedAccountName = NormalizeStringQueryParameter(input.AccountName);
                user = accounts.Query(_ => _.FirstOrDefault((x) => x.UserName == normalizedAccountName));
            }
            else if (!string.IsNullOrEmpty(input.AccountEmail))
            {
                var normalizedAccountEmail = NormalizeStringQueryParameter(input.AccountEmail);
                user = accounts.Query(_ => _.FirstOrDefault((x) => x.Email.ToLower() == normalizedAccountEmail));
            }
            else if (!string.IsNullOrEmpty(input.ConfirmationToken))
            {
                user = accounts.Query(_ => _.FirstOrDefault((x) => x.ConfirmationToken == input.ConfirmationToken));
            }
            else if (!string.IsNullOrEmpty(input.ResetPasswordToken))
            {
                user = accounts.Query(_ => _.FirstOrDefault((x) => x.PasswordResetToken == input.ResetPasswordToken));
            }

            if (user == null)
                return null;
            
            return new AccountView
            {
                ApplicationName = user.ApplicationName,
                ProviderUserKey = user.ProviderUserKey,
                UserName = user.UserName,
                Comment = user.Comment,
                ConfirmationToken = user.ConfirmationToken,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                IsConfirmed = user.IsConfirmed,
                IsLockedOut = user.IsLockedOut,
                IsOnline = user.IsOnline,
                LastActivityAt = user.LastActivityAt,
                LastLockedOutAt = user.LastLockedOutAt,
                LastLoginAt = user.LastLoginAt,
                LastPasswordChangeAt = user.LastPasswordChangeAt,
                Password = user.Password,
                PasswordSalt = user.PasswordSalt,
                PasswordQuestion = user.PasswordQuestion,
                PasswordAnswer = user.PasswordAnswer,
                PasswordResetToken = user.PasswordResetToken,
                PasswordResetExpirationDate = user.PasswordResetExpirationDate,
                SimpleRoles = user.SimpleRoles
            };
        }

        private string NormalizeStringQueryParameter(string accountName)
        {
            return accountName.ToLower();
        }
    }
}