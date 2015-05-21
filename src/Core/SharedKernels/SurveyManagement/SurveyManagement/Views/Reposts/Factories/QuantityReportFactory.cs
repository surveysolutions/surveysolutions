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
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class QuantityReportFactory : 
        IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView>,
        IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> statuses;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public QuantityReportFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> statuses, IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.statuses = statuses;
            this.users = users;
        }

        private QuantityByResponsibleReportView Load(
            DateTime reportStartDate,
            string period,
            int columnCount,
            int page,
            int pageSize,
            Guid questionnaireId,
            long questionnaireVersion,
            InterviewStatus status,
            Expression<Func<UserDocument, bool>> queryUsers, 
            Expression<Func<InterviewCommentedStatus, UserAndTimestamp>> userIdSelector)
        {
            var to = reportStartDate.Date;
            var from = AddPeriod(to, period, -columnCount);

            var dateTimeRanges =
                Enumerable.Range(0, columnCount)
                    .Select(i => new DateTimeRange(AddPeriod(to, period, -(i + 1)).Date, AddPeriod(to, period, -i).Date))
                    .ToArray();

            var usersCount = users.Query(_ => _.Count(queryUsers));

            var userDetails = users.Query(
                _ => _.Where(queryUsers)
                    .OrderBy(u => u.UserName)
                    .Select(u => new {UserId = u.PublicKey, UserName = u.UserName})
                    .Skip((page - 1)*pageSize)
                    .Take(pageSize).ToArray());

            var userIds = userDetails.Select(u => u.UserId).ToHashSet();

            var allInterviewsInStatus = statuses.Query(_ =>
                _.Where(x => x.QuestionnaireId == questionnaireId && x.QuestionnaireVersion == questionnaireVersion)
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Where(ics => ics.Timestamp.Date > from && ics.Timestamp.Date <= to.Date && ics.Status == status)
                    .Select(userIdSelector)
                    .Where(ics => ics.UserId.HasValue && userIds.Contains(ics.UserId.Value))
                    .Select(i => new {UserId = i.UserId.Value, i.Timestamp})
                    .ToArray()
                );

            var rows = userDetails.Select(u =>
            {
                var interviewsForUser = allInterviewsInStatus.Where(i => i.UserId == u.UserId).ToArray();
                var quantityByPeriod = new List<long>();

                foreach (var dateTimeRange in dateTimeRanges)
                {
                    var count =
                        interviewsForUser.Count(
                            ics => ics.Timestamp.Date > dateTimeRange.From && ics.Timestamp.Date <= dateTimeRange.To);
                    quantityByPeriod.Add(count);
                }
                return new QuantityByInterviewersReportRow(u.UserId, quantityByPeriod.ToArray(),
                    u.UserName, interviewsForUser.Count());
            }).ToArray();

            return new QuantityByResponsibleReportView(rows, dateTimeRanges, usersCount);
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
                input.InterviewStatus,
                u => u.Roles.Contains(UserRoles.Operator) && u.Supervisor.Id == input.SupervisorId,
                i => new UserAndTimestamp() {UserId = i.InterviewerId, Timestamp = i.Timestamp});
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
                input.InterviewStatus,
                u => u.Roles.Contains(UserRoles.Supervisor),
                i => new UserAndTimestamp() {UserId = i.SupervisorId, Timestamp = i.Timestamp});
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
            public DateTime Timestamp { get; set; }
        }
    }
}
