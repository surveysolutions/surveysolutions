using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface IQuantityReportFactory: IReport<QuantityByInterviewersReportInputModel>, IReport<QuantityBySupervisorsReportInputModel>
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
            QuestionnaireIdentity questionnaire,
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
            var to = reportStartDate.Date.AddDays(1);

            DateTime? minDate = GetFirstInterviewCreatedDate(questionnaire);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(this.AddPeriod(from, period, i).Date, this.AddPeriod(from, period, i + 1).Date))
                    .Where(i => i.From.Date <= DateTime.Now.Date && minDate.HasValue && i.To.Date >= minDate)
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
                    var count = interviewsForUser.Count(ics => ics.Timestamp >= dateTimeRange.From && ics.Timestamp < dateTimeRange.To);
                    quantityByPeriod.Add(count);
                }
                return new QuantityByResponsibleReportRow(u, quantityByPeriod.ToArray(),
                    interviewsForUser.Any() ? interviewsForUser.First().UserName : "", interviewsForUser.Count());
            }).ToArray();

            var quantityTotalRow =
                this.CreateQuantityTotalRow(interviewStatusesByDateRange, dateTimeRanges, selectUserAndTimestamp);

            return new QuantityByResponsibleReportView(rows, quantityTotalRow, dateTimeRanges, responsibleUsersCount);
        }

        private DateTime? GetFirstInterviewCreatedDate(QuestionnaireIdentity questionnaire)
        {
            DateTime? minDate;
            if (questionnaire != null)
            {
                minDate = this.interviewstatusStorage.Query(_ => _
                    .Where(x => x.QuestionnaireId == questionnaire.QuestionnaireId &&
                                x.QuestionnaireVersion == questionnaire.Version)
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Select(x => (DateTime?) x.Timestamp.Date)
                    .Min());
            }
            else
            {
                minDate = this.interviewstatusStorage.Query(_ => _
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Select(x => (DateTime?) x.Timestamp.Date)
                    .Min());
            }
            return minDate;
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
                        .Where(ics => ics.EndStatusTimestamp >= from && ics.EndStatusTimestamp < to.Date && ics.EndStatus == InterviewExportedAction.Completed));
            }
            else
            {
                return this.interviewStatusTimeSpansStorage.Query(_ =>
                        _.SelectMany(x => x.TimeSpansBetweenStatuses)
                        .Where(ics => ics.EndStatusTimestamp >= from && ics.EndStatusTimestamp < to.Date && ics.EndStatus == InterviewExportedAction.Completed));

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
                                ics.Timestamp >= from && ics.Timestamp < to.Date &&
                                statuses.Contains(ics.Status)));
            }
            else
            {
                return this.interviewstatusStorage.Query(
                    _ =>
                        _.SelectMany(x => x.InterviewCommentedStatuses)
                            .Where(ics =>
                                ics.Timestamp >= from && ics.Timestamp < to.Date &&
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
                questionnaire: input.Questionnaire(),
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
                questionnaire: input.Questionnaire(),
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
                 questionnaire: input.Questionnaire(),
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
                questionnaire: input.Questionnaire(),
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

        public ReportView GetReport(QuantityByInterviewersReportInputModel model)
            => GetReportView(this.Load(model));

        public ReportView GetReport(QuantityBySupervisorsReportInputModel model) 
            => GetReportView(this.Load(model));

        private ReportView GetReportView(QuantityByResponsibleReportView view)
            => new ReportView
            {
                Headers = ToReportHeader(view).ToArray(),
                Data = view.Items.Select(x => ToReportRow(x).ToArray()).ToArray()
            };

        private IEnumerable<string> ToReportHeader(QuantityByResponsibleReportView view)
        {
            yield return Report.COLUMN_TEAM_MEMBER;

            foreach (var date in view.DateTimeRanges.Select(y => y.From.ToString("yyyy-MM-dd")))
                yield return date;

            yield return Report.COLUMN_AVERAGE;
            yield return Report.COLUMN_TOTAL;
        }

        private IEnumerable<object> ToReportRow(QuantityByResponsibleReportRow row)
        {
            yield return row.ResponsibleName;

            foreach (var quantity in row.QuantityByPeriod)
                yield return quantity;

            yield return row.Average;
            yield return row.Total;
        }
    }
}
