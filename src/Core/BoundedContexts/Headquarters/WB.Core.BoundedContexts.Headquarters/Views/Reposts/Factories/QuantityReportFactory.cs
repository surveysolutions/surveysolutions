using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface IQuantityReportFactory
    {
        QuantityByResponsibleReportView Load(QuantityByInterviewersReportInputModel input);
        QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input);
    }

    public class QuantityReportFactory : IQuantityReportFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewstatusStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage;

        public QuantityReportFactory(
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewstatusStorage,
            IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage)
        {
            this.interviewstatusStorage = interviewstatusStorage;
            this.interviewStatusTimeSpansStorage = interviewStatusTimeSpansStorage;
        }

        private QuantityByResponsibleReportView Load<T>(
            DateTime reportStartDate,
            string period,
            int columnCount,
            int page,
            int pageSize,
            Func<DateTime, DateTime, IQueryable<T>> queryInterviewStatusesByDateRange,
            Expression<Func<T, Guid>> selectUserId,
            Expression<Func<T, UserAndTimestamp>> selectUserAndTimestamp)
        {
            var from = this.AddPeriod(reportStartDate.Date, period, -columnCount + 1);
            var to = reportStartDate.Date; 

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(this.AddPeriod(from, period, i).Date, this.AddPeriod(from, period, i + 1).Date))
                    .Where(i => i.From.Date <= DateTime.Now.Date)
                    .ToArray();

            var interviewStatusesByDateRange = queryInterviewStatusesByDateRange(from, to);

            var userIdsOfAllResponsibleForTheInterviews = interviewStatusesByDateRange
                    .Select(selectUserId)
                    .Distinct();

            var responsibleUsersCount = userIdsOfAllResponsibleForTheInterviews.Count();

            var responsibleUserIdsForOnePage = userIdsOfAllResponsibleForTheInterviews.Skip((page - 1) * pageSize)
                .Take(pageSize).ToArray();

            var interviewStatusChangeDateWithResponsible = queryInterviewStatusesByDateRange(from, to)
                    .Select(selectUserAndTimestamp)
                    .Where(ics => ics.UserId.HasValue && responsibleUserIdsForOnePage.Contains(ics.UserId.Value))
                    .Select(i => new { UserId = i.UserId.Value, i.UserName, i.Timestamp })
                    .ToArray();

            var rows = responsibleUserIdsForOnePage.Select(u =>
            {
                var interviewsForUser = interviewStatusChangeDateWithResponsible.Where(i => i.UserId == u).ToArray();
                var quantityByPeriod = new List<long>();

                foreach (var dateTimeRange in dateTimeRanges)
                {
                    var count =
                        interviewsForUser.Count(
                            ics => ics.Timestamp.Date >= dateTimeRange.From && ics.Timestamp.Date < dateTimeRange.To);
                    quantityByPeriod.Add(count);
                }
                return new QuantityByResponsibleReportRow(u, quantityByPeriod.ToArray(),
                    interviewsForUser.Any() ? interviewsForUser.First().UserName : "", interviewsForUser.Count());
            }).ToArray();

            var quantityTotalRow =
                this.CreateQuantityTotalRow(interviewStatusesByDateRange, dateTimeRanges, selectUserAndTimestamp);

            return new QuantityByResponsibleReportView(rows, quantityTotalRow, dateTimeRanges, responsibleUsersCount);
        }

        private QuantityTotalRow CreateQuantityTotalRow<T>(IQueryable<T> interviews, DateTimeRange[] dateTimeRanges,
            Expression<Func<T, UserAndTimestamp>> userIdSelector)
        {
            var allInterviewsInStatus = interviews.Select(userIdSelector)
                  .Select(i => i.Timestamp.Date)
                  .ToArray();

            var quantityByPeriod = new List<long>();

            foreach (var dateTimeRange in dateTimeRanges)
            {
                var count = allInterviewsInStatus.Count(d => d >= dateTimeRange.From && d < dateTimeRange.To);
                quantityByPeriod.Add(count);
            }

            return new QuantityTotalRow(quantityByPeriod.ToArray(), allInterviewsInStatus.Length);
        }

        private IQueryable<TimeSpanBetweenStatuses> QueryCompleteStatusesExcludingRestarts(
         Guid questionnaireId,
         long questionnaireVersion,
         DateTime from,
         DateTime to)
        {
            if (questionnaireId != Guid.Empty)
            {
                return this.interviewStatusTimeSpansStorage.Query(_ =>
                    _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                        .SelectMany(x => x.TimeSpansBetweenStatuses)
                        .Where(ics => ics.EndStatusTimestamp.Date >= from && ics.EndStatusTimestamp.Date < to.Date && ics.EndStatus == InterviewExportedAction.Completed));
            }
            else
            {
                return this.interviewStatusTimeSpansStorage.Query(_ =>
                        _.SelectMany(x => x.TimeSpansBetweenStatuses)
                        .Where(ics => ics.EndStatusTimestamp.Date >= from && ics.EndStatusTimestamp.Date < to.Date && ics.EndStatus == InterviewExportedAction.Completed));

            }
        }

        private IQueryable<InterviewCommentedStatus> QueryInterviewStatuses(
            Guid questionnaireId,
            long questionnaireVersion,
            DateTime from,
            DateTime to,
            InterviewExportedAction[] statuses)
        {
            if (questionnaireId != Guid.Empty)
            {
                return this.interviewstatusStorage.Query(
                    _ =>
                        _.Where(
                                x => x.QuestionnaireId == questionnaireId &&
                                     x.QuestionnaireVersion == questionnaireVersion)
                            .SelectMany(x => x.InterviewCommentedStatuses)
                            .Where(ics =>
                                ics.Timestamp.Date >= from && ics.Timestamp.Date < to.Date &&
                                statuses.Contains(ics.Status)));
            }
            else
            {
                return this.interviewstatusStorage.Query(
                    _ =>
                        _.SelectMany(x => x.InterviewCommentedStatuses)
                            .Where(ics =>
                                ics.Timestamp.Date >= from && ics.Timestamp.Date < to.Date &&
                                statuses.Contains(ics.Status)));
            }
        }

        private bool IsCompleteReportRequested(InterviewExportedAction[] interviewStatuses)
        {
            if (interviewStatuses.Length != 1)
                return false;
            return interviewStatuses[0] == InterviewExportedAction.Completed;
        }

        public QuantityByResponsibleReportView Load(QuantityByInterviewersReportInputModel input)
        {
            if (this.IsCompleteReportRequested(input.InterviewStatuses))
            {
                return this.Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                queryInterviewStatusesByDateRange: (from, to) => this.QueryCompleteStatusesExcludingRestarts(input.QuestionnaireId, input.QuestionnaireVersion, from, to).Where(u => u.SupervisorId == input.SupervisorId),
                selectUserId: u => u.InterviewerId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.EndStatusTimestamp });
            }
            return this.Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                queryInterviewStatusesByDateRange: (from, to) => this.QueryInterviewStatuses(input.QuestionnaireId, input.QuestionnaireVersion, from, to, input.InterviewStatuses).Where(u => u.SupervisorId == input.SupervisorId),
                selectUserId: u => u.InterviewerId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.Timestamp });
        }

        public QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input)
        {
            if (this.IsCompleteReportRequested(input.InterviewStatuses))
            {
                return this.Load(
                 reportStartDate: input.From,
                 period: input.Period,
                 columnCount: input.ColumnCount,
                 page: input.Page,
                 pageSize: input.PageSize,
                 queryInterviewStatusesByDateRange: (from, to) => this.QueryCompleteStatusesExcludingRestarts(input.QuestionnaireId, input.QuestionnaireVersion, from, to),
                 selectUserId: u => u.SupervisorId.Value,
                 selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.EndStatusTimestamp });
            }

            return this.Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                queryInterviewStatusesByDateRange: (from, to) => this.QueryInterviewStatuses(input.QuestionnaireId, input.QuestionnaireVersion, from, to, input.InterviewStatuses),
                selectUserId: u => u.SupervisorId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.Timestamp });
        }

        private DateTime AddPeriod(DateTime d, string period, int value)
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

        class UserAndTimestamp
        {
            public Guid? UserId { get; set; }
            public string UserName { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
