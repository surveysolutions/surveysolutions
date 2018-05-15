using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ReportsTests
{
    [TestFixture]
    public class when_build_report_with_NumericalReportViewBuilder
    {
        [Test]
        public void sample_test_with_special_values()
        {
            // arrange
            var fixture = Create.Other.AutoFixture();

            var numericalData = fixture.CreateMany<GetNumericalReportItem>(10).ToList();

            // add total row
            numericalData[0].TeamLeadName = null;
            numericalData[0].ResponsibleName = null;

            var item1 = new GetCategoricalReportItem
            {
                TeamLeadName = numericalData[1].TeamLeadName,
                ResponsibleName = numericalData[1].ResponsibleName,
                Answer = 1,
                Count = 30
            };

            var item2 = new GetCategoricalReportItem
            {
                TeamLeadName = numericalData[1].TeamLeadName,
                ResponsibleName = numericalData[1].ResponsibleName,
                Answer = 2,
                Count = 20
            };

            var item3 = new GetCategoricalReportItem
            {
                TeamLeadName = numericalData[2].TeamLeadName,
                ResponsibleName = numericalData[2].ResponsibleName,
                Answer = 1,
                Count = 10
            };

            var categories = new CategoricalReportViewBuilder(
                new List<Answer>
                {
                    new Answer{AnswerText = "Hey", AnswerCode = 1},
                    new Answer{AnswerText = "Hoy", AnswerCode = 2}
                },
                new List<GetCategoricalReportItem>
                {
                    item1, item2, item3
                });
            
            // act
            var report = new NumericalReportViewBuilder(numericalData, categories).AsReportView();

            // assert

            var arrangeItem = ToRow(numericalData[1])
                .Concat(new object[] { item1.Count, item2.Count, item1.Count + item2.Count });
            var reportViewItem = report.Data[0];
            Assert.That(reportViewItem, Is.EqualTo(arrangeItem));

            //check that zeros are filled for missing items

            var arrangeItem2 = ToRow(numericalData[2])
                .Concat(new object[] { item3.Count, 0L, item3.Count });
            var reportViewItem2 = report.Data[1];
            Assert.That(reportViewItem2, Is.EqualTo(arrangeItem2));
        }

        [Test]
        public void sample_test_with_no_special_values()
        {
            // arrange
            var fixture = Create.Other.AutoFixture();

            var numericalData = fixture.CreateMany<GetNumericalReportItem>(10).ToList();

            // add total row
            numericalData[0].TeamLeadName = null;
            numericalData[0].ResponsibleName = null;

            CategoricalReportViewBuilder categories = new CategoricalReportViewBuilder(
                new List<Answer>(), Enumerable.Empty<GetCategoricalReportItem>());

            // act
            var report = new NumericalReportViewBuilder(numericalData, categories).AsReportView();

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
