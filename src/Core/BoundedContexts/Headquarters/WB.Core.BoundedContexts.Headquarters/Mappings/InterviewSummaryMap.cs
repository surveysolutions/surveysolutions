using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewSummaryMap : ClassMapping<InterviewSummary>
    {
        public InterviewSummaryMap()
        {
            this.Table("InterviewSummaries");
            this.DynamicUpdate(true);
            Id(x => x.SummaryId);
            Property(x => x.QuestionnaireTitle);
            Property(x => x.ResponsibleName);
            Property(x => x.TeamLeadId, pm => pm.Column(cm => cm.Index("InterviewSummaries_TeamLeadId")));
            Property(x => x.TeamLeadName);
            Property(x => x.ResponsibleRole);
            Property(x => x.UpdateDate);
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
            Property(x => x.LastResumeEventUtcTimestamp);
            Property(x => x.ClientKey);
            Property(x => x.ErrorsCount);
            Property(x => x.AssignmentId);
            Property(x => x.ReceivedByInterviewer, pm => pm.Column(cm =>
            {
                cm.Default(false);
                cm.NotNullable(true);
            }));
            Property(x => x.IsAssignedToInterviewer, pm => pm.Column(cm =>
            {
                cm.Default(true);
                cm.NotNullable(true);
            }));

            Bag(x => x.AnswersToFeaturedQuestions,
                collection => {
                    collection.Key(c => {
                        c.Column("InterviewSummaryId");
                    });
                    collection.OrderBy(x  =>  x.Position);
                    collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    collection.Inverse(true);
                },
                rel => { 
                    rel.OneToMany();
                });

            Bag(x => x.InterviewCommentedStatuses, listMap =>
                {
                    listMap.Table("InterviewCommentedStatuses");
                    
                    listMap.Key(keyMap =>
                    {
                        keyMap.Column(clm =>
                        {
                            clm.Name("InterviewId");
                        });
                    });
                    listMap.Lazy(CollectionLazy.Lazy);
                    listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    listMap.Inverse(true);
                    listMap.OrderBy(x => x.Position);
                },
                rel => rel.OneToMany()
            );

            Set(x => x.TimeSpansBetweenStatuses, set => {
                    set.Table("timespanbetweenstatuses");
                    set.Key(key =>
                    {
                        key.Column(cm =>
                        {
                            cm.Name("InterviewId");
                            cm.Index("InterviewSummary_InterviewStatusTimeSpans");
                        });
                        key.ForeignKey("FK_InterviewSummary_TimeSpansBetweenStatuses");
                    });
                    set.Lazy(CollectionLazy.Lazy);
                    set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                },
                rel => {
                    rel.OneToMany();
                }
            );
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

            ManyToOne(x => x.InterviewSummary, mto => mto.Column("InterviewId"));
        }
    }

    public class QuestionAnswerMap : ClassMapping<QuestionAnswer>
    {
        public QuestionAnswerMap()
        {
            Id(x => x.Id, idMap => idMap.Generator(Generators.HighLow));
            this.Table("AnswersToFeaturedQuestions");
            Property(x => x.Questionid, clm => clm.Column("QuestionId"));
            Property(x => x.Title, col => col.Column("AnswerTitle"));
            Property(x => x.Position, col => col.Column("Position"));
            Property(x => x.Answer, col =>
            {
                col.Column("AnswerValue");
            });
            ManyToOne(x => x.InterviewSummary, mtm => {
                mtm.Column("InterviewSummaryId");
                mtm.Index("InterviewSummaries_QuestionAnswers");
                mtm.ForeignKey("FK_InterviewSummaries_AnswersToFeaturedQuestions");
            });
        }
    }
}
