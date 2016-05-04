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

        public QuantityReportFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> interviewstatusStorage)
        {
            this.interviewstatusStorage = interviewstatusStorage;
        }

        private QuantityByResponsibleReportView Load(
            DateTime reportStartDate,
            string period,
            int columnCount,
            int page,
            int pageSize,
            Guid questionnaireId,
            long questionnaireVersion,
            InterviewExportedAction[] statuses,
            Expression<Func<InterviewCommentedStatus, Guid>> selectUser,
            Expression<Func<InterviewCommentedStatus, bool>> restrictUser,
            Expression<Func<InterviewCommentedStatus, UserAndTimestamp>> userIdSelector)
        {
            var from = reportStartDate.Date;
            var to = AddPeriod(from, period, columnCount);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(from, period, i).Date, AddPeriod(from, period, i + 1).Date))
                    .Where(i => i.From.Date <= DateTime.Now.Date)
                    .ToArray();

            var allUsersQuery = QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, statuses);

            if (restrictUser != null)
                allUsersQuery = allUsersQuery.Where(restrictUser);

            var users = allUsersQuery
                    .Select(selectUser)
                    .Distinct();

            var usersCount = users.Count();

            var userIds = users.Skip((page - 1)*pageSize)
                .Take(pageSize).ToArray();

            var allInterviewsInStatus =
                QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, statuses)
                    .Select(userIdSelector)
                    .Where(ics => ics.UserId.HasValue && userIds.Contains(ics.UserId.Value))
                    .Select(i => new {UserId = i.UserId.Value, i.UserName, i.Timestamp})
                    .ToArray();

            var rows = userIds.Select(u =>
            {
                var interviewsForUser = allInterviewsInStatus.Where(i => i.UserId == u).ToArray();
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
                CreateQuantityTotalRow(allUsersQuery, dateTimeRanges);

            return new QuantityByResponsibleReportView(rows, quantityTotalRow, dateTimeRanges, usersCount);
        }

        private QuantityTotalRow CreateQuantityTotalRow(IQueryable<InterviewCommentedStatus> interviews, DateTimeRange[] dateTimeRanges)
        {
            var allInterviewsInStatus = interviews
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

        public QuantityByResponsibleReportView Load(QuantityByInterviewersReportInputModel input)
        {
            return Load(
                reportStartDate: input.From,
                period: input.Period,
                columnCount: input.ColumnCount,
                page: input.Page,
                pageSize: input.PageSize,
                questionnaireId: input.QuestionnaireId,
                questionnaireVersion: input.QuestionnaireVersion,
                statuses: input.InterviewStatuses,
                selectUser: u => u.InterviewerId.Value,
                restrictUser: u => u.SupervisorId == input.SupervisorId,
                userIdSelector: i => new UserAndTimestamp(){UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.Timestamp});
        }

        public QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input)
        {
            return Load(
                reportStartDate:input.From,
                period:input.Period,
                columnCount:input.ColumnCount,
                page:input.Page,
                pageSize:input.PageSize,
                questionnaireId:input.QuestionnaireId,
                questionnaireVersion:input.QuestionnaireVersion,
                statuses:input.InterviewStatuses,
                selectUser:u => u.SupervisorId.Value,
                restrictUser:null,
                userIdSelector:i => new UserAndTimestamp(){UserId = i.SupervisorId, UserName = i.SupervisorName, Timestamp = i.Timestamp});
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
