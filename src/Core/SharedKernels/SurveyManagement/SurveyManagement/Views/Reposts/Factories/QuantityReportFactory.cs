using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
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
    public class QuantityReportFactory : 
        IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView>,
        IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewstatusStorage;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> usersStorage;

        public QuantityReportFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> interviewstatusStorage, IQueryableReadSideRepositoryReader<UserDocument> usersStorage)
        {
            this.interviewstatusStorage = interviewstatusStorage;
            this.usersStorage = usersStorage;
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
            Expression<Func<InterviewCommentedStatus, UserAndTimestamp>> userIdSelector)
        {
            var from = reportStartDate.Date;
            var to = AddPeriod(from, period, columnCount);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(from, period, i).Date, AddPeriod(from, period, i + 1).Date))
                    .ToArray();

            var users =
                QueryInterviewStatuses(questionnaireId, questionnaireVersion, from, to, statuses)
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

            return new QuantityByResponsibleReportView(rows, dateTimeRanges, usersCount);
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
                input.From,
                input.Period,
                input.ColumnCount,
                input.Page,
                input.PageSize,
                input.QuestionnaireId,
                input.QuestionnaireVersion,
                input.InterviewStatuses,
                u => u.InterviewerId.Value,
                i => new UserAndTimestamp() {UserId = i.InterviewerId, UserName = i.InterviewerName, Timestamp = i.Timestamp});
        }

        public QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input)
        {
            return Load(
                input.From,
                input.Period,
                input.ColumnCount,
                input.Page,
                input.PageSize,
                input.QuestionnaireId,
                input.QuestionnaireVersion,
                input.InterviewStatuses,
                u => u.SupervisorId.Value,
                i => new UserAndTimestamp() {UserId = i.SupervisorId, UserName =i.SupervisorName, Timestamp = i.Timestamp});
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

        class UserAndTimestamp
        {
            public Guid? UserId { get; set; }
            public string UserName { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
