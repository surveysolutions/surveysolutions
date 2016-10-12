using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public interface IAccountViewFactory
    {
        IAccountView Load(AccountViewInputModel input);
    }

    public class AccountViewFactory : IAccountViewFactory
    {
        private readonly IPlainStorageAccessor<Aggregates.Account> accounts;

        public AccountViewFactory(IPlainStorageAccessor<Aggregates.Account> accounts)
        {
            this.accounts = accounts;
        }

        public IAccountView Load(AccountViewInputModel input)
        {
            Aggregates.Account user = null;
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

            return user;
        }

        private static string NormalizeStringQueryParameter(string accountName) => accountName.ToLower();
    }
}