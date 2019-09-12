﻿using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentMap : ClassMapping<Assignment>
    {
        public AssignmentMap()
        {
            DynamicUpdate(true);

            Id(x => x.PublicKey, mapper => mapper.Column("PublicKey"));
            Property(x => x.Id, mapper => mapper.Unique(true));

            Property(x => x.ResponsibleId);
            Property(x => x.Quantity);
            Property(x => x.Archived);
            Property(x => x.CreatedAtUtc);
            Property(x => x.UpdatedAtUtc);
            Property(x => x.ReceivedByTabletAtUtc);
            Property(x => x.IsAudioRecordingEnabled);
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
                    key.Column("assignmentid");
                });
                set.Lazy(CollectionLazy.Extra);
                set.Cascade(Cascade.None);
                set.Schema(new UnitOfWorkConnectionSettings().ReadSideSchemaName);
            }, relation => relation.OneToMany());

            List(x => x.IdentifyingData, mapper =>
            {
                mapper.Table("AssignmentsIdentifyingAnswers");
                mapper.Key(k => k.Column("AssignmentId"));
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

        }
    }

    [PlainStorage]
    public class QuestionnaireLiteViewItemMap : ClassMapping<QuestionnaireLiteViewItem>
    {
        public QuestionnaireLiteViewItemMap()
        {
            this.Table("questionnairebrowseitems");

            Id(x => x.Id);
            Property(x => x.Title);
            Property(x => x.IsAudioRecordingEnabled);
        }
    }
}
