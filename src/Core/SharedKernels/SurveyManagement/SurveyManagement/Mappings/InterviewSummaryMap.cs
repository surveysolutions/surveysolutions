using NHibernate;
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
            Table("InterviewSummaries");
            DynamicUpdate(true);
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
                    collection.Key(c => {
                        c.Column("InterviewSummaryId");
                        
                    });
                    collection.Table("AnswersToFeaturedQuestions");
                    collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                    collection.Inverse(true);
                    collection.Lazy(CollectionLazy.NoLazy);
                    
                    collection.Fetch(CollectionFetchMode.Subselect);
                },
                rel => { 
                    rel.OneToMany();
                    
                });

            Set(x => x.QuestionOptions,
               collection =>
               {
                   collection.Key(key => key.Column("InterviewSummaryId"));
                   collection.Cascade(Cascade.All | Cascade.DeleteOrphans);
                   collection.Inverse(true);
                   collection.Lazy(CollectionLazy.NoLazy);
                   collection.Fetch(CollectionFetchMode.Subselect);
               },
               relation => relation.OneToMany());
        }
    }

    public class QuestionOptionsMap : ClassMapping<QuestionOptions>
    {
        public QuestionOptionsMap()
        {
            Id(x => x.Id, id => id.Generator(Generators.HighLow));
            Property(x => x.QuestionId);
            Property(x => x.Text);
            Property(x => x.Value);
            ManyToOne(x => x.InterviewSummary, mtm => {
                mtm.Column("InterviewSummaryId");
                mtm.Index("InterviewSummaries_QuestionOptions"); });
        }
    }


    public class QuestionAnswerMap : ClassMapping<QuestionAnswer>
    {
        public QuestionAnswerMap()
        {
            Id(x => x.Id, idMap => idMap.Generator(Generators.HighLow));
            Property(x => x.Questionid, clm => clm.Column("QuestionId"));
            Property(x => x.Title, col => col.Column("AnswerTitle"));
            Property(x => x.Answer, col => col.Column("AnswerValue"));
            ManyToOne(x => x.InterviewSummary, mtm => {
                mtm.Column("InterviewSummaryId");
                mtm.Index("InterviewSummaries_QuestionAnswers"); });
        }
    }
}