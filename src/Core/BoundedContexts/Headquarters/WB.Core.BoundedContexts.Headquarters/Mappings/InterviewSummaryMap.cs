using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewSummaryMap : ClassMapping<InterviewSummary>
    {
        public InterviewSummaryMap()
        {
            this.Table("InterviewSummaries");
            this.DynamicUpdate(true);

            Id(x => x.Id, p => p.Generator(Generators.Identity));

            this.PropertyKeyAlias(x => x.SummaryId, id => summary => summary.SummaryId == id);

            Property(x => x.QuestionnaireTitle);
            Property(x => x.QuestionnaireVariable, pm => pm.Column("questionnaire_variable"));
            Property(x => x.ResponsibleName);
            Property(x => x.ResponsibleNameLowerCase, pm => pm.Column("responsible_name_lower_case"));
            Property(x => x.SupervisorId, pm => pm.Column("teamleadid"));
            Property(x => x.SupervisorName, pm => pm.Column("teamleadname"));
            Property(x => x.SupervisorNameLowerCase, pm => pm.Column("teamlead_name_lower_case"));
            Property(x => x.ResponsibleRole);
            Property(x => x.UpdateDate, pm => pm.Type<UtcDateTimeType>());
            Property(x => x.WasCreatedOnClient);
            Property(x => x.WasRejectedBySupervisor);
            Property(x => x.WasCompleted);
            Property(x => x.InterviewId);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.ResponsibleId, pm => pm.Column(cm => cm.Index("InterviewSummaries_ResponsibleId")));
            Property(x => x.Status, pm => pm.Column(cm => cm.Index("InterviewSummaries_Status")));
            Property(x => x.Key);
            Property(x => x.QuestionnaireIdentity);
            Property(x => x.InterviewDurationLong, ptp => ptp.Column("interviewduration"));
            Property(x => x.LastResumeEventUtcTimestamp, pm => pm.Type<UtcDateTimeType>());
            Property(x => x.ClientKey);
            Property(x => x.FirstInterviewerName);
            Property(x => x.FirstSupervisorName);
            Property(x => x.FirstInterviewerId);
            Property(x => x.FirstSupervisorId);
            Property(x => x.CreatedDate);
            Property(x => x.FirstAnswerDate);
            Property(x => x.HasResolvedComments);
            Property(x => x.ErrorsCount);
            Property(x => x.NotAnsweredCount, ptp => ptp.Column("not_answered_count"));
            Property(x => x.CommentedEntitiesCount, clm =>
            {
                clm.Lazy(true);
                clm.Formula(
                    @"(SELECT COUNT(DISTINCT (c.summary_id, c.variable, c.rostervector)) FROM commentaries c 
                               WHERE c.summary_id = id AND c.variable not like '@@%')");
            });
            Property(x => x.AssignmentId);

            Property(x => x.ReceivedByInterviewerAtUtc, pm =>
            {
                pm.Type<UtcDateTimeType>();
                pm.Column(cm =>
                {
                    cm.Default(null);
                    cm.NotNullable(false);
                });
            });
            Property(x => x.IsAssignedToInterviewer, pm => pm.Column(cm =>
            {
                cm.Default(true);
                cm.NotNullable(true);
            }));

            Bag(x => x.IdentifyEntitiesValues,
                collection =>
                {
                    collection.Key(key => key.Column("interview_id"));
                    collection.OrderBy(x => x.Position);
                    collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    
                    collection.Inverse(true);
                },
                rel => rel.OneToMany());

            Bag(x => x.InterviewCommentedStatuses, listMap =>
                {
                    listMap.Table("InterviewCommentedStatuses");

                    listMap.Key(key => key.Column("interview_id"));
                    listMap.Lazy(CollectionLazy.Lazy);
                    listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    listMap.Inverse(true);
                    listMap.OrderBy(x => x.Position);
                },
                rel => rel.OneToMany()
            );

            Set(x => x.TimeSpansBetweenStatuses, set =>
                {
                    set.Key(key => key.Column("interview_id"));
                    set.Lazy(CollectionLazy.Lazy);
                    set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    set.Inverse(true);
                },
                rel => { rel.OneToMany(); }
            );

            Set(x => x.GpsAnswers, set =>
                {
                    set.Key(key => key.Column("interview_id"));
                    set.Lazy(CollectionLazy.Lazy);
                    set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    set.Inverse(true);
                },
                rel => rel.OneToMany());

            Set(x => x.StatisticsReport, listMap =>
            {
                listMap.Table("report_statistics");

                listMap.Key(key => key.Column("interview_id"));

                listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
                listMap.Lazy(CollectionLazy.Lazy);
                listMap.Inverse(true);
            }, rel => rel.OneToMany());

            Set(x => x.Comments, sm =>
            {
                sm.Table("commentaries");
                sm.Key(k => k.Column("summary_id"));
                sm.Cascade(Cascade.All | Cascade.DeleteOrphans);
                sm.Lazy(CollectionLazy.Lazy);
                sm.Inverse(true);
            }, rel => rel.OneToMany());

            Property(x => x.HasSmallSubstitutions);
        }
    }

    public class InterviewCommentedStatusMap : ClassMapping<InterviewCommentedStatus>
    {
        public InterviewCommentedStatusMap()
        {
            Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Assigned);
            });
            Property(x => x.SupervisorId);
            Property(x => x.InterviewerId);
            Property(x => x.StatusChangeOriginatorId);
            Property(x => x.Timestamp, tm => tm.Column("timestamp"));
            Property(x => x.StatusChangeOriginatorName);
            Property(x => x.StatusChangeOriginatorRole);
            Property(x => x.Status);
            Property(x => x.Comment);
            Property(x => x.TimespanWithPreviousStatusLong, clm =>
            {
                clm.Column("TimeSpanWithPreviousStatus");
            });
            Property(x => x.SupervisorName);
            Property(x => x.InterviewerName);

            Property(x => x.Position);

            ManyToOne(x => x.InterviewSummary, mto => mto.Column("interview_id"));
        }
    }

    public class IdentifyEntityValueMap : ClassMapping<IdentifyEntityValue>
    {
        public IdentifyEntityValueMap()
        {
            Id(x => x.Id, idMap => idMap.Generator(Generators.HighLow));

            Table("identifyingentityvalue");

            Property(x => x.Position, col => col.Column("Position"));
            Property(x => x.Value, col => col.Column("value"));
            Property(x => x.AnswerCode, col => col.Column("answer_code"));
            Property(x => x.ValueLowerCase, col => col.Column("value_lower_case"));
            
            ManyToOne(x => x.Entity, mtm =>
            {
                mtm.Lazy(LazyRelation.Proxy);
                mtm.Column("entity_id");                
            });

            ManyToOne(x => x.InterviewSummary, mtm => mtm.Column("interview_id"));
        }
    }
}
