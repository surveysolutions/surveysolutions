using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class QuantityBySupervisorsReport : IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> statuses;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public QuantityBySupervisorsReport(
            IQueryableReadSideRepositoryReader<InterviewStatuses> statuses, 
            IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.statuses = statuses;
            this.users = users;
        }

        public QuantityByResponsibleReportView Load(QuantityBySupervisorsReportInputModel input)
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
                            u.Roles.Contains(UserRoles.Supervisor)));

            var userDetails = users.Query(_ => _.Where(
                u => u.Roles.Contains(UserRoles.Supervisor))
                .OrderBy(u => u.UserName)
                .Select(u => new { SupervisorId = u.PublicKey, SupervisorName = u.UserName })
                .Skip((input.Page - 1)*input.PageSize)
                .Take(input.PageSize).ToArray());

            var userIds = userDetails.Select(u => u.SupervisorId).ToArray();
            
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
                                && ics.SupervisorId.HasValue
                                && userIds.Contains(ics.SupervisorId.Value))
                        .Select(i => new { SupervisorId = i.SupervisorId.Value, i.Timestamp })
                        .ToArray()
                );

            var rows = userDetails.Select(u =>
            {
                var interviewsForUser = allInterviewsInStatus.Where(i => i.SupervisorId == u.SupervisorId).ToArray();
                var quantityByPeriod = new List<long>();

                foreach (var dateTimeRange in dateTimeRanges)
                {
                    var count =
                        interviewsForUser.Count(
                            ics => ics.Timestamp.Date >= dateTimeRange.From && ics.Timestamp.Date < dateTimeRange.To);
                    quantityByPeriod.Add(count);
                }
                return new QuantityByInterviewersReportRow(u.SupervisorId, quantityByPeriod.ToArray(),
                    u.SupervisorName, interviewsForUser.Count());
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
