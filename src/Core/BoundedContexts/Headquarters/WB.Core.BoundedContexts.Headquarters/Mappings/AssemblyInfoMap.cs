using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class AssemblyInfoMap : ClassMapping<AssemblyInfo>
    {
        public AssemblyInfoMap()
        {
            this.Id(x => x.AssemblyId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.CreationDate);
            this.Property(x => x.Content);
        }
    }
}