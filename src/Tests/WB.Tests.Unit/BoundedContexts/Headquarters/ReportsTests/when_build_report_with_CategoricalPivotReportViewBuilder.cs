using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ReportsTests
{
    [TestFixture]
    public class when_build_report_with_CategoricalPivotReportViewBuilder
    {
        private static class Sex
        {
            public const int Male = 1; public const int Female = 2;
        }

        private static class Position
        {
            public const int QA = 1; public const int Dev = 2;
        }

        private IQuestion SexQuestion;


        private IQuestion PositionQuestion;
        public List<GetReportCategoricalPivotReportItem> GetPivotDataResponse { get; set; }
        private ReportView report;

        [OneTimeSetUp]
        public void Context()
        {
            SexQuestion = PrepareQuestion(nameof(Sex),
                (Sex.Male, nameof(Sex.Male)),
                (Sex.Female, nameof(Sex.Female)));

            PositionQuestion = PrepareQuestion(
                nameof(Position),
                (Position.Dev, nameof(Position.Dev)),
                (Position.QA, nameof(Position.QA)));

            GetPivotDataResponse = new List<GetReportCategoricalPivotReportItem>
            {
                NewItem(row: Sex.Male,   column: Position.Dev, count: 5),
                NewItem(row: Sex.Female, column: Position.QA,  count: 2),
                NewItem(row: Sex.Male,   column: Position.QA,  count: 2)
            };

            var builder = new CategoricalPivotReportViewBuilder(
                columnQuestion: PositionQuestion, 
                rowsQuestion: SexQuestion, 
                items: GetPivotDataResponse);

            this.report = builder.AsReportView();
            /*
             *  Sex,    Dev, Qa, Total
             *  Male      5,  2,     7
             *  Female    0,  2,     2
             *  Total,    5,  4,     9
             */
        }

        [Test]
        public void should_calculate_totals()
        {
            Assert.That(this.report.Totals, Is.EqualTo(new object[]
                {"Total", 5L, 4L, 9L}
            ));
        }

        [Test]
        public void should_place_all_rows_with_zero_for_missing_data()
        {
            //                                  Row name, Dev,  QA,Total
            AssertExactlyOneRowInData(nameof(  Sex.Male),  5L,  2L,   7L);
            AssertExactlyOneRowInData(nameof(Sex.Female),  0L,  2L,   2L);
        }

        [Test]
        public void should_place_headers_in_answers_order()
        {
            Assert.That(this.report.Columns[0], Is.EqualTo("variable"));

            Assert.That(this.report.Columns[1], Is.EqualTo(PositionQuestion.Answers[0].AsColumnName()));
            Assert.That(this.report.Columns[2], Is.EqualTo(PositionQuestion.Answers[1].AsColumnName()));
        }
        
        private void AssertExactlyOneRowInData(params object[] row)
        {
            // Assert.That(this.report.Data, Has.One.EqualTo(row)); doesn't work as expected
            var match = new List<Object[]>();

            foreach (var dataRow in this.report.Data)
            {
                if(dataRow.SequenceEqual(row))
                    match.Add(dataRow);
            }

            Assert.That(match, Has.Exactly(1).Items);
        }
        
        private GetReportCategoricalPivotReportItem NewItem(int row, int column, long count)
            => Create.Entity.GetReportCategoricalPivotReportItem(row, column, count);

        private static IQuestion PrepareQuestion(string name, params (int id, string text)[] answers)
        {
            return Mock.Of<IQuestion>(q => 
                q.StataExportCaption == name &&
                q.Answers == answers
                    .Select(a => new Answer
                        {
                            AnswerText = a.text, AnswerCode = a.id
                        }
                    )
                    .ToList()
            );
        }
    }
}
