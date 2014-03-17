using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyViewFactoryTests
{
    [Subject(typeof(SurveyViewFactory))]
    internal class SurveyViewFactoryTestsContext
    {
        protected static SurveyViewFactory CreateSurveyViewFactory(
            IQueryableReadSideRepositoryReader<SurveyLineView> surveyLineViewRepositoryReader = null,
            IQueryableReadSideRepositoryReader<SurveyDetailsView> surveyDetailsViewRepositoryReader = null)
        {
            return new SurveyViewFactory(
                surveyLineViewRepositoryReader ?? Mock.Of<IQueryableReadSideRepositoryReader<SurveyLineView>>(),
                surveyDetailsViewRepositoryReader ?? Mock.Of<IQueryableReadSideRepositoryReader<SurveyDetailsView>>());
        }
    }
}