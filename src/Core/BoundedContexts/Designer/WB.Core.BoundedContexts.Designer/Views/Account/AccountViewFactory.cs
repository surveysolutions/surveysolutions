using System;
using System.Linq;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public class AccountViewFactory : IViewFactory<AccountViewInputModel, AccountView>
    {
        private readonly IQueryableReadSideRepositoryWriter<AccountDocument> accounts;

        public AccountViewFactory(IQueryableReadSideRepositoryWriter<AccountDocument> accounts)
        {
            this.accounts = accounts;
        }

        public AccountView Load(AccountViewInputModel input)
        {
            IQueryable<AccountDocument> users = Enumerable.Empty<AccountDocument>().AsQueryable();
            if (input.ProviderUserKey != null)
            {
                users = accounts.Query(_ => _.Where((x) => x.ProviderUserKey== input.ProviderUserKey));
            }
            else if (!string.IsNullOrEmpty(input.AccountName))
            {
                var normalizedAccountName = NormalizeAccountName(input.AccountName);
                users = accounts.Query(_ => _.Where((x) => x.UserName == normalizedAccountName));
            }
            else if (!string.IsNullOrEmpty(input.AccountEmail))
            {
                users = accounts.Query(_ => _.Where((x) => x.Email == input.AccountEmail));
            }
            else if (!string.IsNullOrEmpty(input.ConfirmationToken))
            {
                users = accounts.Query(_ => _.Where((x) => x.ConfirmationToken == input.ConfirmationToken));
            }
            else if (!string.IsNullOrEmpty(input.ResetPasswordToken))
            {
                users = accounts.Query(_ => _.Where((x) => x.PasswordResetToken == input.ResetPasswordToken));
            }

            return
                users
                    .Select(
                        x =>
                            new AccountView
                            {
                                ApplicationName = x.ApplicationName,
                                ProviderUserKey = x.ProviderUserKey,
                                UserName = x.UserName,
                                Comment = x.Comment,
                                ConfirmationToken = x.ConfirmationToken,
                                CreatedAt = x.CreatedAt,
                                Email = x.Email,
                                FailedPasswordAnswerWindowAttemptCount =
                                    x.FailedPasswordAnswerWindowAttemptCount,
                                FailedPasswordAnswerWindowStartedAt = x.FailedPasswordAnswerWindowStartedAt,
                                FailedPasswordWindowAttemptCount = x.FailedPasswordWindowAttemptCount,
                                FailedPasswordWindowStartedAt = x.FailedPasswordWindowStartedAt,
                                IsConfirmed = x.IsConfirmed,
                                IsLockedOut = x.IsLockedOut,
                                IsOnline = x.IsOnline,
                                LastActivityAt = x.LastActivityAt,
                                LastLockedOutAt = x.LastLockedOutAt,
                                LastLoginAt = x.LastLoginAt,
                                LastPasswordChangeAt = x.LastPasswordChangeAt,
                                Password = x.Password,
                                PasswordSalt = x.PasswordSalt,
                                PasswordQuestion = x.PasswordQuestion,
                                PasswordAnswer = x.PasswordAnswer,
                                PasswordResetToken = x.PasswordResetToken,
                                PasswordResetExpirationDate = x.PasswordResetExpirationDate,
                                SimpleRoles = x.SimpleRoles
                            })
                    .FirstOrDefault();
        }

        private string NormalizeAccountName(string accountName)
        {
            return accountName.ToLower();
        }
    }
}