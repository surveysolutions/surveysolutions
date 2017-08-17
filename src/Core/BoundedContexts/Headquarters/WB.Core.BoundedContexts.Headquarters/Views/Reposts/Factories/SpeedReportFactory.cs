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

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public interface ISpeedReportFactory: IReport<SpeedByInterviewersReportInputModel>, 
        IReport<SpeedBySupervisorsReportInputModel>,
        IReport<SpeedBetweenStatusesByInterviewersReportInputModel>,
        IReport<SpeedBetweenStatusesBySupervisorsReportInputModel>
    {
        SpeedByResponsibleReportView Load(SpeedByInterviewersReportInputModel input);
        SpeedByResponsibleReportView Load(SpeedBySupervisorsReportInputModel input);
        SpeedByResponsibleReportView Load(SpeedBetweenStatusesByInterviewersReportInputModel input);
        SpeedByResponsibleReportView Load(SpeedBetweenStatusesBySupervisorsReportInputModel input);
    }

    public class SpeedReportFactory: ISpeedReportFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatusesStorage;

        private readonly IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage;

        public SpeedReportFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatusesStorage, 
            IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusTimeSpansStorage)
        {
            this.interviewStatusesStorage = interviewStatusesStorage;
            this.interviewStatusTimeSpansStorage = interviewStatusTimeSpansStorage;
        }

        private SpeedByResponsibleReportView Load<T>(
         DateTime reportStartDate,
         string period,
         int columnCount,
         int page,
         int pageSize,
         Guid questionnaireId,
         long questionnaireVersion,
         Func<Guid, long,DateTime,DateTime, IQueryable<T>> query, 
         Expression<Func<T, Guid>> selectUser,
         Expression<Func<T, bool>> restrictUser,
         Expression<Func<T, UserAndTimestampAndTimespan>> userIdSelector)
        {
            var from = this.AddPeriod(reportStartDate.Date, period, -columnCount + 1);
            var to = reportStartDate.Date.AddDays(1);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(this.AddPeriod(from, period, i).Date, this.AddPeriod(from, period, i + 1).Date))
                    .Where(i => i.From.Date <= DateTime.Now.Date)
                    .ToArray();

            var allUsersQuery = query(questionnaireId, questionnaireVersion, from, to);

            if (restrictUser != null)
                allUsersQuery = allUsersQuery.Where(restrictUser);

            var users = allUsersQuery
                .Select(selectUser)
                .Distinct();

            var usersCount = users.Count();

            var userIds = users.Skip((page - 1) * pageSize)
                .Take(pageSize).ToArray();

            var allInterviewsInStatus =
                 query(questionnaireId, questionnaireVersion, from, to)
                    .Select(userIdSelector)
                    .Where(ics => ics.UserId.HasValue && userIds.Contains(ics.UserId.Value))
                    .Select(i => new { UserId = i.UserId.Value, i.UserName, i.Timestamp, i.Timespan })
                    .ToArray();

            var rows = userIds.Select(u =>
            {
                var interviewsForUser = allInterviewsInStatus.Where(i => i.UserId == u).ToArray();
                var speedByPeriod = new List<double?>();

                foreach (var dateTimeRange in dateTimeRanges)
                {
                    var interviewsInPeriod =
                        interviewsForUser.Where(
                            ics => ics.Timestamp.Date >= dateTimeRange.From && ics.Timestamp.Date < dateTimeRange.To).ToArray();
                    if (interviewsInPeriod.Any())
                    {
                        speedByPeriod.Add(Math.Round(interviewsInPeriod.Select(i => Math.Abs(i.Timespan.TotalMinutes)).Average(), 2));
                    }
                    else
                    {
                        speedByPeriod.Add(null);
                    }
                }
                return new SpeedByResponsibleReportRow(
                    responsibleId: u,
                    periods: speedByPeriod.ToArray(),
                    responsibleName: interviewsForUser.Any() ? interviewsForUser.First().UserName : "",
                    average: interviewsForUser.Any() ? Math.Round(interviewsForUser.Select(i => Math.Abs(i.Timespan.TotalMinutes)).Average(), 2) : (double?)null,
                    total: interviewsForUser.Any() ? Math.Round(interviewsForUser.Select(i => Math.Abs(i.Timespan.TotalMinutes)).Sum(), 2) : (double?)null);
            }).ToArray();

            return new SpeedByResponsibleReportView(rows, dateTimeRanges, usersCount);
        }

        private IQueryable<TimeSpanBetweenStatuses> QueryTimeSpanBetweenStatuses(
          Guid questionnaireId,
          long questionnaireVersion,
          DateTime from,
          DateTime to,
          InterviewExportedAction[] beginStatuses,
          InterviewExportedAction[] endStatuses)
        {
            return this.interviewStatusTimeSpansStorage.Query(_ =>
                _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                    .SelectMany(x => x.TimeSpansBetweenStatuses)
                    .Where(
                        ics =>
                            ics.EndStatusTimestamp.Date >= from && ics.EndStatusTimestamp.Date < to.Date && endStatuses.Contains(ics.EndStatus) && beginStatuses.Contains(ics.BeginStatus)));
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
                return this.interviewStatusesStorage.Query(_ =>
                    _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                        .SelectMany(x => x.InterviewCommentedStatuses)
                        .Where(
                            ics =>
                                ics.Timestamp.Date >= from && ics.Timestamp.Date < to.Date &&
                                statuses.Contains(ics.Status) &&
                                ics.TimeSpanWithPreviousStatus.HasValue));
            }
            else
            {
                return this.interviewStatusesStorage.Query(_ =>
                    _.SelectMany(x => x.InterviewCommentedStatuses)
                        .Where(ics =>
                                ics.Timestamp.Date >= from && ics.Timestamp.Date < to.Date &&
                                statuses.Contains(ics.Status) &&
                                ics.TimeSpanWithPreviousStatus.HasValue));
            }
        }

        public SpeedByResponsibleReportView Load(SpeedByInterviewersReportInputModel input)
        {
            return this.Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                query:(questionnaireId, questionnaireVersion, from,to) => this.QueryInterviewStatuses(questionnaireId, questionnaireVersion,from, to,input.InterviewStatuses),
                selectUser: u => u.InterviewerId.Value,
                restrictUser: i=>i.SupervisorId== input.SupervisorId,
                userIdSelector: i => new UserAndTimestampAndTimespan() { UserId = i.InterviewerId, Timestamp = i.Timestamp, Timespan = i.TimeSpanWithPreviousStatus.Value, UserName = i.InterviewerName});
        }

        public SpeedByResponsibleReportView Load(SpeedBySupervisorsReportInputModel input)
        {
            return this.Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                query: (questionnaireId, questionnaireVersion, from, to) => this.QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, input.InterviewStatuses),
                selectUser: u => u.SupervisorId.Value,
                restrictUser: null,
                userIdSelector: i => new UserAndTimestampAndTimespan() { UserId = i.SupervisorId, Timestamp = i.Timestamp, Timespan = i.TimeSpanWithPreviousStatus.Value, UserName = i.SupervisorName});
        }

        public SpeedByResponsibleReportView Load(SpeedBetweenStatusesByInterviewersReportInputModel input)
        {
            return this.Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                query: (questionnaireId, questionnaireVersion, from, to) => this.QueryTimeSpanBetweenStatuses(questionnaireId, questionnaireVersion, from, to, input.BeginInterviewStatuses, input.EndInterviewStatuses),
                selectUser: u => u.InterviewerId.Value, 
                restrictUser: i => i.SupervisorId == input.SupervisorId,
                userIdSelector: i => new UserAndTimestampAndTimespan() { UserId = i.InterviewerId, Timestamp = i.EndStatusTimestamp, Timespan = i.TimeSpan, UserName = i.InterviewerName });
        }

        public SpeedByResponsibleReportView Load(SpeedBetweenStatusesBySupervisorsReportInputModel input)
        {
            return this.Load(
                reportStartDate:input.From,
                period:input.Period,
                columnCount:input.ColumnCount,
                page:input.Page,
                pageSize:input.PageSize,
                questionnaireId:input.QuestionnaireId,
                questionnaireVersion:input.QuestionnaireVersion,
                query:(questionnaireId, questionnaireVersion, from, to) => this.QueryTimeSpanBetweenStatuses(questionnaireId, questionnaireVersion, from, to, input.BeginInterviewStatuses, input.EndInterviewStatuses),
                selectUser:u => u.SupervisorId.Value,
                restrictUser: null,
                userIdSelector: i => new UserAndTimestampAndTimespan() { UserId = i.SupervisorId, Timestamp = i.EndStatusTimestamp, Timespan = i.TimeSpan, UserName = i.SupervisorName });
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
            throw new ArgumentException(string.Format("period '{0}' can't be recognized", period));
        }

        class UserAndTimestampAndTimespan
        {
            public Guid? UserId { get; set; }
            public string UserName { get; set; }
            public DateTime Timestamp { get; set; }
            public TimeSpan Timespan { get; set; }
        }

        public ReportView GetReport(SpeedByInterviewersReportInputModel model)
            => GetReportView(this.Load(model));

        public ReportView GetReport(SpeedBySupervisorsReportInputModel model)
            => GetReportView(this.Load(model));

        public ReportView GetReport(SpeedBetweenStatusesByInterviewersReportInputModel model)
            => GetReportView(this.Load(model));

        public ReportView GetReport(SpeedBetweenStatusesBySupervisorsReportInputModel model) 
            => GetReportView(this.Load(model));

        private ReportView GetReportView(SpeedByResponsibleReportView view)
            => new ReportView
            {
                Headers = ToReportHeader(view).ToArray(),
                Data = view.Items.Select(x => ToReportRow(x).ToArray()).ToArray()
            };

        private IEnumerable<string> ToReportHeader(SpeedByResponsibleReportView view)
        {
            yield return Report.COLUMN_TEAM_MEMBER;

            foreach (var date in view.DateTimeRanges.Select(y => y.From.ToString("yyyy-MM-dd")))
                yield return date;

            yield return Report.COLUMN_AVERAGE;
            yield return Report.COLUMN_TOTAL;
        }

        private IEnumerable<object> ToReportRow(SpeedByResponsibleReportRow row)
        {
            yield return row.ResponsibleName;

            foreach (var quantity in row.SpeedByPeriod)
                yield return quantity;

            yield return row.Average;
            yield return row.Total;
        }
    }
}