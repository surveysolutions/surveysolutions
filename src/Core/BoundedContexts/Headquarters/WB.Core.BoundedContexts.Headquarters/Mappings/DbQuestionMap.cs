using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class DbQuestionMap : ClassMapping<DbQuestion>
    {
        public DbQuestionMap()
        {
            this.Table("interviewquestions");

            this.ComposedId(x =>
            {
                x.Property(m=>m.InterviewId);
                x.Property(m=>m.QuestionIdentity, cmp =>
                {
                    cmp.Column("QuestionId");
                    cmp.Column(y =>
                    {
                        y.Name("RosterVector");
                        y.SqlType("numeric[]");
                    });
                });
            });

            this.Property(x => x.InterviewId, pm => pm.Column(cm => cm.Index("interviewquestion_interviewId")));
            this.Component(x => x.QuestionIdentity, cmp =>
            {
                cmp.Property(x => x.Id, ptp=>ptp.Column("QuestionId"));
                
                cmp.Property(x => x.RosterVector, ptp =>
                {
                    ptp.Type<PostgresRosterVector>();
                    ptp.Column(clm => clm.SqlType("numeric[]"));
                });
            });
            
            this.Property(x => x.IsFlagged);
        }
    }
}