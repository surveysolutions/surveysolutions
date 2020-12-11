using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Mapping
{
    public class AssignmentImportProcessMap : ClassMapping<AssignmentsImportProcess>
    {
        public AssignmentImportProcessMap()
        {
            this.Table("assignmentsimportprocess");
            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.FileName);
            this.Property(x => x.TotalCount);
            this.Property(x => x.Responsible);
            this.Property(x => x.StartedDate);
            this.Property(x => x.QuestionnaireId);
            this.Property(x => x.Status);
            this.Property(x => x.AssignedTo);
        }
    }
}
