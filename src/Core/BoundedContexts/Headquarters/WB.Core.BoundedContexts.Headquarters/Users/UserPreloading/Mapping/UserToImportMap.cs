using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Mapping
{
    public class UserToImportMap : ClassMapping<UserToImport>
    {
        public UserToImportMap()
        {
            this.Table("usertoimport");
            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.Login);
            this.Property(x => x.Email);
            this.Property(x => x.FullName);
            this.Property(x => x.Password);
            this.Property(x => x.PhoneNumber);
            this.Property(x => x.Role);
            this.Property(x => x.Supervisor);
        }
    }
}
