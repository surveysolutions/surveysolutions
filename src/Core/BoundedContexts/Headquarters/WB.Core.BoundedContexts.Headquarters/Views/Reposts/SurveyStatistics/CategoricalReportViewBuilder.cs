using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class CategoricalReportViewBuilder
    {
        // map answer value into its position in answers list of question
        private readonly Dictionary<int, int> answersIndexMap;

        private readonly IEnumerable<string> columnsWithData;
        private readonly IEnumerable<string> headersWithData;

        private readonly GetCategoricalReportItem[] rows;

        /// <summary>
        /// Convert into table result of get_categorical_report function
        /// Input: Array of (TeamLeadName, ResponsibleName, Answer, CountOfInterviewsWithThisAnswer)
        /// where Answer is answer code for question
        /// (TeamLead, ReponsibleName, Answer1, Answer2, ..., Total) 
        /// where values are count of interviews with this answer
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="rows"></param>
        public CategoricalReportViewBuilder(List<Answer> answers, IEnumerable<GetCategoricalReportItem> rows)
        {
            answersIndexMap = GetAnswersMap(answers);

            headersWithData = answers.Select(a => a.AnswerText);
            columnsWithData = answers.Select(a => a.AsColumnName());

            this.rows = rows?.ToArray() ?? Array.Empty<GetCategoricalReportItem>();
        }

        public ReportView AsReportView()
        {
            var report = new ReportView
            {
                Columns = GetColumnData().Select(c => c.column).ToArray(),
                Headers = GetColumnData().Select(c => c.header).ToArray()
            };

            report.Totals = new object[report.Columns.Length];
            var dataIndex = 0;

            SetRowValue(report.Totals, dataIndex++, Strings.AllTeams);
            SetRowValue(report.Totals, dataIndex++, Strings.AllInterviewers);

            report.Totals.Clear(0L, dataIndex);

            var reportData = new Dictionary<(string, string), object[]>();
            var lastColumnIndex = report.Totals.Length - 1;

            foreach (var row in rows)
            {
                var answerIndex = answersIndexMap[row.Answer] + dataIndex;

                if (row.TeamLeadName == null && row.ResponsibleName == null) // handle total row
                {
                    SetRowValue(report.Totals, answerIndex,     row.Count);
                    SetRowValue(report.Totals, lastColumnIndex, row.Count);
                    continue;
                }

                var rowKey = (row.TeamLeadName, row.ResponsibleName);
                
                if (!reportData.TryGetValue(rowKey, out var existingRow))
                {
                    existingRow = new object[report.Columns.Length];
                    var rowIndex = 0;
                    SetRowValue(existingRow, rowIndex++, row.TeamLeadName);
                    SetRowValue(existingRow, rowIndex++, row.ResponsibleName);

                    existingRow.Clear(0L, rowIndex);

                    reportData.Add(rowKey, existingRow);
                }

                SetRowValue(existingRow, answerIndex,     row.Count);
                SetRowValue(existingRow, lastColumnIndex, row.Count);
            }

            report.Data = reportData.Values.ToArray();

            return report;
        }

        private Dictionary<int, int> GetAnswersMap(List<Answer> answers)
        {
            var result = new Dictionary<int, int>();

            for (var index = 0; index < answers.Count; index++)
            {
                var answer = answers[index];
                result.Add((int) answer.GetParsedValue(), index);
            }

            return result;
        }

        private IEnumerable<(string column, string header)> GetColumnData()
        {
            yield return ("TeamLead", Report.COLUMN_TEAMS);
            yield return ("Responsible", Report.COLUMN_TEAM_MEMBER);

            foreach (var data in columnsWithData.Zip(headersWithData, (col, head) => (col, head))) yield return data;

            yield return ("total", Strings.Total);
        }

        private void SetRowValue(object[] array, int index, long value)
        {
            array[index] = (long) (array[index] ?? 0L) + value;
        }

        private void SetRowValue(object[] array, int index, string value)
        {
            array[index] = value;
        }
    }
}
