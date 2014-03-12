using WB.Core.BoundedContexts.Headquarters.Views.Survey;

namespace WB.Core.BoundedContexts.Headquarters.ViewFactories
{
    public interface ISurveyViewFactory
    {
        SurveyLineView[] GetAllLineViews();
    }
}