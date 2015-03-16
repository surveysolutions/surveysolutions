using System.CodeDom;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes
{
    public class InterviewsSearchIndex : AbstractIndexCreationTask<InterviewSummary, SeachIndexContent> 
    {
        public InterviewsSearchIndex()
        {
            Map = interviews => from interview in interviews
                                where interview.IsDeleted == false
                                select new SeachIndexContent
                                {
                                    IsDeleted = interview.IsDeleted,
                                    //FeaturedQuestionsWithAnswers = string.Join(" ", interview.AnswersToFeaturedQuestions.Select(x => x.Value).Select(x => x.Answer + " " + x.Title)),
                                    //AnswersToFeaturedQuestions = interview.AnswersToFeaturedQuestions,
                                    TeamLeadId = interview.TeamLeadId,
                                    ResponsibleId = interview.ResponsibleId,
                                    ResponsibleName = interview.ResponsibleName,
                                    HasErrors = interview.HasErrors,
                                    InterviewId = interview.InterviewId,
                                    ResponsibleRole = interview.ResponsibleRole,
                                    WasCreatedOnClient = interview.WasCreatedOnClient,
                                    Status = interview.Status,
                                    QuestionnaireVersion = interview.QuestionnaireVersion,
                                    UpdateDate = interview.UpdateDate,
                                    QuestionnaireId = interview.QuestionnaireId,
                                    QuestionnaireTitle = interview.QuestionnaireTitle
                                };
            
            Analyze(x => x.FeaturedQuestionsWithAnswers, "Lucene.Net.Analysis.Standard.StandardAnalyzer");
            Store(x => x.FeaturedQuestionsWithAnswers, FieldStorage.No);
            Index(x => x.FeaturedQuestionsWithAnswers, FieldIndexing.Analyzed);
            Index(x => x.IsDeleted, FieldIndexing.Default);
            Index(x => x.TeamLeadId, FieldIndexing.NotAnalyzed);
            Index(x => x.ResponsibleId, FieldIndexing.NotAnalyzed);
            Index(x => x.Status, FieldIndexing.NotAnalyzed);
            Index(x => x.QuestionnaireVersion, FieldIndexing.NotAnalyzed);
            Index(x => x.UpdateDate, FieldIndexing.NotAnalyzed);
            Index(x => x.QuestionnaireId, FieldIndexing.NotAnalyzed);
            Index(x => x.QuestionnaireTitle, FieldIndexing.NotAnalyzed);

            Sort(x => x.UpdateDate, SortOptions.String);
        }
    }
}