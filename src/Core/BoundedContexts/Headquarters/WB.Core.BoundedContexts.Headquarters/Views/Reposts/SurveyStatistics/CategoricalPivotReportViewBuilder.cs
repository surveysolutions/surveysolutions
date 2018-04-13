using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class CategoricalPivotReportViewBuilder
    {
        private readonly IQuestion questionA;
        private readonly IQuestion rowsQuestion;
        private readonly List<GetReportCategoricalPivotReportItem> items;

        public CategoricalPivotReportViewBuilder(IQuestion questionA, IQuestion rowsQuestion, List<GetReportCategoricalPivotReportItem> items)
        {
            this.questionA = questionA;
            this.rowsQuestion = rowsQuestion;
            this.items = items;
        }

        public ReportView AsReportView()
        {
            var columnAnswers = GetAnswersIndex(questionA);
            var rowsAnswers = GetAnswersIndex(rowsQuestion);

            var report = new ReportView();

            IEnumerable<string> GetColumns()
            {
                yield return "variable";

                foreach (var answer in questionA.Answers)
                {
                    yield return answer.AsColumnName();
                }

                yield return "total";
            }
            
            IEnumerable<string> GetHeaders()
            {
                yield return rowsQuestion.StataExportCaption;

                foreach (var answer in questionA.Answers)
                {
                    yield return answer.AnswerText;
                }

                yield return Strings.Total;
            }


            report.Columns = GetColumns().ToArray();
            report.Headers = GetHeaders().ToArray();
            report.Totals = new object[report.Columns.Length];
            report.Totals[0] = Strings.Total;
            
            report.Data = new object[rowsQuestion.Answers.Count][];

            foreach (var rowIndex in rowsAnswers.Values)
            {
                report.Data[rowIndex.index] = new object[report.Columns.Length];
                var row = report.Data[rowIndex.index];
                row[0] = rowIndex.text;                     // first column is name of row
                row[report.Columns.Length - 1] = 0L;

                foreach (var colIndex in columnAnswers.Values)
                {
                    row[colIndex.index + 1] = 0L;
                }
            }

            // A - is a columns, B - is rows
            foreach (var item in items)
            {
                var rowIndex = rowsAnswers[item.B];
                var row = report.Data[rowIndex.index];

                var columnIndex = columnAnswers[item.A];
                row[columnIndex.index + 1] = item.Count;

                AddAt(row,           report.Columns.Length - 1, item.Count);
                AddAt(report.Totals, columnIndex.index + 1,     item.Count); // sum per column
                AddAt(report.Totals, report.Columns.Length - 1, item.Count); // overall sum

                void AddAt(object[] array, int index, long value)
                {
                    array[index] = (long)(array[index] ?? 0L) + value;
                }
            }

            return report;
        }

        private Dictionary<int, (int index, string text)> GetAnswersIndex(IQuestion question)
        {
            var rowIndexMap = new Dictionary<int, (int index, string text)>();

            for (var index = 0; index < question.Answers.Count; index++)
            {
                var answer = question.Answers[index];
                rowIndexMap.Add((int) answer.GetParsedValue(), (index, answer.AnswerText));
            }

            return rowIndexMap;
        }
    }
}
