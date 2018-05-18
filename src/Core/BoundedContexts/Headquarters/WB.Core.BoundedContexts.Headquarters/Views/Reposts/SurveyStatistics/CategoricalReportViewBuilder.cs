using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.SurveyStatistics
{
    public class CategoricalReportViewBuilder
    {
        private readonly Dictionary<int, int> answersIndexMap;
        public const string TeamLeadColumn = "TeamLead";
        public const string ResponsibleColumn = "Responsible";

        public List<CategoricalReportViewItem> Data { get; set; } = new List<CategoricalReportViewItem>();
        public string[] Headers { get; }
        public string[] Columns { get; }
        public long[] Totals { get; set; }
        private bool TeamLeadOnly { get; }

        public int DataStartAtIndex => TeamLeadOnly ? 1 : 2;

        public CategoricalReportViewBuilder(List<Answer> answers, IEnumerable<GetCategoricalReportItem> rows)
        {
            this.answersIndexMap = new Dictionary<int, int>();
            var data = (rows ?? new List<GetCategoricalReportItem>()).ToList();
            this.TeamLeadOnly = data.All(d => string.IsNullOrWhiteSpace(d.ResponsibleName));

            for (var index = 0; index < answers.Count; index++)
            {
                var answer = answers[index];
                answersIndexMap.Add((int) answer.GetParsedValue(), index);
            }

            Headers = (this.TeamLeadOnly ? new[] { Report.COLUMN_TEAMS } : new[] { Report.COLUMN_TEAMS, Report.COLUMN_TEAM_MEMBER })
                .Concat(answers.Select(a => a.AnswerText))
                .Concat(new[] {Strings.Total})
                .ToArray();

            Columns = (this.TeamLeadOnly ? new[] { TeamLeadColumn} : new[] { TeamLeadColumn, ResponsibleColumn })
                .Concat(answers.Select(a => a.AsColumnName()))
                .Concat(new[] {"total"})
                .ToArray();

            SetData(data);
        }

        private void SetData(List<GetCategoricalReportItem> rows)
        {
            this.Data.Clear();
            this.Totals = new long[answersIndexMap.Count];
            
            CategoricalReportViewItem perTeamReportItem = null;

            foreach (var row in rows)
            {
                // check if this is total row related data
                if (string.IsNullOrWhiteSpace(row.TeamLeadName) && string.IsNullOrWhiteSpace(row.ResponsibleName))
                {
                    Totals[answersIndexMap[row.Answer]] = row.Count;

                    continue;
                }

                if (perTeamReportItem != null
                    && (perTeamReportItem.ResponsibleName != row.ResponsibleName
                        || perTeamReportItem.TeamLeadName != row.TeamLeadName))
                {
                    this.Data.Add(perTeamReportItem);
                    perTeamReportItem = null;
                }

                if (perTeamReportItem == null)
                {
                    perTeamReportItem = new CategoricalReportViewItem
                    {
                        ResponsibleName = row.ResponsibleName,
                        TeamLeadName = row.TeamLeadName,
                        Values = new long[answersIndexMap.Count]
                    };
                }

                perTeamReportItem.Values[answersIndexMap[row.Answer]] = row.Count;
            }

            if (perTeamReportItem != null) this.Data.Add(perTeamReportItem);
        }

        public ReportView AsReportView()
        {
            var report = new ReportView
            {
                Columns = Columns,
                Headers = Headers,
                Totals = new object[Columns.Length],
                Data = new object[Data.Count][]
            };

            report.Totals[0] = Strings.AllTeams;

            if (!TeamLeadOnly)
                report.Totals[1] = Strings.AllInterviewers;

            int dataIndexStart = DataStartAtIndex;

            Array.Copy(Totals, 0, report.Totals, dataIndexStart, Totals.Length);

            report.Totals[Columns.Length - 1] = Totals.Sum();

            for (var index = 0; index < Data.Count; index++)
            {
                var item = Data[index];

                var row = new object[report.Columns.Length];
                row[0] = item.TeamLeadName;

                if(!TeamLeadOnly)
                    row[1] = item.ResponsibleName;

                var results = item.Values;

                Array.Copy(results, 0, row, dataIndexStart, results.Length);

                row[report.Columns.Length - 1] = item.Total;
                report.Data[index] = row;
            }

            return report;
        }
    }
}
