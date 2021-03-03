using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class GpsAnswerQuery
    {
        public InterviewGps Answer { get; set; }
        public InterviewSummary InterviewSummary { get; set; }
        public QuestionnaireCompositeItem QuestionnaireItem { get; set; }
    }
}
