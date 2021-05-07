using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class CategoricalPivotReportViewBuilder
    {
        private readonly IQuestionnaire questionnaire;
        private readonly Guid columnQuestionId;
        private readonly Guid rowsQuestionId;
        private readonly List<GetReportCategoricalPivotReportItem> items;

        public CategoricalPivotReportViewBuilder(IQuestionnaire questionnaire, Guid columnQuestionId, Guid rowsQuestionId, List<GetReportCategoricalPivotReportItem> items)
        {
            this.questionnaire = questionnaire;
            this.columnQuestionId = columnQuestionId;
            this.rowsQuestionId = rowsQuestionId;
            this.items = items ?? new List<GetReportCategoricalPivotReportItem>();
        }

        public ReportView AsReportView()
        {
            var columnOptions = questionnaire.GetOptionsForQuestion(columnQuestionId, null, null, null).ToList();
            var rowsOptions = questionnaire.GetOptionsForQuestion(rowsQuestionId, null, null, null).ToList();

            var columnAnswers = GetAnswersIndex(columnOptions);
            var rowsAnswers = GetAnswersIndex(rowsOptions);

            var report = new ReportView();

            IEnumerable<string> GetColumns()
            {
                yield return "variable";

                foreach (var answer in columnOptions)
                {
                    yield return answer.AsColumnName();
                }

                yield return "total";
            }
            
            IEnumerable<string> GetHeaders()
            {
                yield return questionnaire.GetQuestionExportDescription(rowsQuestionId) ?? questionnaire.GetQuestionVariableName(rowsQuestionId);

                foreach (var answer in columnOptions)
                {
                    yield return answer.Title;
                }

                yield return Strings.Total;
            }

            report.Columns = GetColumns().ToArray();
            report.Headers = GetHeaders().ToArray();
            report.Totals = new object[report.Columns.Length];
            report.Totals[0] = Strings.Total;

            for (int i = 1; i < report.Totals.Length; i++)
            {
                report.Totals[i] = 0L;
            }

            report.Data = new object[rowsOptions.Count][];

            foreach (var kv in rowsAnswers)
            {
                var rowIndex = kv.Value;
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
                var rowIndex = rowsAnswers[item.RowValue];
                var row = report.Data[rowIndex.index];

                if (!columnAnswers.ContainsKey(item.ColValue))
                {
                    report.Warnings.Add(ReportWarnings.AnswerIgnored);
                    continue;
                }

                var columnIndex = columnAnswers[item.ColValue];
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

        private SortedDictionary<long, (int index, string text)> GetAnswersIndex(List<CategoricalOption> options)
        {
            var rowIndexMap = new SortedDictionary<long, (int index, string text)>();

            for (var index = 0; index < options.Count; index++)
            {
                var answer = options[index];
                rowIndexMap.Add((long) answer.Value, (index, answer.Title));
            }

            return rowIndexMap;
        }
    }
}
