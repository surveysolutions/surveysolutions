using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyViewFactoryTests
{
    internal class when_getting_details_view : SurveyViewFactoryTestsContext
    {
        Establish context = () =>
        {
            viewFromReader = new SurveyDetailsView { Name = "Survey" };

            var surveyDetailsViewRepositoryReader = Mock.Of<IQueryableReadSideRepositoryReader<SurveyDetailsView>>(reader
                => reader.GetById(surveyId) == viewFromReader);

            viewFactory = CreateSurveyViewFactory(surveyDetailsViewRepositoryReader: surveyDetailsViewRepositoryReader);
        };

        Because of = () =>
            result = viewFactory.GetDetailsView(surveyId);

        It should_return_view_provided_by_details_repository_reader = () =>
            result.ShouldEqual(viewFromReader);

        private static SurveyDetailsView result;
        private static SurveyDetailsView viewFromReader;
        private static SurveyViewFactory viewFactory;
        private static string surveyId = "survey-id";
    }
}