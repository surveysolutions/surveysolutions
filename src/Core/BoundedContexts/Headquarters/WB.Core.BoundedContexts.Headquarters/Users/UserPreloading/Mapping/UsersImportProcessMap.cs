using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.Services;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Mapping
{
    [Users]
    public class UsersImportProcessMap : ClassMapping<UsersImportProcess>
    {
        public UsersImportProcessMap()
        {
            this.Table("usersimportprocess");
            this.Schema("users");
            
            this.Id(x => x.Id, Idmap =>
            {
                Idmap.Column("id");
                Idmap.Generator(Generators.Identity);
            });
            this.Property(x => x.FileName, c=> c.Column("filename"));
            this.Property(x => x.SupervisorsCount, c=> c.Column("supervisorscount"));
            this.Property(x => x.InterviewersCount, c=> c.Column("interviewerscount"));
            this.Property(x => x.Responsible, c=> c.Column("responsible"));
            this.Property(x => x.StartedDate, c=> c.Column("starteddate"));
            this.Property(x => x.Workspace, c=> c.Column("workspace"));
        }
    }
}
