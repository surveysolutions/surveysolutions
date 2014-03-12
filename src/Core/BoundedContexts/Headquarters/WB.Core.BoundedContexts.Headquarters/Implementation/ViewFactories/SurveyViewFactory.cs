using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories
{
    internal class SurveyViewFactory : ISurveyViewFactory
    {
        public SurveyLineView[] GetAllLineViews()
        {
            return new[]
            {
                new SurveyLineView { Name = "Survey 1" },
                new SurveyLineView { Name = "Survey 2" },
            };
        }
    }
}