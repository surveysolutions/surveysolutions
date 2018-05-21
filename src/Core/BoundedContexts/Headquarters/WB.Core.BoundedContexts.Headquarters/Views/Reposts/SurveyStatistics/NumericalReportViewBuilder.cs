using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics.Data;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class NumericalReportViewBuilder
    {
        private readonly List<GetNumericalReportItem> numericalData;

        public NumericalReportViewBuilder(List<GetNumericalReportItem> numericalData)
        {
            this.numericalData = numericalData ?? new List<GetNumericalReportItem>();
        }

        public ReportView AsReportView()
        {
            var report = new ReportView
            {
                Columns = ColumnData().Select(d => d.column).ToArray(),
                Headers = ColumnData().Select(d => d.header).ToArray()
            };

            report.Totals = new object[report.Columns.Length];

            var resultData = new List<object[]>();

            foreach (var reportItem in numericalData)
            {
                if (reportItem.TeamLeadName == null && reportItem.ResponsibleName == null)
                {
                    report.Totals = CreateRow(reportItem, Strings.AllTeams, Strings.AllInterviewers);
                    continue;
                }

                var row = CreateRow(reportItem, reportItem.TeamLeadName, reportItem.ResponsibleName);

                resultData.Add(row);
            }

            report.Data = resultData.ToArray();
            return report;

            object[] CreateRow(GetNumericalReportItem numeric, params string[] rowPrefix)
            {
                var row = new object[report.Columns.Length];
                var rowIndex = 0;

                foreach (var prefix in rowPrefix)
                {
                    AppendToRow(prefix);
                }

                AppendToRow(numeric.Count);
                AppendToRow(numeric.Average);
                AppendToRow(numeric.Median);
                AppendToRow(numeric.Sum);
                AppendToRow(numeric.Min);
                AppendToRow(numeric.Percentile05);
                AppendToRow(numeric.Percentile50);
                AppendToRow(numeric.Percentile95);
                AppendToRow(numeric.Max);

                return row;

                void AppendToRow(object value)
                {
                    row[rowIndex++] = value;
                }
            }
        }

        private IEnumerable<(string column, string header)> ColumnData()
        {
            yield return ("TeamLead", Report.COLUMN_TEAMS);
            yield return ("Responsible", Report.COLUMN_TEAM_MEMBER);

            yield return ("count", "Count");
            yield return ("average", "Average");
            yield return ("median", "Median");
            yield return ("sum", "Sum");
            yield return ("min", "Min");
            yield return ("percentile_05", "Percentile 05");
            yield return ("percentile_50", "Percentile 50");
            yield return ("percentile_95", "Percentile 95");
            yield return ("max", "Max");
        }

        public ReportView Merge(ReportView specialValuesReport)
        {
            IEnumerable<string> IndexRows()
            {
                yield return "TeamLead";
                yield return "Responsible";
            }

            var index = IndexRows().ToArray();
            return AsReportView().Merge(specialValuesReport, index);
        }
    }
}
