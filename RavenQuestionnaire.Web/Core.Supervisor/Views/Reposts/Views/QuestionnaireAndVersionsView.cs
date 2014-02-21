using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class QuestionnaireAndVersionsView
    {
        public QuestionnaireAndVersionsItem[] Items { get; set; }
        public int TotalCount { get; set; }
    }
}