﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    [PlainStorage]
    public class AssignmentMap : ClassMapping<Assignment>
    {
        public AssignmentMap()
        {
            Id(x => x.Id, mapper => mapper.Generator(Generators.Identity));
            DynamicUpdate(true);
            Property(x => x.ResponsibleId);
            Property(x => x.Quantity);
            Property(x => x.Archived);
            Property(x => x.CreatedAtUtc);
            Property(x => x.UpdatedAtUtc);

            Component(x => x.QuestionnaireId, cmp =>
            {
                cmp.Lazy(false);
                cmp.Property(x => x.QuestionnaireId);
                cmp.Property(x => x.Version, ptp => ptp.Column("QuestionnaireVersion"));
            });

            Set(x => x.InterviewSummaries, set =>
            {
                set.Key(key => key.Column("assignmentid"));
                set.Lazy(CollectionLazy.Lazy);
                set.Cascade(Cascade.None);
            },

            relation => relation.OneToMany());

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
    public class InterviewSummaryMap : ClassMapping<InterviewSummary>
    {
        public InterviewSummaryMap()
        {
            Schema("readside");
            this.Table("InterviewSummaries");
            this.DynamicUpdate(true);
            Id(x => x.SummaryId);
            Property(x => x.AssignmentId);
            Property(x => x.IsDeleted);
        }
    }
}