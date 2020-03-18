using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewCommentMap : ClassMapping<InterviewComment>
    {
        public InterviewCommentMap()
        {
            this.Table("commentaries");
            this.Id(x => x.Id, idMapper =>
            {
                idMapper.Generator(Generators.Identity);
            });
            this.Property(x => x.Comment);
            this.Property(x => x.CommentSequence);
            this.Property(x => x.OriginatorUserId); 
            this.Property(x => x.OriginatorName);
            this.Property(x => x.OriginatorRole);
            this.Property(x => x.RosterVector, ptp =>
            {
                ptp.Type<PostgresSqlArrayType<decimal>>();
                ptp.Column(clm => clm.SqlType("numeric[]"));
            });
            this.Property(x => x.Roster);
            this.Property(x => x.Timestamp);
            this.Property(x => x.Variable);
            this.ManyToOne(x => x.InterviewCommentaries, mto => mto.Column("summary_id"));
        }
    }
}
