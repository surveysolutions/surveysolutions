using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Mappings
{
    public class WorkspaceUsersMap : ClassMapping<WorkspacesUsers>
    {
        public WorkspaceUsersMap()
        {
            Schema(WorkspaceConstants.SchemaName);
            Table("workspace_users");
            Id(x => x.Id, idMap => idMap.Generator(Generators.Identity));
            ManyToOne(x => x.Supervisor, pm => pm.Column("supervisor_id"));

            ManyToOne(x => x.Workspace);
            ManyToOne(x => x.User, ptp => ptp.Column("user_id"));
        }
    }
}
