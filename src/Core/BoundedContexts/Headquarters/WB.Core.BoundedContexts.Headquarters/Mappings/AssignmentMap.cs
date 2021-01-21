using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class AssignmentMap : ClassMapping<Assignment>
    {
        public AssignmentMap()
        {
            DynamicUpdate(true);

            Id(x => x.PublicKey, mapper => mapper.Column("PublicKey"));
            NaturalId(mapping => mapping.Property(x => x.Id, mapper => mapper.Unique(true)));

            Property(x => x.ResponsibleId);
            Property(x => x.Quantity);
            Property(x => x.Archived);
            Property(x => x.CreatedAtUtc,pm => pm.Type<UtcDateTimeType>());
            Property(x => x.UpdatedAtUtc, pm => pm.Type<UtcDateTimeType>());
            Property(x => x.ReceivedByTabletAtUtc, pm => pm.Type<UtcDateTimeType>());
            Property(x => x.AudioRecording);
            Property(x => x.Password);
            Property(x => x.Email);
            Property(x => x.WebMode);
            Property(x => x.Comments);

            Component(x => x.QuestionnaireId, cmp =>
            {
                cmp.Lazy(false);
                cmp.Property(x => x.QuestionnaireId);
                cmp.Property(x => x.Id, ptp => ptp.Column("Questionnaire"));
                cmp.Property(x => x.Version, ptp => ptp.Column("QuestionnaireVersion"));
            });

            this.Property(x => x.ProtectedVariables, m => m.Type<PostgresJson<List<string>>>());

            ManyToOne(x => x.Questionnaire, mto =>
            {
                mto.Column("Questionnaire");
                mto.Cascade(Cascade.None);
                mto.Update(false);
                mto.Insert(false);
            });

            Set(x => x.InterviewSummaries, set =>
            {
                set.Key(key =>
                {
                    key.PropertyRef(a => a.Id);
                    key.Column(nameof(InterviewSummary.AssignmentId));
                });
                set.Lazy(CollectionLazy.Extra);
                set.Cascade(Cascade.None);
            }, relation => relation.OneToMany());

            List(x => x.IdentifyingData, mapper =>
            {
                mapper.Table("AssignmentsIdentifyingAnswers");
                mapper.Key(key =>
                {
                    key.PropertyRef(a => a.Id);
                    key.Column("AssignmentId");
                });
                mapper.Index(i => i.Column("Position"));
                mapper.Cascade(Cascade.All);
            }, r => r.Component(c =>
            {
                c.Property(x => x.Answer);
                c.Component(id => id.Identity, cmp =>
                {
                    cmp.Lazy(false);
                    cmp.Property(x => x.Id, pm => pm.Column("QuestionId"));
                    cmp.Property(x => x.RosterVector, pm =>
                    {
                        pm.Column(clmn =>
                        {
                            clmn.SqlType("integer[]");
                            clmn.Name("RosterVector");
                            clmn.NotNullable(true);
                        });

                        pm.Type<PostgresSqlConvertorType<int, RosterVector, RosterVectorTypeConvertor>>();
                    });
                });
                c.Property(x => x.AnswerAsString);
                c.Property(x => x.Assignment);
            }));

            Property(x => x.Answers, mapper =>
            {
                mapper.Lazy(true);
                mapper.Type<PostgresJson<IList<InterviewAnswer>>>();
            });

            ManyToOne(x => x.Responsible, mto =>
            {
                mto.Column("ResponsibleId");
                mto.Cascade(Cascade.None);
                mto.Update(false);
                mto.Insert(false);
            });

            Set(x => x.GpsAnswers, set =>
                {
                    //set.Table("assignment_geo_answers");
                    set.Key(key =>
                    {
                        key.PropertyRef(a => a.Id);
                        //key.Column("assignment_id");
                        key.Column(nameof(AssignmentGps.AssignmentId));
                    });
                    set.Lazy(CollectionLazy.Lazy);
                    set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    set.Inverse(true);
                },
                rel => rel.OneToMany());
        }
    }
}
