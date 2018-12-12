﻿using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ChartStatisticsViewFactoryTests
{
    internal class when_statistics_filter_without_data : ChartStatisticsViewFactoryTestsContext
    {
        [OneTimeSetUp]
        public void Establish()
        {
            var questionnaireId = Guid.NewGuid();
            baseDate = new DateTime(2014, 8, 22);
            var questionnaireVersion = 1;

            var data =
                new StatisticsGroupedByDateAndTemplate
                {
                    StatisticsByDate =
                        new Dictionary<DateTime, QuestionnaireStatisticsForChart>()
                        {
                            {
                                baseDate.AddDays(-3),
                                new QuestionnaireStatisticsForChart
                                {
                                    ApprovedByHeadquartersCount = 1,
                                    ApprovedBySupervisorCount = 1,
                                    CompletedCount = 1,
                                    RejectedByHeadquartersCount = 1,
                                    RejectedBySupervisorCount = 1,
                                }
                            }
                        }
                };

            chartStatisticsViewFactory = CreateChartStatisticsViewFactory(statistics: data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-2),
                To = baseDate.AddDays(-1)
            };
            Because();
        }

        public void Because() => view = chartStatisticsViewFactory.Load(input);

        [Test]
        public void should_have_5_lines() => view.Lines.Length.Should().Be(5);

        [Test]
        public void should_each_line_has_2_days_inside() => view.Lines.Should().OnlyContain(line => line.Length == 2);

        [Test]
        public void should_each_line_has_first_record_equal_to_from_date_and_with_count_equal_to_1() => view.Lines.Should().OnlyContain(line => line[0][0].ToString() == baseDate.AddDays(-2).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) && (int)line[0][1] == 1);

        [Test]
        public void should_each_line_has_second_record_equal_to_to_date_and_with_count_equal_to_1() => view.Lines.Should().OnlyContain(line => line[1][0].ToString() == baseDate.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) && (int)line[1][1] == 1);

        private static ChartStatisticsViewFactory chartStatisticsViewFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
        private static DateTime baseDate;
    }
}
