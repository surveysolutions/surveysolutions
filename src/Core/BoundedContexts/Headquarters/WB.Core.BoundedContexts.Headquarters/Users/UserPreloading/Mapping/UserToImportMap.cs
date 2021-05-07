using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Mapping
{
    [Users]
    public class UserToImportMap : ClassMapping<UserToImport>
    {
        public UserToImportMap()
        {
            this.Table("usertoimport");
            this.Schema("users");
            
            this.Id(x => x.Id, Idmap =>
            {
                Idmap.Column("id");
                Idmap.Generator(Generators.Identity);
            });
            this.Property(x => x.Login, c=> c.Column("login"));
            this.Property(x => x.Email, c=> c.Column("email"));
            this.Property(x => x.FullName, c=> c.Column("fullname"));
            this.Property(x => x.Password, c=> c.Column("password"));
            this.Property(x => x.PhoneNumber, c=> c.Column("phonenumber"));
            this.Property(x => x.Role, c=> c.Column("role"));
            this.Property(x => x.Supervisor, c=> c.Column("supervisor"));
            this.Property(x => x.Workspace, c=> c.Column("workspace"));
        }
    }
}
