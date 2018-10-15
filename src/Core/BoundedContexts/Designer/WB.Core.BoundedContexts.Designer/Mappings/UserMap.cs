using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Mappings
{
    [PlainStorage]
    public class UserMap : ClassMapping<User>
    {
        public UserMap()
        {
            Id(x => x.UserId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            DynamicUpdate(true);

            Property(x => x.ProviderUserKey);

            Property(x => x.ApplicationName);

            Property(x => x.Comment);

            Property(x => x.ConfirmationToken);

            Property(x => x.CreatedAt);

            Property(x => x.Email);

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

            Property(x => x.CanImportOnHq);

            Property(x => x.FullName);

            Set(x => x.SimpleRoles, m =>
            {
                m.Key(km => km.Column("UserId"));
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
