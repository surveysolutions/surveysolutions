using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings
{
    public class WorkspaceMap : ClassMapping<Workspace>
    {
        public WorkspaceMap()
        {
            Id(x => x.Name, mapper =>
            {
                mapper.Generator(Generators.Assigned);
                mapper.Column("name");
            });

            Property(x => x.DisplayName, ptp => ptp.Column("display_name"));
            Schema(WorkspaceConstants.SchemaName);
            Set(x => x.Users, set =>
            {
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                set.Key(k => k.Column("workspace"));
            }, s => s.OneToMany());   
        }
    }
}
