using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class InterviewDbEntityMap : ClassMapping<InterviewDbEntity>
    {
        public InterviewDbEntityMap()
        {
            this.Table("interviewentities");

            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.Identity));
            this.Property(x => x.InterviewId, pm => pm.Column(cm => cm.Index("interviewentities_interviewId")));

            this.Component(x => x.QuestionIdentity, cmp =>
            {
                cmp.Property(y => y.Id, ptp => ptp.Column("QuestionId"));
                cmp.Property(x => x.RosterVector, ptp =>
                {
                    ptp.Type<PostgresRosterVector>();
                    ptp.Column(clm => clm.SqlType("int[]"));
                });
            });

            this.Property(x => x.HasFlag);
        }
    }
}