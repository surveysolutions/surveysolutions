using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class QuantityReportFactory : 
        IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView>,
        IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView>
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
            Guid questionnaireId,
            long questionnaireVersion,
            Func<Guid, long, DateTime, DateTime, IQueryable<T>> queryInterviewStatusesByDateRange,
            Expression<Func<T, Guid>> selectUserId,
            Expression<Func<T, UserAndTimestamp>> selectUserAndTimestamp)
        {
            var from = reportStartDate.Date;
            var to = AddPeriod(from, period, columnCount);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(from, period, i).Date, AddPeriod(from, period, i + 1).Date))
                    .Where(i => i.From.Date <= DateTime.Now.Date)
                    .ToArray();

            var interviewStatusesByDateRange = queryInterviewStatusesByDateRange(questionnaireId, questionnaireVersion, from, to);

            var userIdsOfAllResponsibleForTheInterviews = interviewStatusesByDateRange
                    .Select(selectUserId)
                    .Distinct();

            var responsibleUsersCount = userIdsOfAllResponsibleForTheInterviews.Count();

            var responsibleUserIdsForOnePage = userIdsOfAllResponsibleForTheInterviews.Skip((page - 1)*pageSize)
                .Take(pageSize).ToArray();

            var interviewStatusChangeDateWithResponsible = queryInterviewStatusesByDateRange(questionnaireId, questionnaireVersion, from, to)
                    .Select(selectUserAndTimestamp)
                    .Where(ics => ics.UserId.HasValue && responsibleUserIdsForOnePage.Contains(ics.UserId.Value))
                    .Select(i => new {UserId = i.UserId.Value, i.UserName, i.Timestamp})
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
                CreateQuantityTotalRow(interviewStatusesByDateRange, dateTimeRanges, selectUserAndTimestamp);

            return new QuantityByResponsibleReportView(rows, quantityTotalRow, dateTimeRanges, responsibleUsersCount);
        }

        private QuantityTotalRow CreateQuantityTotalRow<T>(IQueryable<T> interviews, DateTimeRange[] dateTimeRanges,
            Expression<Func<T, UserAndTimestamp>> userIdSelector)
        {
            var allInterviewsInStatus = interviews.Select(userIdSelector)
                  .Select(i =>  i.Timestamp.Date)
                  .ToArray();

            var quantityByPeriod = new List<long>();

            foreach (var dateTimeRange in dateTimeRanges)
            {
                var count = allInterviewsInStatus.Count(d => d >= dateTimeRange.From && d< dateTimeRange.To);
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
            return interviewStatusTimeSpansStorage.Query(_ =>
                _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                    .SelectMany(x => x.TimeSpansBetweenStatuses)
                    .Where(ics =>ics.EndStatusTimestamp.Date >= from && ics.EndStatusTimestamp.Date < to.Date && ics.EndStatus== InterviewExportedAction.Completed));
        }

        private IQueryable<InterviewCommentedStatus> QueryInterviewStatuses(
            Guid questionnaireId,
            long questionnaireVersion,
            DateTime from,
            DateTime to,
            InterviewExportedAction[] statuses)
        {
           return interviewstatusStorage.Query(
             _ =>
                 _.Where(
                     x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                     .SelectMany(x => x.InterviewCommentedStatuses)
                     .Where(ics =>
                         ics.Timestamp.Date >= from && ics.Timestamp.Date < to.Date &&
                         statuses.Contains(ics.Status)));
        }

        private bool IsCompleteReportRequested(InterviewExportedAction[] interviewStatuses)
        {
            if (interviewStatuses.Length != 1)
                return false;
            return interviewStatuses[0] == InterviewExportedAction.Completed;
        }

        public QuantityByResponsibleReportView Load(QuantityByInterviewersReportInputModel input)
        {
            if (IsCompleteReportRequested(input.InterviewStatuses))
            {
                return Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                queryInterviewStatusesByDateRange: (questionnaireId, questionnaireVersion, from, to) => this.QueryCompleteStatusesExcludingRestarts(questionnaireId, questionnaireVersion, from, to).Where(u => u.SupervisorId == input.SupervisorId),
                selectUserId: u => u.InterviewerId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.EndStatusTimestamp });
            }
            return Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                queryInterviewStatusesByDateRange: (questionnaireId, questionnaireVersion, from, to) => QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, input.InterviewStatuses).Where(u => u.SupervisorId == input.SupervisorId),
                selectUserId: u => u.InterviewerId.Value,
                selectUserAndTimestamp: i => new UserAndTimestamp(){UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.Timestamp});
        }

        public QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input)
        {
            if (IsCompleteReportRequested(input.InterviewStatuses))
            {
                return Load(
                 reportStartDate: input.From,
                 period: input.Period,
                 columnCount: input.ColumnCount,
                 page: input.Page,
                 pageSize: input.PageSize,
                 questionnaireId: input.QuestionnaireId,
                 questionnaireVersion: input.QuestionnaireVersion,
                 queryInterviewStatusesByDateRange: QueryCompleteStatusesExcludingRestarts,
                 selectUserId: u => u.SupervisorId.Value,
                 selectUserAndTimestamp: i => new UserAndTimestamp() { UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.EndStatusTimestamp });
            }

            return Load(
                reportStartDate:input.From,
                period:input.Period,
                columnCount:input.ColumnCount,
                page:input.Page,
                pageSize:input.PageSize,
                questionnaireId:input.QuestionnaireId,
                questionnaireVersion:input.QuestionnaireVersion,
                queryInterviewStatusesByDateRange: (questionnaireId, questionnaireVersion, from, to) => QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, input.InterviewStatuses),
                selectUserId:u => u.SupervisorId.Value,
                selectUserAndTimestamp:i => new UserAndTimestamp(){UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.Timestamp});
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
