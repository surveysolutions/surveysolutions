using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.Storage.Postgre.NhExtensions;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewCommentariesMap : ClassMapping<InterviewCommentaries>
    {
        public InterviewCommentariesMap()
        {
            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.IsDeleted);
            Property(x => x.IsApprovedByHQ);
            Property(x=>x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            List(x => x.Commentaries, listMap =>
            {
                listMap.Table("Commentaries");

                listMap.Index(index => index.Column("Position"));
                listMap.Key(keyMap =>
                {
                    keyMap.Column(clm =>
                    {
                        clm.Name("InterviewId");
                        clm.Index("InterviewCommentaries_Comment");
                    });
                });
                listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                rel =>
                {
                    rel.Component(cmp =>
                    {
                        cmp.Property(x => x.Comment);
                        cmp.Property(x => x.CommentSequence);
                        cmp.Property(x => x.OriginatorUserId);
                        cmp.Property(x => x.OriginatorName);
                        cmp.Property(x => x.OriginatorRole);
                        cmp.Property(x => x.RosterVector, ptp =>
                        {
                            ptp.Type<PostgresSqlArrayType<decimal>>();
                            ptp.Column(clm => clm.SqlType("numeric[]"));
                        });
                        cmp.Property(x => x.Roster);
                        cmp.Property(x => x.Timestamp);
                        cmp.Property(x => x.Variable);
                    });
                });
        }
    }
}