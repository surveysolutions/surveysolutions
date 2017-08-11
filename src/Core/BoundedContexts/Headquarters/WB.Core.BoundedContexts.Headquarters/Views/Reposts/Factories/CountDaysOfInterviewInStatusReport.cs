using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;


namespace WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories
{
    public class CountDaysOfInterviewInStatusReport : ICountDaysOfInterviewInStatusReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatusesStorage;

        public CountDaysOfInterviewInStatusReport(IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatusesStorage)
        {
            this.interviewStatusesStorage = interviewStatusesStorage;
        }

        class CounterObject
        {
            public int InterviewsCount { get; set; }
            public InterviewExportedAction Status { get; set; }
            public DateTime StatusDate { get; set; }
        }

        public CountDaysOfInterviewInStatusView Load(CountDaysOfInterviewInStatusInputModel input)
        {
            var datesAndStatuses = this.interviewStatusesStorage.Query(_ =>
            {
                var filteredInterviews = _;

                if (input.QuestionnaireId.HasValue && input.QuestionnaireVersion.HasValue)
                {
                    filteredInterviews = filteredInterviews.Where(x
                        => x.QuestionnaireId == input.QuestionnaireId.Value
                           && x.QuestionnaireVersion == input.QuestionnaireVersion.Value);
                }

                var statuses = filteredInterviews.SelectMany(x => x.InterviewCommentedStatuses);

//                var test = (from f in statuses
//                    group f by new { f.Status, f.Timestamp.Date } into g
//                    select new CounterObject
//                    {
//                        Status = g.Key.Status,
//                        StatusDate = g.Key.Date,
//                        InterviewsCount = g.Count(),
//                    }
//                ).ToList();
//
//                var statusWithTime2 = ( filteredInterviews//.SelectMany(x => x.InterviewCommentedStatuses)
//                    //.Select(f => f.InterviewCommentedStatuses.Last())
//                    .Select(f => f.InterviewCommentedStatuses.OrderByDescending(x => x.Timestamp).FirstOrDefault())
//                    ).ToList();

                var statusWithTime = (from f in statuses//.SelectMany(x => x.InterviewCommentedStatuses)
                                                        //.Select(f => f.InterviewCommentedStatuses.Last())
//                    .Select(f => f.InterviewCommentedStatuses.OrderByDescending(x => x.Timestamp).FirstOrDefault())
                    group f by new { f.Status, f.Timestamp.Date } into g
                    select new CounterObject
                    {
                        Status = g.Key.Status,
                        StatusDate = g.Key.Date,
                        InterviewsCount = g.Count(),
                    }
                    ).ToList();

                return statusWithTime;
            });


            Dictionary<DateTime, Dictionary<InterviewExportedAction, int>> dictStatistics = new Dictionary<DateTime, Dictionary<InterviewExportedAction, int>>();

            foreach (var counterObject in datesAndStatuses.AsQueryable())
            {
                if (!dictStatistics.ContainsKey(counterObject.StatusDate))
                    dictStatistics[counterObject.StatusDate] = new Dictionary<InterviewExportedAction, int>();

                dictStatistics[counterObject.StatusDate][counterObject.Status] = counterObject.InterviewsCount;
            }


            var utcNow = DateTime.UtcNow;
            var statistics = new CountDaysOfInterviewInStatusView();

            var statisticsRows = dictStatistics.OrderBy(s => s.Key).ToList();
            statistics.Rows = new CountDaysOfInterviewInStatusRow[statisticsRows.Count];

            for (int i = 0; i < statisticsRows.Count; i++)
            {
                var statisticsRow = statisticsRows[i];
                int daysCount = (utcNow - statisticsRow.Key).Days;
                statistics.Rows[i] = new CountDaysOfInterviewInStatusRow()
                    {
                        DaysCount = daysCount,
                        Date = statisticsRow.Key,
                        Cells = GetStatusValuesInCorrectOrder(statisticsRow.Value, input.Statuses)
                    };
            }
            
            return statistics;
        }

        private int[] GetStatusValuesInCorrectOrder(Dictionary<InterviewExportedAction, int> dictionary, InterviewExportedAction[] statuses)
        {
            return statuses.Select(status =>
            {
                if (dictionary.TryGetValue(status, out int value))
                    return value;
                
                return 0;
            })
            .ToArray();
        }
    }
}