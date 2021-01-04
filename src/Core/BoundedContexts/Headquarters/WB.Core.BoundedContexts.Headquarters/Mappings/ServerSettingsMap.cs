using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class ServerSettingsMap : ClassMapping<ServerSettings>
    {
        public ServerSettingsMap()
        {
            Id(x => x.Id, (id) => id.Generator(Generators.Assigned));
            Table("server_settings");
            Schema(WorkspaceConstants.SchemaName);
            Property(x => x.Value);
        }
    }
}
