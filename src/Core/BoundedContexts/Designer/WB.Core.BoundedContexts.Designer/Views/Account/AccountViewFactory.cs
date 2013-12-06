using System;
using System.Linq;
using Main.Core.View;
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
            Func<AccountDocument, bool> query = (x) => false;
            if (input.ProviderUserKey != null)
            {
                query = (x) => x.ProviderUserKey.ToString()== input.ProviderUserKey.ToString();
            }
            else if (!string.IsNullOrEmpty(input.AccountName))
            {
                query = (x) => x.UserName == NormalizeAccountName(input.AccountName);
            }
            else if (!string.IsNullOrEmpty(input.AccountEmail))
            {
                query = (x) => x.Email == input.AccountEmail;
            }
            else if (!string.IsNullOrEmpty(input.ConfirmationToken))
            {
                query = (x) => x.ConfirmationToken == input.ConfirmationToken;
            }
            else if (!string.IsNullOrEmpty(input.ResetPasswordToken))
            {
                query = (x) => x.PasswordResetToken == input.ResetPasswordToken;
            }

            return
                this.accounts.Query(_ => _
                    .Where(query)
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
                    .FirstOrDefault());
        }

        private string NormalizeAccountName(string accountName)
        {
            return accountName.ToLower();
        }
    }
}