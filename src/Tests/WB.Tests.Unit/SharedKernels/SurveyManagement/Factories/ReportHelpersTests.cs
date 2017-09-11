using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories
{
    [TestOf(typeof(ReportHelpers))]
    public class ReportHelpersTests
    {
        [Test]
        public void should_calculate_report_timespans_corrently_for_weeks()
        {
            var userSelectedFrom = new DateTime(2010, 5, 8);
            var userTimeZoneAdjastsment = 120;

            var queryableReadSideRepositoryReader = new TestInMemoryWriter<InterviewSummary>();
            queryableReadSideRepositoryReader.Store(Create.Entity.InterviewSummary(statuses: new List<InterviewCommentedStatus>
            {
                Create.Entity.InterviewCommentedStatus(timestamp: userSelectedFrom.AddMinutes(10))
            }), "1");

            // Act
            var reportTimeline = ReportHelpers.BuildColumns(userSelectedFrom, "w", 1, userTimeZoneAdjastsment, null, queryableReadSideRepositoryReader);

            // Assert
            DateTimeRange lastUtcTimeSpan = reportTimeline.ColumnRangesUtc.Last();
            var expectedUtcFrom = userSelectedFrom.AddMinutes(userTimeZoneAdjastsment).AddDays(1).AddDays(-7);
            var expectedUtcTo = userSelectedFrom.AddMinutes(userTimeZoneAdjastsment).AddDays(1);

            Assert.Multiple(() =>
            {
                Assert.That(lastUtcTimeSpan, Has.Property(nameof(DateTimeRange.From)).EqualTo(expectedUtcFrom));
                Assert.That(lastUtcTimeSpan, Has.Property(nameof(DateTimeRange.To)).EqualTo(expectedUtcTo));
            });

            Assert.Multiple(() =>
            {
                Assert.That(reportTimeline.ColumnRangesLocal.Last(), Has.Property(nameof(DateTimeRange.From)).EqualTo(userSelectedFrom.AddDays(-6)));
                Assert.That(reportTimeline.ColumnRangesLocal.Last(), Has.Property(nameof(DateTimeRange.To)).EqualTo(userSelectedFrom.AddDays(1)));
            });
        }

        [Test]
        public void should_calculate_report_timespans_corrently_for_days()
        {
            var userSelectedFrom = new DateTime(2010, 5, 7);
            var timezoneAdjastmentMins = 120;

            var queryableReadSideRepositoryReader = new TestInMemoryWriter<InterviewSummary>();
            queryableReadSideRepositoryReader.Store(Create.Entity.InterviewSummary(statuses: new List<InterviewCommentedStatus>
            {
                Create.Entity.InterviewCommentedStatus(timestamp: userSelectedFrom.AddMinutes(timezoneAdjastmentMins + 10))
            }), "1");
            var reportTimeline = ReportHelpers.BuildColumns(userSelectedFrom, "d", 2, timezoneAdjastmentMins, null, queryableReadSideRepositoryReader);

            Assert.That(reportTimeline.ColumnRangesUtc, Has.Length.EqualTo(1), "First timespan is completely before of first interview created date and should not be counted");

            DateTimeRange lastUtcTimeSpan = reportTimeline.ColumnRangesUtc.Last();
            Assert.Multiple(() =>
            {
                Assert.That(lastUtcTimeSpan, Has.Property(nameof(DateTimeRange.From)).EqualTo(userSelectedFrom.AddMinutes(timezoneAdjastmentMins)));
                Assert.That(lastUtcTimeSpan, Has.Property(nameof(DateTimeRange.To)).EqualTo(userSelectedFrom.AddDays(1).AddMinutes(timezoneAdjastmentMins)));
            });

            var lastLocalTimeSpan = reportTimeline.ColumnRangesLocal.Last();
            Assert.Multiple(() =>
            {
                Assert.That(lastLocalTimeSpan, Has.Property(nameof(DateTimeRange.From)).EqualTo(userSelectedFrom));
                Assert.That(lastLocalTimeSpan, Has.Property(nameof(DateTimeRange.To)).EqualTo(userSelectedFrom.AddDays(1)));
            });
        }
    }
}