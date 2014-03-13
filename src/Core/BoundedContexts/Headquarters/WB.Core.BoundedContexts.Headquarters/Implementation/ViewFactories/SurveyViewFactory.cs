using System.Linq;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories
{
    internal class SurveyViewFactory : ISurveyViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<SurveyLineView> surveyLineViewRepositoryReader;

        public SurveyViewFactory(IQueryableReadSideRepositoryReader<SurveyLineView> surveyLineViewRepositoryReader)
        {
            this.surveyLineViewRepositoryReader = surveyLineViewRepositoryReader;
        }

        public SurveyLineView[] GetAllLineViews()
        {
            return this.surveyLineViewRepositoryReader.Query(views => views.ToArray());
        }
    }
}