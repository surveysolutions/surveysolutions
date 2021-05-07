using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface IQuantityReportFactory: IReport<QuantityByInterviewersReportInputModel>, IReport<QuantityBySupervisorsReportInputModel>
    {
        QuantityByResponsibleReportView Load(QuantityByInterviewersReportInputModel input);
        QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input);
    }

    public class QuantityReportFactory : IQuantityReportFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;

        public QuantityReportFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
        }

        private QuantityByResponsibleReportView Load<T>(
            Guid? questionnaireId, 
            long? questionnaireVersion,
            DateTime reportStartDate,
            int timezoneAdjastmentMins,
            string period,
            int columnCount,
            int page,
            int pageSize,
            Func<DateTime, DateTime, IQueryable<T>> queryInterviewStatusesByDateRange,
            Expression<Func<T, Guid>> selectUserId,
            Expression<Func<T, UserAndTimestamp>> selectUserAndTimestamp)
        {
            var ranges = ReportHelpers.BuildColumns(reportStartDate, period, columnCount, timezoneAdjastmentMins,
                questionnaireId, questionnaireVersion, this.interviewSummaryStorage);

            var interviewStatusesByDateRange = queryInterviewStatusesByDateRange(ranges.FromUtc, ranges.ToUtc);

            var userIdsOfAllResponsibleForTheInterviews = interviewStatusesByDateRange
                    .Select(selectUserId)
                    .Distinct();

            var responsibleUsersCount = userIdsOfAllResponsibleForTheInterviews.Count();

            var responsibleUserIdsForOnePage = userIdsOfAllResponsibleForTheInterviews.Skip((page - 1) * pageSize)
                .Take(pageSize).ToArray();

            var interviewStatusChangeDateWithResponsible = queryInterviewStatusesByDateRange(ranges.FromUtc, ranges.ToUtc)
                    .Select(selectUserAndTimestamp)
                    .Where(ics => ics.UserId.HasValue && responsibleUserIdsForOnePage.Contains(ics.UserId.Value))
                    .Select(i => new { UserId = i.UserId.Value, i.UserName, i.Timestamp });
            
            List<QuantityByResponsibleReportRow> list = responsibleUserIdsForOnePage
                .Select(x => new QuantityByResponsibleReportRow(ranges.ColumnRangesLocal.Length)
                {
                    ResponsibleId = x
                }).ToList();

            for (int dateRangeIndex = 0; dateRangeIndex < ranges.ColumnRangesUtc.Length; dateRangeIndex++)
            {
                var timeSpanFrom = ranges.ColumnRangesUtc[dateRangeIndex].From;
                var timeSpanTo = ranges.ColumnRangesUtc[dateRangeIndex].To;
                var range = (from i in interviewStatusChangeDateWithResponsible
                    where i.Timestamp >= timeSpanFrom && i.Timestamp < timeSpanTo
                    group i by new {i.UserId, i.UserName } into grouping
                    select new
                    {
                        grouping.Key.UserId,
                        grouping.Key.UserName,
                        Count = grouping.Count()
                    }).ToList();


                foreach (var r in range)
                {
                    var quantityByResponsibleReportRow = list.First(x => x.ResponsibleId == r.UserId);

                    quantityByResponsibleReportRow.ResponsibleName = r.UserName;
                    quantityByResponsibleReportRow.QuantityByPeriod[dateRangeIndex] = r.Count;
                }
            }

            foreach (var column in ranges.ColumnRangesUtc)
            {
                var range = (from i in interviewStatusChangeDateWithResponsible
                    group i by i.UserId into grouping
                    select new
                    {
                        UserId = grouping.Key,
                        Count = grouping.Count()
                    }).ToList();

                foreach (var r in range)
                {
                    var quantityByResponsibleReportRow = list.First(x => x.ResponsibleId == r.UserId);
                    quantityByResponsibleReportRow.Total = r.Count;
                }
            }

            var quantityTotalRow =
                this.CreateQuantityTotalRow(interviewStatusesByDateRange, ranges.ColumnRangesUtc, selectUserAndTimestamp);

            return new QuantityByResponsibleReportView(list, quantityTotalRow, ranges.ColumnRangesLocal, responsibleUsersCount);
        }

        
        private QuantityTotalRow CreateQuantityTotalRow<T>(IQueryable<T> interviews, DateTimeRange[] dateTimeRanges,
            Expression<Func<T, UserAndTimestamp>> userIdSelector)
        {
            var allInterviewsInStatus = interviews.Select(userIdSelector)
                  .Select(i => i.Timestamp);

            var quantityByPeriod = new List<long>();

            foreach (var dateTimeRange in dateTimeRanges)
            {
                var fromDate = dateTimeRange.From;
                var toDate = dateTimeRange.To;
                var count = allInterviewsInStatus.Count(d => d >= fromDate && d < toDate);
                quantityByPeriod.Add(count);
            }

            return new QuantityTotalRow(quantityByPeriod.ToArray(), allInterviewsInStatus.Count());
        }

        private IQueryable<TimeSpanBetweenStatuses> QueryCompleteStatusesExcludingRestarts(
         Guid? questionnaireId,
         long? questionnaireVersion,
         DateTime from,
         DateTime to)
        {
            return this.interviewSummaryStorage.Query(_ =>
            {
                var query = _;
                if (questionnaireId.HasValue)
                {
                    query = query.Where(x => x.QuestionnaireId == questionnaireId);
                }

                if (questionnaireVersion.HasValue)
                {
                    query = query.Where(x => x.QuestionnaireVersion == questionnaireVersion);
                }

                return query.SelectMany(x => x.TimeSpansBetweenStatuses)
                            .Where(ics =>
                                ics.EndStatusTimestamp >= @from && ics.EndStatusTimestamp < to &&
                                ics.EndStatus == InterviewExportedAction.Completed);
            });
        }

        private IQueryable<InterviewCommentedStatus> QueryInterviewStatuses(
            Guid? questionnaireId,
            long? questionnaireVersion,
            DateTime from,
            DateTime to,
            InterviewExportedAction[] statuses)
        {
            return this.interviewSummaryStorage.Query(
                _ =>
                {
                    var query = _;
                    if (questionnaireId.HasValue)
                    {
                        query = query.Where(x => x.QuestionnaireId == questionnaireId);
                    }

                    if (questionnaireVersion.HasValue)
                    {
                        query = query.Where(x => x.QuestionnaireVersion == questionnaireVersion);
                    }

                    return query.SelectMany(x => x.InterviewCommentedStatuses)
                                .Where(ics =>
                                    ics.Timestamp >= @from && ics.Timestamp < to &&
                                    statuses.Contains(ics.Status));
                });
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
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                reportStartDate: input.From,
                timezoneAdjastmentMins: input.TimezoneOffsetMinutes,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                queryInterviewStatusesByDateRange: (from, to) => 
                    this.QueryCompleteStatusesExcludingRestarts(input.QuestionnaireId, input.QuestionnaireVersion, from, to).Where(u => u.SupervisorId == input.SupervisorId),
                selectUserId: u => u.InterviewerId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.EndStatusTimestamp });
            }
            return this.Load(
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                reportStartDate: input.From,
                timezoneAdjastmentMins: input.TimezoneOffsetMinutes,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                queryInterviewStatusesByDateRange: (from, to) => 
                    this.QueryInterviewStatuses(input.QuestionnaireId, input.QuestionnaireVersion, from, to, input.InterviewStatuses).Where(u => u.SupervisorId == input.SupervisorId),
                selectUserId: u => u.InterviewerId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.Timestamp });
        }

        public QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input)
        {
            if (this.IsCompleteReportRequested(input.InterviewStatuses))
            {
                return this.Load(
                 questionnaireId: input.QuestionnaireId,
                 questionnaireVersion: input.QuestionnaireVersion,
                 reportStartDate: input.From,
                 timezoneAdjastmentMins: input.TimezoneOffsetMinutes,
                 period: input.Period,
                 columnCount: input.ColumnCount,
                 page: input.Page,
                 pageSize: input.PageSize,
                 queryInterviewStatusesByDateRange: (from, to) => this.QueryCompleteStatusesExcludingRestarts(input.QuestionnaireId, input.QuestionnaireVersion, from, to),
                 selectUserId: u => u.SupervisorId.Value,
                 selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.EndStatusTimestamp });
            }

            return this.Load(
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                reportStartDate: input.From,
                timezoneAdjastmentMins: input.TimezoneOffsetMinutes,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                queryInterviewStatusesByDateRange: (from, to) => this.QueryInterviewStatuses(input.QuestionnaireId, input.QuestionnaireVersion, from, to, input.InterviewStatuses),
                selectUserId: u => u.SupervisorId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.Timestamp });
        }

        class UserAndTimestamp
        {
            public Guid? UserId { get; set; }
            public string UserName { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public ReportView GetReport(QuantityByInterviewersReportInputModel model)
            => GetReportView(this.Load(model), false);

        public ReportView GetReport(QuantityBySupervisorsReportInputModel model) 
            => GetReportView(this.Load(model), true);

        private ReportView GetReportView(QuantityByResponsibleReportView view, bool forAdminOrHq)
            => new ReportView
            {
                Headers = ToReportHeader(view).ToArray(),
                Data = ToDataView(view, forAdminOrHq) 
            };

        private IEnumerable<string> ToReportHeader(QuantityByResponsibleReportView view)
        {
            yield return Report.COLUMN_TEAM_MEMBER;

            foreach (var date in view.DateTimeRanges.Select(y => y.To.ToString("yyyy-MM-dd")))
                yield return date;

            yield return Report.COLUMN_AVERAGE;
            yield return Report.COLUMN_TOTAL;
        }

        private object[][] ToDataView(QuantityByResponsibleReportView view, bool forAdminOrHq)
        {
            var data = new List<object[]> {ToReportRow(view.TotalRow, forAdminOrHq).ToArray()};

            data.AddRange(view.Items.Select(ToReportRow).Select(item => item.ToArray()));

            return data.ToArray();
        }

        private IEnumerable<object> ToReportRow(QuantityTotalRow totalRow, bool forAdminOrHq)
        {
            yield return forAdminOrHq ? Strings.AllTeams : Strings.AllInterviewers;
            foreach (var total in totalRow.QuantityByPeriod)
            {
                yield return total;
            }
            yield return totalRow.Average;
            yield return totalRow.Total;
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
