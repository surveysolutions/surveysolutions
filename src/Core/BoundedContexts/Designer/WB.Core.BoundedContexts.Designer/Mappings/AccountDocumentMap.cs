using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Views.Account;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    public class AccountDocumentMap : ClassMapping<AccountDocument>
    {
        public AccountDocumentMap()
        {
            Id(x => x.UserId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.ProviderUserKey);

            Property(x => x.ApplicationName);

            Property(x => x.Comment);

            Property(x => x.ConfirmationToken);

            Property(x => x.CreatedAt);

            Property(x => x.Email);

            Property(x => x.FailedPasswordAnswerWindowAttemptCount);

            Property(x => x.FailedPasswordAnswerWindowStartedAt);

            Property(x => x.FailedPasswordWindowAttemptCount);

            Property(x => x.FailedPasswordWindowStartedAt);

            Property(x => x.IsConfirmed);

            Property(x => x.IsLockedOut);

            Property(x => x.IsOnline);

            Property(x => x.LastActivityAt);

            Property(x => x.LastLockedOutAt);

            Property(x => x.LastLoginAt);

            Property(x => x.LastPasswordChangeAt);

            Property(x => x.Password);

            Property(x => x.PasswordAnswer);

            Property(x => x.PasswordQuestion);

            Property(x => x.PasswordResetExpirationDate);

            Property(x => x.PasswordResetToken);

            Property(x => x.PasswordSalt);

            Property(x => x.UserName);

            Set(x => x.SimpleRoles, m =>
            {
                m.Key(km => km.Column("AccountId"));
                m.Table("SimpleRoles");
                m.Lazy(CollectionLazy.NoLazy);
            },
                r => r.Element(e =>
                {
                    e.Column("SimpleRoleId");
                }));
        }
    }
}
