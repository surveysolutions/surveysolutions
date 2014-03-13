using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SurveyDetailsView : IView
    {
        public string SurveyId { get; set; }
        public string Name { get; set; }
    }
}