using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class NumericalReportViewBuilder
    {
        private readonly List<GetNumericalReportItem> numericalData;
        private readonly CategoricalReportViewBuilder specialValuesData;

        public const string TeamLeadColumn = "TeamLead";
        public const string ResponsibleColumn = "Responsible";
        public const string AllTeams = "AllTeams";
        public const string AllResponsible = "AllResponsible";
        
        public NumericalReportViewBuilder(List<GetNumericalReportItem> numericalData, CategoricalReportViewBuilder specialValuesData)
        {
            this.numericalData = numericalData;
            this.specialValuesData = specialValuesData;
        }

        private static readonly string[] NumericColumns =
        {
            TeamLeadColumn, ResponsibleColumn,
            "count", "average", "median", "sum", "min", "percentile_05", "percentile_50", "percentile_95", "max"
        };

        private static readonly string[] NumericHeaders =
        {
            TeamLeadColumn, ResponsibleColumn,
            "Count", "Average", "Median", "Sum", "Min", "Percentile05", "Percentile50", "Percentile95", "Max"
        };

        public ReportView AsReportView()
        {
            var report = new ReportView
            {
                // skip 2 - is TeamLead and Responsible columns
                Columns = NumericColumns.Union(this.specialValuesData.Columns.Skip(2)).ToArray(),
                Headers=  NumericHeaders.Union(this.specialValuesData.Headers.Skip(2)).ToArray()
            };

            report.Totals = new object[report.Columns.Length];

            Dictionary<(string teamLeadName, string responsibleName), CategoricalReportViewItem> categoricalData 
                = this.specialValuesData.Data.ToDictionary(d => (d.TeamLeadName, d.ResponsibleName));

            var data = new List<object[]>();

            object[] CreateRow(GetNumericalReportItem numeric, (string teamLeadName, string responsible) categoricalKey, params string[] rowPrefix)
            {
                var row = new object[report.Columns.Length];
                int rowIndex = 0;
                void AppendToRow(object value) => row[rowIndex++] = value;

                foreach (var column in rowPrefix)
                {
                    AppendToRow(column);
                }

                if (numeric != null)
                {
                    var numericRowData = new object[]
                    {
                        numeric.Count, numeric.Average, numeric.Median, numeric.Sum,
                        numeric.Min, numeric.Percentile05, numeric.Percentile50, numeric.Percentile95, numeric.Max
                    };

                    foreach (var value in numericRowData)
                    {
                        AppendToRow(value);
                    }
                }
                else
                {
                    for (int i = 0; i < NumericColumns.Length - 2; i++)
                    {
                        AppendToRow(null);
                    }
                }

                if (categoricalData.TryGetValue(categoricalKey, out var categorical))
                {
                    foreach (var value in categorical.Values)
                    {
                        AppendToRow(value);
                    }

                    categoricalData.Remove(categoricalKey);
                }
                else
                {
                    if (categoricalKey.teamLeadName == null && categoricalKey.responsible == null)
                    {
                        foreach (var total in specialValuesData.Totals)
                        {
                            AppendToRow(total);
                        }
                    }
                    else
                    {
                        for (int i = 2; i < specialValuesData.Columns.Length; i++)
                        {
                            AppendToRow(0);
                        }
                    }
                }

                return row;
            }

            foreach (GetNumericalReportItem numeric in this.numericalData)
            {
                if (numeric.TeamLeadName == null && numeric.ResponsibleName == null)
                {
                    report.Totals = CreateRow(numeric, (null, null), AllTeams, AllResponsible);
                    continue;
                }

                var row = CreateRow(numeric, (numeric.TeamLeadName, numeric.ResponsibleName), numeric.TeamLeadName, numeric.ResponsibleName);
                data.Add(row);
            }

            // for those cases when there is only Special Values answers
            foreach (var categoricalItem in categoricalData.Values.ToList())
            {
                if(!categoricalData.ContainsKey((categoricalItem.TeamLeadName, categoricalItem.ResponsibleName)))
                    continue;
                
                if (categoricalItem.TeamLeadName == null && categoricalItem.ResponsibleName == null)
                {
                    report.Totals = CreateRow(null, (null, null), AllTeams, AllResponsible);
                    continue;
                }
                
                var row = CreateRow(null, (categoricalItem.TeamLeadName, categoricalItem.ResponsibleName), 
                    categoricalItem.TeamLeadName, categoricalItem.ResponsibleName);

                data.Add(row);
            }

            report.Data = data.ToArray();
            return report;
        }
    }
}
