using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class SpeedReportBetweenStatusesFactory:
        IViewFactory<SpeedBetweenStatusesByInterviewersReportInputModel, SpeedByResponsibleReportView>,
        IViewFactory<SpeedBetweenStatusesBySupervisorsReportInputModel, SpeedByResponsibleReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusesStorage;

        public SpeedReportBetweenStatusesFactory(IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusesStorage)
        {
            this.interviewStatusesStorage = interviewStatusesStorage;
        }

        private SpeedByResponsibleReportView Load(
            DateTime reportStartDate,
            string period,
            int columnCount,
            int page,
            int pageSize,
            Guid questionnaireId,
            long questionnaireVersion,
            InterviewExportedAction[] beginStatuses,
            InterviewExportedAction[] endStatuses,
            Expression<Func<TimeSpanBetweenStatuses, Guid>> selectUser,
            Expression<Func<TimeSpanBetweenStatuses, UserAndTimestampAndTimespan>> userIdSelector)
        {
            var to = reportStartDate.Date;
            var from = AddPeriod(to, period, -columnCount);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(to, period, -(i + 1)).Date, AddPeriod(to, period, -i).Date))
                    .ToArray();

            var users =
                QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, beginStatuses, endStatuses)
                    .Select(selectUser)
                    .Distinct();

            var usersCount = users.Count();

            var userIds = users.Skip((page - 1) * pageSize)
                .Take(pageSize).ToArray();

            var allInterviewsInStatus =
                QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, beginStatuses, endStatuses)
                    .Select(userIdSelector)
                    .Where(ics => ics.UserId.HasValue && userIds.Contains(ics.UserId.Value))
                    .Select(i => new {UserId = i.UserId.Value, i.UserName, i.Timestamp, i.Timespan})
                    .ToArray();

            var rows = userIds.Select(u =>
            {
                var interviewsForUser = allInterviewsInStatus.Where(i => i.UserId == u).ToArray();
                var speedByPeriod = new List<double?>();

                foreach (var dateTimeRange in dateTimeRanges)
                {
                    var interviewsInPeriod =
                        interviewsForUser.Where(
                            ics => ics.Timestamp.Date > dateTimeRange.From && ics.Timestamp.Date <= dateTimeRange.To).ToArray();
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

        private IQueryable<TimeSpanBetweenStatuses> QueryInterviewStatuses(
            Guid questionnaireId,
            long questionnaireVersion,
            DateTime from,
            DateTime to,
            InterviewExportedAction[] beginStatuses,
            InterviewExportedAction[] endStatuses)
        {
            return interviewStatusesStorage.Query(_ =>
                _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                    .SelectMany(x => x.TimeSpansBetweenStatuses)
                    .Where(
                        ics =>
                            ics.EndStatusTimestamp.Date > from && ics.EndStatusTimestamp.Date <= to.Date && endStatuses.Contains(ics.EndStatus) && beginStatuses.Contains(ics.BeginStatus)));
        }

        public SpeedByResponsibleReportView Load(SpeedBetweenStatusesByInterviewersReportInputModel input)
        {
            return Load(
                input.From,
                input.Period,
                input.ColumnCount,
                input.Page,
                input.PageSize,
                input.QuestionnaireId,
                input.QuestionnaireVersion,
                input.BeginInterviewStatuses,
                input.EndInterviewStatuses,
                u => u.InterviewerId.Value,
                i => new UserAndTimestampAndTimespan() { UserId = i.InterviewerId, Timestamp = i.EndStatusTimestamp, Timespan = i.TimeSpan, UserName = i.InterviewerName });
        }

        public SpeedByResponsibleReportView Load(SpeedBetweenStatusesBySupervisorsReportInputModel input)
        {
            return Load(
                input.From,
                input.Period,
                input.ColumnCount,
                input.Page,
                input.PageSize,
                input.QuestionnaireId,
                input.QuestionnaireVersion,
                 input.BeginInterviewStatuses,
                input.EndInterviewStatuses,
                u => u.SupervisorId.Value,
                i => new UserAndTimestampAndTimespan() { UserId = i.SupervisorId, Timestamp = i.EndStatusTimestamp, Timespan = i.TimeSpan, UserName = i.SupervisorName});
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
    }
}