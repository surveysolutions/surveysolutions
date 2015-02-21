using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_statistics_filter_could_not_find_questionnaire : ChartStatisticsViewFactoryTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Empty;
            var questionnaireVersion = 1;

            baseDate = new DateTime(2014, 8, 22);

            var readSideRepositoryReader = Mock.Of<IReadSideKeyValueStorage<StatisticsGroupedByDateAndTemplate>>(_
                => _.GetById(Moq.It.IsAny<string>()) == null as StatisticsGroupedByDateAndTemplate);

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(readSideRepositoryReader);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-2),
                To = baseDate
            };
        };

        Because of = () => view = chartStatisticsViewFactory.Load(input);

        It should_have_0_lines = () => view.Lines.Length.ShouldEqual(0);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
        private static DateTime baseDate;
    }
}