using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Mapping
{
    public class UsersImportProcessMap : ClassMapping<UsersImportProcess>
    {
        public UsersImportProcessMap()
        {
            this.Table("usersimportprocess");
            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.FileName);
            this.Property(x => x.SupervisorsCount);
            this.Property(x => x.InterviewersCount);
            this.Property(x => x.Responsible);
            this.Property(x => x.StartedDate);
        }
    }
}
