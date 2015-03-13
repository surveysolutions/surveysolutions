using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
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
            Map(x => x.AnswersToFeaturedQuestions,
                collection =>
                {
                    collection.Table("AnswersToFeaturedQuestions");
                    collection.Key(c => c.Column("InterviewSummaryId"));
                }, 
                rel => rel.Element(el => el.Column("QuestionId")), 
                map => map.Component(cmp => {
                    cmp.Property(x => x.Id, col => col.Column("AnswerId"));
                    cmp.Property(x => x.Title, col => col.Column("AnswerTitle"));
                    cmp.Property(x => x.Answer, col => col.Column("AnswerValue"));
                }));


            //public virtual IDictionary<Guid, QuestionAnswer> AnswersToFeaturedQuestions { get; set; }
            //public virtual IList<InterviewCommentedStatus> CommentedStatusesHistory { get; set; }
        }
    }
}