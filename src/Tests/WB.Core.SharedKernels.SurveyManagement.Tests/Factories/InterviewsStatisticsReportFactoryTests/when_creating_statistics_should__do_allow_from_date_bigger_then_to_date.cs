using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.InterviewsStatisticsReportFactoryTests
{
    internal class when_creating_statistics_should__do_allow_from_date_bigger_then_to_date : InterviewsStatisticsReportFactoryTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.NewGuid();
            var baseDate = new DateTime(2014, 8, 22);
            var questionnaireVersion = 1;

            var data = new List<StatisticsLineGroupedByDateAndTemplate>
            {
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate,
                    DateTicks = baseDate.Ticks,
                    ApprovedByHeadquartersCount = 0,
                    ApprovedBySupervisorCount = 0,
                    CompletedCount = 0,
                    InterviewerAssignedCount = 0,
                    RejectedByHeadquartersCount = 0,
                    RejectedBySupervisorCount = 0,
                    SupervisorAssignedCount = 0
                }
            }.AsQueryable();
            
            chartStatisticsFactory = CreateInterviewsStatisticsReportFactory(data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-1),
                To = baseDate.AddDays(-2)
            };
        };

        Because of = () => view = chartStatisticsFactory.Load(input);

        It should_have_days_count_muliply_two_records = () => view.Ticks.Length.ShouldEqual(0);

        private static ChartStatisticsFactory chartStatisticsFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;

    }
}
