using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class SupervisorTeamsAndStatusesReport : AbstractTeamsAndStatusesReport, ISupervisorTeamsAndStatusesReport
    {
        private readonly INativeReadSideStorage<InterviewSummary> interviewsReader;

        public SupervisorTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        public override TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var query = this.interviewsReader.Query(_ =>
            {
                if (input.TemplateId.HasValue)
                {
                    _ = _.Where(x => x.QuestionnaireId == input.TemplateId);
                }

                if (input.TemplateVersion.HasValue)
                {
                    _ = _.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
                }

                if (input.ViewerId.HasValue)
                {
                    _ = _.Where(x => x.TeamLeadId == input.ViewerId);
                }

                return _.Select(x => new
                {
                    x.ResponsibleId,
                    x.ResponsibleName,
                    SupervisorAssigned = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                    InterviewerAssigned = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                    Completed = x.Status == InterviewStatus.Completed ? 1 : 0,
                    RejectedBySupervisor = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                    ApprovedBySupervisor = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                    RejectedByHeadquarters = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                    ApprovedByHeadquarters = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                })
                .GroupBy(x => new { x.ResponsibleName, x.ResponsibleId })
                .Select(x => new
                {
                    ResponsibleId = x.Key.ResponsibleId,
                    Responsible = x.Key.ResponsibleName,
                    SupervisorAssignedCount = x.Sum(y => y.SupervisorAssigned),
                    InterviewerAssignedCount = x.Sum(y => y.InterviewerAssigned),
                    CompletedCount = x.Sum(y => y.Completed),
                    RejectedBySupervisorCount = x.Sum(y => y.RejectedBySupervisor),
                    ApprovedBySupervisorCount = x.Sum(y => y.ApprovedBySupervisor),
                    RejectedByHeadquartersCount = x.Sum(y => y.RejectedByHeadquarters),
                    ApprovedByHeadquartersCount = x.Sum(y => y.ApprovedByHeadquarters),
                    TotalCount = x.Count()
                });
            });

            var totalStatistics = new TeamsAndStatusesReportLine();

            return new TeamsAndStatusesReportView
            {
                TotalCount = query.Count(),
                Items = query.OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList()
                    .Select(x => new TeamsAndStatusesReportLine
                    {
                        ResponsibleId = x.ResponsibleId,
                        Responsible = x.Responsible,
                        SupervisorAssignedCount = x.SupervisorAssignedCount,
                        InterviewerAssignedCount = x.InterviewerAssignedCount,
                        CompletedCount = x.CompletedCount,
                        RejectedBySupervisorCount = x.RejectedBySupervisorCount,
                        ApprovedBySupervisorCount = x.ApprovedBySupervisorCount,
                        RejectedByHeadquartersCount = x.RejectedByHeadquartersCount,
                        ApprovedByHeadquartersCount = x.ApprovedByHeadquartersCount,
                        TotalCount = x.TotalCount
                    }),
                TotalRow = totalStatistics
            };
        }
    }
}