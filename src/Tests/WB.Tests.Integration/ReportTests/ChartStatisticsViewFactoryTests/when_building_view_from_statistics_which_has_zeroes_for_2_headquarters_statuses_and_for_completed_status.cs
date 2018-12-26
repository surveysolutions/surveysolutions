using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    public class when_building_view_from_statistics_which_has_zeroes_for_2_headquarters_statuses_and_for_completed_status : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {   
            var day1 = new DateTime(2014, 8, 20);
            var day2 = day1.AddDays(1);
            var day3 = day1.AddDays(2);

            var qid = Create.Entity.QuestionnaireIdentity();


            AddInterviewStatuses(qid, day1,
                new[] { InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor },
                new[] { InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor });

            AddInterviewStatuses(qid, day2,
                            new[] { InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor },
                            new[] { InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor });

            AddInterviewStatuses(qid, day3,
                            new[] { InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor },
                            new[] { InterviewStatus.Completed, InterviewStatus.Completed });

            input = new ChartStatisticsInputModel
            {
                CurrentDate = new DateTime(2014, 8, 22),
                QuestionnaireId = qid.QuestionnaireId,
                QuestionnaireVersion = qid.Version,
                From = new DateTime(2014, 8, 20),
                To = new DateTime(2014, 8, 22),
            };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            Because();
        }

        public void Because() =>
            view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_return_3_lines_the_same_as_statuses_count() => view.DataSets.Count.Should().Be(3);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
    }
}
