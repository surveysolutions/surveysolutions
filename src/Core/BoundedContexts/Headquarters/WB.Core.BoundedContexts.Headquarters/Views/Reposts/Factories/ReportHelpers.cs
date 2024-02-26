using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class ReportTimeline
    {
        public DateTime ToLocal { get; set; }
        public DateTime ToUtc { get; set; }

        public DateTime FromLocal { get; set; }
        public DateTime FromUtc { get; set; }

        public DateTimeRange[] ColumnRangesLocal { get; set; }

        public DateTimeRange[] ColumnRangesUtc { get; set; }
    }

    public class ReportHelpers
    {
        public static ReportTimeline BuildColumns(DateTime from, 
            string period,
            int columnCount,
            int timezoneAdjastmentMins,
            Guid? questionnaireId,
            long? questionnaireVersion,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewstatusStorage)
        {
            var localTo = from.Date.AddDays(1);
            var localFrom = AddPeriod(localTo, period, -columnCount);

            var utcFrom = localFrom.AddMinutes(timezoneAdjastmentMins).SetUtcKind();
            var utcTo = localTo.AddMinutes(timezoneAdjastmentMins).SetUtcKind();

            DateTime? utcMinDate = GetFirstInterviewCreatedDate(questionnaireId, questionnaireVersion, interviewstatusStorage);
            DateTime? localMinDate = utcMinDate?.AddMinutes(-timezoneAdjastmentMins).SetKind(DateTimeKind.Unspecified);

            DateTimeRange[] dateTimeRangesLocal =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(localFrom, period, i), AddPeriod(localFrom, period, i + 1)))
                    .Where(i => localMinDate.HasValue && i.To >= localMinDate)
                    .Select(i => new DateTimeRange(i.From.AddDays(-1), i.To.AddDays(-1)))
                    .ToArray();

            DateTimeRange[] dateTimeRangesUtc =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(utcFrom, period, i), AddPeriod(utcFrom, period, i + 1)))
                    .Where(i => utcMinDate.HasValue && i.To >= utcMinDate)
                    .ToArray();

            return new ReportTimeline
            {
                FromLocal = localFrom,
                FromUtc = utcFrom,
                ToLocal = localTo,
                ToUtc = utcTo,
                ColumnRangesLocal = dateTimeRangesLocal,
                ColumnRangesUtc = dateTimeRangesUtc
            };
        }

        private static DateTime AddPeriod(DateTime d, string period, int value)
        {
            switch (period)
            {
                case "d":
                    return d.AddDays(value);
                case "w":
                    return d.AddDays(value * 7);
                case "m":
                    return d.AddMonths(value);
            }
            throw new ArgumentException($"period '{period}' can't be recognized");
        }

        private static DateTime? GetFirstInterviewCreatedDate(Guid? questionnaireId,
            long? questionnaireVersion, IQueryableReadSideRepositoryReader<InterviewSummary> interviewstatusStorage)
        {
            DateTime? minDate = interviewstatusStorage.Query(_ => {
                    var query = _;

                    if (questionnaireId.HasValue)
                    {
                        query = query.Where(x => x.QuestionnaireId == questionnaireId);
                    }

                    if (questionnaireVersion.HasValue)
                    {
                        query = query.Where(x => x.QuestionnaireVersion == questionnaireVersion);
                    }

                    return query
                            .SelectMany(x => x.InterviewCommentedStatuses)
                        .Select(x => (DateTime?) x.Timestamp)
                        .OrderBy(x => x);
                })
                .Take(1)
                .FirstOrDefault();

            return minDate;
        }
    }
}
