using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class SpeedReportFactory:
        IViewFactory<SpeedByInterviewersReportInputModel, SpeedByResponsibleReportView>,
        IViewFactory<SpeedBySupervisorsReportInputModel, SpeedByResponsibleReportView>, 
        IViewFactory<SpeedBetweenStatusesByInterviewersReportInputModel, SpeedByResponsibleReportView>,
        IViewFactory<SpeedBetweenStatusesBySupervisorsReportInputModel, SpeedByResponsibleReportView>
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
         Expression<Func<T, UserAndTimestampAndTimespan>> userIdSelector)
        {
            var from = reportStartDate.Date;
            var to = AddPeriod(from, period, columnCount);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(from, period, i).Date, AddPeriod(from, period, i + 1).Date))
                    .ToArray();

            var users =
                query(questionnaireId, questionnaireVersion, from, to)
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
            return interviewStatusTimeSpansStorage.Query(_ =>
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
            return interviewStatusesStorage.Query(_ =>
                _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Where(
                        ics =>
                            ics.Timestamp.Date >= from && ics.Timestamp.Date < to.Date && statuses.Contains(ics.Status) &&
                            ics.TimeSpanWithPreviousStatus.HasValue));
        }

        public SpeedByResponsibleReportView Load(SpeedByInterviewersReportInputModel input)
        {
            return Load(
                input.From,
                input.Period,
                input.ColumnCount,
                input.Page,
                input.PageSize,
                input.QuestionnaireId,
                input.QuestionnaireVersion,
                (questionnaireId, questionnaireVersion, from,to)=>QueryInterviewStatuses(questionnaireId, questionnaireVersion,from, to,input.InterviewStatuses),
                u => u.InterviewerId.Value,
                i => new UserAndTimestampAndTimespan() { UserId = i.InterviewerId, Timestamp = i.Timestamp, Timespan = i.TimeSpanWithPreviousStatus.Value, UserName = i.InterviewerName});
        }

        public SpeedByResponsibleReportView Load(SpeedBySupervisorsReportInputModel input)
        {
            return Load(
                input.From,
                input.Period,
                input.ColumnCount,
                input.Page,
                input.PageSize,
                input.QuestionnaireId,
                input.QuestionnaireVersion,
                (questionnaireId, questionnaireVersion, from, to) => QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, input.InterviewStatuses),
                u => u.SupervisorId.Value,
                i => new UserAndTimestampAndTimespan() { UserId = i.SupervisorId, Timestamp = i.Timestamp, Timespan = i.TimeSpanWithPreviousStatus.Value, UserName = i.SupervisorName});
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
                (questionnaireId, questionnaireVersion, from, to) => QueryTimeSpanBetweenStatuses(questionnaireId, questionnaireVersion, from, to, input.BeginInterviewStatuses, input.EndInterviewStatuses),
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
                (questionnaireId, questionnaireVersion, from, to) => QueryTimeSpanBetweenStatuses(questionnaireId, questionnaireVersion, from, to, input.BeginInterviewStatuses, input.EndInterviewStatuses),
                u => u.SupervisorId.Value,
                i => new UserAndTimestampAndTimespan() { UserId = i.SupervisorId, Timestamp = i.EndStatusTimestamp, Timespan = i.TimeSpan, UserName = i.SupervisorName });
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
