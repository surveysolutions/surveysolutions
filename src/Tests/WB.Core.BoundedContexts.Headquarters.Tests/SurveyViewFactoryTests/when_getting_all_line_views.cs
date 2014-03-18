using System;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyViewFactoryTests
{
    internal class when_getting_all_line_views : SurveyViewFactoryTestsContext
    {
        Establish context = () =>
        {
            viewsFromReader = new[]
            {
                new SurveyLineView { SurveyId = "1", Name = "Survey One" },
                new SurveyLineView { SurveyId = "2", Name = "Survey Two" },
                new SurveyLineView { SurveyId = "6", Name = "Survey Six" },
            };

            var surveyLineViewRepositoryReader = Mock.Of<IQueryableReadSideRepositoryReader<SurveyLineView>>();
            Mock.Get(surveyLineViewRepositoryReader)
                .Setup(reader => reader.QueryAll(it.IsAny<Expression<Func<SurveyLineView, bool>>>()))
                .Returns<Expression<Func<SurveyLineView, bool>>>(condition => viewsFromReader.Where(condition.Compile()));

            viewFactory = CreateSurveyViewFactory(surveyLineViewRepositoryReader: surveyLineViewRepositoryReader);
        };

        Because of = () =>
            result = viewFactory.GetAllLineViews();

        It should_return_all_views_queried_from_line_repository_reader_via_query_all_method = () =>
            result.ShouldContainOnly(viewsFromReader);

        private static SurveyLineView[] result;
        private static SurveyLineView[] viewsFromReader;
        private static SurveyViewFactory viewFactory;
    }
}