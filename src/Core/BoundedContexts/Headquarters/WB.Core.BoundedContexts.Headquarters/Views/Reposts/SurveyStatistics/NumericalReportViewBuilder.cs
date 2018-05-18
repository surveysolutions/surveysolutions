using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class NumericalReportViewBuilder
    {
        private readonly List<GetNumericalReportItem> numericalData;
        private readonly CategoricalReportViewBuilder specialValuesData;

        public const string TeamLeadColumn = "TeamLead";
        public const string ResponsibleColumn = "Responsible";
        
        public NumericalReportViewBuilder(List<GetNumericalReportItem> numericalData, CategoricalReportViewBuilder specialValuesData)
        {
            this.numericalData = numericalData ?? new List<GetNumericalReportItem>();
            this.specialValuesData = specialValuesData;
        }

        private static readonly string[] NumericColumns =
        {
            TeamLeadColumn, ResponsibleColumn,
            "count", "average", "median", "sum", "min", "percentile_05", "percentile_50", "percentile_95", "max"
        };

        private static readonly string[] NumericHeaders =
        {
            Report.COLUMN_TEAMS, Report.COLUMN_TEAM_MEMBER,
            "Count", "Average", "Median", "Sum", "Min", "Percentile05", "Percentile50", "Percentile95", "Max"
        };

        public ReportView AsReportView()
        {
            var report = new ReportView
            {
                Columns = NumericColumns,
                Headers = NumericHeaders
            };

            var hasSpecialValues = this.specialValuesData.Columns.Length > 3;
            if (hasSpecialValues) // has any data
            {
                // skip 2 - is TeamLead and Responsible columns
                report.Columns = report.Columns.Concat(this.specialValuesData.Columns.Skip(this.specialValuesData.DataStartAtIndex)).ToArray();
                report.Headers = report.Headers.Concat(this.specialValuesData.Headers.Skip(this.specialValuesData.DataStartAtIndex)).ToArray();
            }

            report.Totals = new object[report.Columns.Length];

            Dictionary<(string teamLeadName, string responsibleName), CategoricalReportViewItem> categoricalData
                = this.specialValuesData.Data.ToDictionary(d => (d.TeamLeadName, d.ResponsibleName));                

            var resultData = new List<object[]>();

            foreach (GetNumericalReportItem reportItem in this.numericalData)
            {
                if (reportItem.TeamLeadName == null && reportItem.ResponsibleName == null)
                {
                    report.Totals = CreateRow(reportItem, (null, null), Strings.AllTeams, Strings.AllInterviewers);
                    continue;
                }

                var row = CreateRow(reportItem, (reportItem.TeamLeadName, reportItem.ResponsibleName), reportItem.TeamLeadName, reportItem.ResponsibleName);
                resultData.Add(row);
            }

            // for those cases when there is only Special Values answers
            foreach (var categoricalItem in categoricalData.Values.ToList())
            {
                if(!categoricalData.ContainsKey((categoricalItem.TeamLeadName, categoricalItem.ResponsibleName)))
                    continue;
                
                if (categoricalItem.TeamLeadName == null && categoricalItem.ResponsibleName == null)
                {
                    report.Totals = CreateRow(null, (null, null), Strings.AllTeams, Strings.AllInterviewers);
                    continue;
                }
                
                var row = CreateRow(null, (categoricalItem.TeamLeadName, categoricalItem.ResponsibleName), 
                    categoricalItem.TeamLeadName, categoricalItem.ResponsibleName);

                resultData.Add(row);
            }

            report.Data = resultData.ToArray();
            return report;
            
            object[] CreateRow(GetNumericalReportItem numeric, (string teamLeadName, string responsible) categoricalKey, 
                params string[] rowPrefix)
            {
                var row = new object[report.Columns.Length];
                int rowIndex = 0;
                void AppendToRow(object value) => row[rowIndex++] = value;

                // adding row prefixes
                foreach (var column in rowPrefix)
                {
                    AppendToRow(column);
                }
                
                AppendToRow(numeric?.Count);
                AppendToRow(numeric?.Average);
                AppendToRow(numeric?.Median);
                AppendToRow(numeric?.Sum);
                AppendToRow(numeric?.Min);
                AppendToRow(numeric?.Percentile05);
                AppendToRow(numeric?.Percentile50);
                AppendToRow(numeric?.Percentile95);
                AppendToRow(numeric?.Max);

                if (!hasSpecialValues) return row;

                if (categoricalData.TryGetValue(categoricalKey, out var categorical))
                {
                    foreach (var value in categorical.Values)
                    {
                        AppendToRow(value);
                    }

                    AppendToRow(categorical.Total);

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

                        AppendToRow(specialValuesData.Totals.Sum());
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
        }
    }
}
