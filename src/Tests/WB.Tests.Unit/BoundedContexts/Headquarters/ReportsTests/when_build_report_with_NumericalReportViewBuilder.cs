using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ReportsTests
{
    [TestFixture]
    public class when_build_report_with_NumericalReportViewBuilder
    {
        [Test]
        public void sample_test_with_no_special_values()
        {
            // arrange
            var fixture = Create.Other.AutoFixture();

            var numericalData = fixture.CreateMany<GetNumericalReportItem>(10).ToList();

            // add total row
            numericalData[0].TeamLeadName = null;
            numericalData[0].ResponsibleName = null;
            
            // act
            var report = new NumericalReportViewBuilder(numericalData).AsReportView();

            // assert
            Assert.That(report.Totals.Skip(2), Is.EqualTo(ToRow(numericalData[0]).Skip(2)));

            for (var index = 0; index < numericalData.Skip(1).ToArray().Length; index++)
            {
                var arrangeItem = numericalData[index + 1];
                var reportViewItem = report.Data[index];

                Assert.That(reportViewItem, Is.EqualTo(ToRow(arrangeItem)));
            }
        }

        IEnumerable<object> ToRow(GetNumericalReportItem item)
        {
            yield return item.TeamLeadName;
            yield return item.ResponsibleName;
            yield return item.Count;
            yield return item.Average;
            yield return item.Median;
            yield return item.Sum;
            yield return item.Min;
            yield return item.Percentile05;
            yield return item.Percentile50;
            yield return item.Percentile95;
            yield return item.Max;
        }
    }
}
