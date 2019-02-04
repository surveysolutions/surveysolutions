using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Tests.Abc;

namespace WB.Tests.Integration.ReportTests.ChartStatisticsViewFactoryTests
{
    internal class when_statistics_filter_out_data_by_date : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var questionnaireId = Guid.NewGuid();
            baseDate = new DateTime(2014, 8, 22);

            var qid = Create.Entity.QuestionnaireIdentity(questionnaireId);

            CreateQuestionnaireStatisticsForChartWithSameCountForAllStatuses(qid, baseDate.AddDays(-3), 1);

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory();

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = qid.QuestionnaireId,
                QuestionnaireVersion = qid.Version,
                From = baseDate.AddDays(-2),
                To = baseDate.AddDays(-1)
            };

            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_have_5_lines() => view.DataSets.Count.Should().Be(5);

        [Test]
        public void should_each_line_has_2_days_inside() => view.DataSets.Should().OnlyContain(line => line.Data.Count == 2);

        [Test]
        public void should_each_line_has_first_record_equal_to_from_date_and_with_count_equal_to_1() 
            => view.DataSets.Should().OnlyContain(line => 
                line.Data[0].X.ToString() == baseDate.AddDays(-2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) 
                && (int)line.Data[0].Y == 1);

        [Test]
        public void should_each_line_has_second_record_equal_to_to_date_and_with_count_equal_to_1() 
            => view.DataSets.Should().OnlyContain(line => 
                line.Data[1].X.ToString() == baseDate.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) 
                && (int)line.Data[1].Y== 1);

        private ChartStatisticsViewFactory chartStatisticsViewFactory;
        private ChartStatisticsInputModel input;
        private ChartStatisticsView view;
        private DateTime baseDate;
    }
}
