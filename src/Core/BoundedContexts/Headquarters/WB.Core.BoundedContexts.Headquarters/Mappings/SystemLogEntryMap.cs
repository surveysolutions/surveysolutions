using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class SystemLogEntryMap : ClassMapping<SystemLogEntry>
    {
        public SystemLogEntryMap()
        {
            this.Table("systemlog");

            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Increment);
                idMap.Column("Id");
            });
            
            this.Property(x => x.UserId);
            this.Property(x => x.UserName);
            this.Property(x => x.LogDate);
            this.Property(x => x.Type);
            this.Property("Log", m =>
            {
                m.Column("log");
            });
        }
    }
}
