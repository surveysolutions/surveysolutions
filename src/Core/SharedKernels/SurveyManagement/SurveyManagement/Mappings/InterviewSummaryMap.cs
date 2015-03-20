using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Raven.Client.Linq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewSummaryMap : ClassMapping<InterviewSummary>
    {
        public InterviewSummaryMap()
        {
            this.Table("InterviewSummaries");
            Id(x => x.SummaryId);
            Property(x => x.QuestionnaireTitle);
            Property(x => x.ResponsibleName);
            Property(x => x.TeamLeadId);
            Property(x => x.TeamLeadName);
            Property(x => x.ResponsibleRole);
            Property(x => x.UpdateDate);
            Property(x => x.WasCreatedOnClient);
            Property(x => x.InterviewId);
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.ResponsibleId);
            Property(x => x.Status);
            Property(x => x.IsDeleted);
            Property(x => x.HasErrors);

            Set(x => x.AnswersToFeaturedQuestions,
                collection => {
                    collection.Key(c => c.Column("InterviewSummaryId"));
                    collection.Table("AnswersToFeaturedQuestions");
                    collection.Cascade(Cascade.All);
                    collection.Inverse(true);
                    collection.BatchSize(12);
                },
                rel => rel.OneToMany());

            Set(x => x.CommentedStatusesHistory,
                collection =>
                {
                    collection.Key(key => key.Column("InterviewSummaryId"));
                    collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    collection.Inverse(true);
                },
                relation => relation.OneToMany());

            Set(x => x.QuestionOptions,
               collection =>
               {
                   collection.Table("QuestionOptions");
                   collection.Key(key => key.Column("InterviewSummaryId"));
                   collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
               },
               relation => relation.Component(element =>
               {
                   element.Property(x => x.QuestionId);
                   element.Property(x => x.Text);
                   element.Property(x => x.Value);
               }));
        }
    }

    public class InterviewCommentedStatusMap : ClassMapping<InterviewCommentedStatus>
    {
        public InterviewCommentedStatusMap()
        {
            Table("CommentedStatusesHistoryItems");   
            Id(x => x.Id, idMap => idMap.Generator(Generators.HighLow));
            Property(x => x.Comment);
            Property(x => x.Date);
            Property(x => x.Status);
            Property(x => x.Responsible);
            Property(x => x.ResponsibleId);
        }
    }

    public class QuestionAnswerMap : ClassMapping<QuestionAnswer>
    {
        public QuestionAnswerMap()
        {
            this.Table("AnswersToFeaturedQuestions");

            this.ComposedId(cmp =>
            {
                cmp.ManyToOne(x => x.InterviewSummary, clm => clm.Column("InterviewSummaryId"));
                cmp.Property(x => x.Id, clm => clm.Column("id"));
            });

            Property(x => x.Title, col => col.Column("AnswerTitle"));
            Property(x => x.Answer, col => col.Column("AnswerValue"));
        }
    }
}