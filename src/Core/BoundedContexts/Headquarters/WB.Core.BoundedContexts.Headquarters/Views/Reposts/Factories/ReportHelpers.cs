using System;
using System.Linq;
using NHibernate.Hql.Ast.ANTLR.Tree;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

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
            QuestionnaireIdentity identity,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewstatusStorage)
        {
            var localTo = from.Date.AddDays(1);
            var localFrom = AddPeriod(localTo, period, -columnCount);

            var utcFrom = localFrom.AddMinutes(timezoneAdjastmentMins);
            var utcTo = localTo.AddMinutes(timezoneAdjastmentMins);

            DateTime? utcMinDate = GetFirstInterviewCreatedDate(identity, interviewstatusStorage);
            DateTime? localMinDate = utcMinDate?.AddMinutes(-timezoneAdjastmentMins);

            DateTimeRange[] dateTimeRangesLocal =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(localFrom, period, i), AddPeriod(localFrom, period, i + 1)))
                    .Where(i => localMinDate.HasValue && i.To >= localMinDate)
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

        private static DateTime? GetFirstInterviewCreatedDate(QuestionnaireIdentity questionnaire, IQueryableReadSideRepositoryReader<InterviewSummary> interviewstatusStorage)
        {
            DateTime? minDate;
            if (questionnaire != null && questionnaire.QuestionnaireId != Guid.Empty)
            {
                minDate = interviewstatusStorage.Query(_ => _
                    .Where(x => x.QuestionnaireId == questionnaire.QuestionnaireId &&
                                x.QuestionnaireVersion == questionnaire.Version)
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Select(x => (DateTime?)x.Timestamp)
                    .OrderBy(x => x))
                    .Take(1)
                    .FirstOrDefault();
            }

            else
            {
                minDate = interviewstatusStorage.Query(_ => _
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Select(x => (DateTime?)x.Timestamp)
                    .OrderBy(x => x))
                    .Take(1)
                    .FirstOrDefault();
            }
            return minDate;
        }

    }
}