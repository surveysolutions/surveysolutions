using System;
using System.Collections.Generic;
using System.Linq;
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
    public class QuantityByInterviewersReport : IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> statuses;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public QuantityByInterviewersReport(IQueryableReadSideRepositoryReader<InterviewStatuses> statuses, IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.statuses = statuses;
            this.users = users;
        }

        public QuantityByResponsibleReportView Load(QuantityByInterviewersReportInputModel input)
        {
            var from = input.From.AddDays(1).Date;
            var to = AddPeriod(input.From, input.Period, -input.ColumnCount);

            var dateTimeRanges =
                Enumerable.Range(0, input.ColumnCount)
                    .Select(
                        i =>
                            new DateTimeRange(AddPeriod(from, input.Period, -(i + 1)).Date.AddSeconds(-1),
                                AddPeriod(from, input.Period, -i).Date.AddSeconds(-1)))
                    .ToArray();

            var usersCount = users.Query(
                _ =>
                    _.Count(
                        u =>
                            u.Supervisor.Id == input.SupervisorId
                            && u.Roles.Contains(UserRoles.Operator)));

            var userDetails = users.Query(_ => _.Where(
                u =>
                    u.Supervisor.Id == input.SupervisorId
                    && u.Roles.Contains(UserRoles.Operator))
                .OrderBy(u => u.UserName)
                .Select(u => new {InterviewerId = u.PublicKey, InterviewerName = u.UserName})
                .Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize).ToArray());

            var userIds = userDetails.Select(u => u.InterviewerId).ToArray();
            
            var allInterviewsInStatus = statuses.Query(
                _ =>
                    _.Where(
                        x =>
                            x.QuestionnaireId == input.QuestionnaireId &&
                            x.QuestionnaireVersion == input.QuestionnaireVersion)
                        .SelectMany(x => x.InterviewCommentedStatuses)
                        .Where(
                            ics =>
                                ics.Timestamp < from && ics.Timestamp > to.Date
                                && ics.Status == input.InterviewStatus
                                && ics.InterviewerId.HasValue
                                && userIds.Contains(ics.InterviewerId.Value))
                        .Select(i => new {InterviewerId = i.InterviewerId.Value, i.Timestamp})
                        .ToArray()
                );

            var rows = userDetails.Select(u =>
            {
                var interviewsForUser = allInterviewsInStatus.Where(i => i.InterviewerId == u.InterviewerId).ToArray();
                var quantityByPeriod = new List<long>();

                foreach (var dateTimeRange in dateTimeRanges)
                {
                    var count =
                        interviewsForUser.Count(
                            ics => ics.Timestamp.Date >= dateTimeRange.From && ics.Timestamp.Date < dateTimeRange.To);
                    quantityByPeriod.Add(count);
                }
                return new QuantityByInterviewersReportRow(u.InterviewerId, quantityByPeriod.ToArray(),
                    u.InterviewerName, interviewsForUser.Count());
            }).ToArray();

            return new QuantityByResponsibleReportView(rows, dateTimeRanges, usersCount);
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
    }
}
