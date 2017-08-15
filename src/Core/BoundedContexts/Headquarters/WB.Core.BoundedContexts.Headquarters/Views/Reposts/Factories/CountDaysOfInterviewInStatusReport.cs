using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories
{
    public class CountDaysOfInterviewInStatusReport : ICountDaysOfInterviewInStatusReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatusesStorage;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;

        public CountDaysOfInterviewInStatusReport(IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatusesStorage,
            IPlainStorageAccessor<Assignment> assignmentsStorage)
        {
            this.interviewStatusesStorage = interviewStatusesStorage;
            this.assignmentsStorage = assignmentsStorage;
        }

        class CounterObject
        {
            public int InterviewsCount { get; set; }
            public InterviewExportedAction Status { get; set; }
            public DateTime StatusDate { get; set; }
        }

        public CountDaysOfInterviewInStatusRow[] Load(CountDaysOfInterviewInStatusInputModel input)
        {
            var datesAndStatuses = this.interviewStatusesStorage.Query(_ =>
            {
                var filteredInterviews = _;

                if (input.TemplateId.HasValue && input.TemplateVersion.HasValue)
                {
                    filteredInterviews = filteredInterviews.Where(x
                        => x.QuestionnaireId == input.TemplateId.Value
                           && x.QuestionnaireVersion == input.TemplateVersion.Value);
                }

                var statuses = filteredInterviews.SelectMany(x => x.InterviewCommentedStatuses);
                var statusWithTime = (from f in statuses
                    group f by new { f.Status, f.Timestamp.Date } into g
                    select new CounterObject
                    {
                        Status = g.Key.Status,
                        StatusDate = g.Key.Date,
                        InterviewsCount = g.Count(),
                    });

                return statusWithTime;
            });

            var assignmentsDatesAndCounts = assignmentsStorage.Query(_ =>
            {
                var filteredAssignments = _;

                if (input.TemplateId.HasValue && input.TemplateVersion.HasValue)
                {
                    filteredAssignments = filteredAssignments.Where(x
                        => x.QuestionnaireId.QuestionnaireId == input.TemplateId.Value
                           && x.QuestionnaireId.Version == input.TemplateVersion.Value);
                }

                var statusWithTime = (from f in filteredAssignments
                    group f by new { f.CreatedAtUtc } into g
                    select new CounterObject
                    {
                        StatusDate = g.Key.CreatedAtUtc,
                        InterviewsCount = g.Count(),
                    }
                );

                return statusWithTime;
            });

            Dictionary<DateTime, Dictionary<InterviewExportedAction, int>> dictStatistics = new Dictionary<DateTime, Dictionary<InterviewExportedAction, int>>();

            foreach (var counterObject in datesAndStatuses.AsQueryable())
            {
                if (counterObject.Status == InterviewExportedAction.InterviewerAssigned)
                    continue; // ignore, this statistics will be counted in assignments

                if (!dictStatistics.ContainsKey(counterObject.StatusDate))
                    dictStatistics[counterObject.StatusDate] = new Dictionary<InterviewExportedAction, int>();

                dictStatistics[counterObject.StatusDate][counterObject.Status] = counterObject.InterviewsCount;
            }

            foreach (var counterObject in assignmentsDatesAndCounts.AsQueryable())
            {
                if (!dictStatistics.ContainsKey(counterObject.StatusDate))
                    dictStatistics[counterObject.StatusDate] = new Dictionary<InterviewExportedAction, int>();

                dictStatistics[counterObject.StatusDate][InterviewExportedAction.InterviewerAssigned] = counterObject.InterviewsCount;
            }

            var utcNow = DateTime.UtcNow;

            var statisticsRows = dictStatistics.ToList();
            var rows = new CountDaysOfInterviewInStatusRow[statisticsRows.Count];

            for (int i = 0; i < statisticsRows.Count; i++)
            {
                var statisticsRow = statisticsRows[i];
                int daysCount = (utcNow - statisticsRow.Key).Days;
                rows[i] = new CountDaysOfInterviewInStatusRow()
                    {
                        DaysCount                 = daysCount,
                        Date                      = statisticsRow.Key,
                        InterviewerAssignedCount  = GetStatusValue(statisticsRow.Value, InterviewExportedAction.InterviewerAssigned),
                        CompletedCount            = GetStatusValue(statisticsRow.Value, InterviewExportedAction.Completed),
                        ApprovedBySupervisorCount = GetStatusValue(statisticsRow.Value, InterviewExportedAction.ApprovedBySupervisor),
                        RejectedBySupervisorCount = GetStatusValue(statisticsRow.Value, InterviewExportedAction.RejectedBySupervisor),
                    };
            }

            var ranges = new List<int?> { 0, 1, 2, 3, 4, 5, 10, 15, 20, 30 };
            var defaultGroups =
                from row in rows
                group row by ranges.LastOrDefault(range => row.DaysCount >= range) into g
                where g.Key.HasValue
                select g;

            var result = new List<CountDaysOfInterviewInStatusRow>();

            foreach (var defaultGroup in defaultGroups)
            {
                result.Add(new CountDaysOfInterviewInStatusRow()
                {
                    DaysCount = defaultGroup.Key.Value,
                    InterviewerAssignedCount = defaultGroup.Sum(e => e.InterviewerAssignedCount),
                    CompletedCount = defaultGroup.Sum(e => e.CompletedCount),
                    ApprovedBySupervisorCount = defaultGroup.Sum(e => e.ApprovedBySupervisorCount),
                    RejectedBySupervisorCount = defaultGroup.Sum(e => e.RejectedBySupervisorCount),
                });
            }

            var addEmptyRowIfDontExistsData = new Action<int>(days =>
                {
                    if (result.FirstOrDefault(r => r.DaysCount == days) == null)
                        result.Add(new CountDaysOfInterviewInStatusRow() {DaysCount = days });
                });

            addEmptyRowIfDontExistsData(1);
            addEmptyRowIfDontExistsData(2);
            addEmptyRowIfDontExistsData(3);
            addEmptyRowIfDontExistsData(4);
            addEmptyRowIfDontExistsData(5);

            return result.OrderBy(r => r.DaysCount).ToArray();
        }

        private int GetStatusValue(Dictionary<InterviewExportedAction, int> dictionary, InterviewExportedAction status)
        {
            if (dictionary.TryGetValue(status, out int value))
                return value;
                
            return 0;
        }
    }
}