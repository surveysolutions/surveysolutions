using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes
{
    public class InterviewsSearchIndex : AbstractIndexCreationTask<InterviewSummary>
    {
        public InterviewsSearchIndex()
        {
            Map = interviews => from interview in interviews
                                select new
                                {
                                    interview.IsDeleted,
                                    interview.AnswersToFeaturedQuestions,
                                    interview.TeamLeadId,
                                    interview.ResponsibleId,
                                    interview.ResponsibleName,
                                    interview.HasErrors,
                                    interview.Status,
                                    interview.QuestionnaireVersion,
                                    interview.UpdateDate,
                                    interview.QuestionnaireId
                                };
            Analyze(x => x.AnswersToFeaturedQuestions, "Lucene.Net.Analysis.Standard.StandardAnalyzer");
            Index(x => x.AnswersToFeaturedQuestions, FieldIndexing.Analyzed);
            Index(x => x.IsDeleted, FieldIndexing.Analyzed);
            Index(x => x.TeamLeadId, FieldIndexing.NotAnalyzed);
            Index(x => x.ResponsibleId, FieldIndexing.NotAnalyzed);
            Index(x => x.Status, FieldIndexing.NotAnalyzed);
            Index(x => x.QuestionnaireVersion, FieldIndexing.NotAnalyzed);
            Index(x => x.UpdateDate, FieldIndexing.NotAnalyzed);
            Index(x => x.QuestionnaireId, FieldIndexing.NotAnalyzed);
        }
    }
}