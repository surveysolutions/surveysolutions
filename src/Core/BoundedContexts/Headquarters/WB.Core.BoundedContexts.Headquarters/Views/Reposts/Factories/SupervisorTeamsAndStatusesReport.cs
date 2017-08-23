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
        public SupervisorTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader) : base(interviewsReader)
        {
        }

        public override TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var queryItems = this.interviewsReader.Query(_ => 
                FilterByQuestionnaireOrTeamLead(_, input)
                .Select(x => new
                {
                    ResponsibleId = x.ResponsibleId,
                    Responsible = x.ResponsibleName,
                    SupervisorAssignedCount = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                    InterviewerAssignedCount = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                    CompletedCount = x.Status == InterviewStatus.Completed ? 1 : 0,
                    RejectedBySupervisorCount = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                    ApprovedBySupervisorCount = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                    RejectedByHeadquartersCount = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                    ApprovedByHeadquartersCount = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                })
                .GroupBy(x => new { x.Responsible, x.ResponsibleId })
                .Select(x => new
                {
                    ResponsibleId = x.Key.ResponsibleId,
                    Responsible = x.Key.Responsible,
                    SupervisorAssignedCount = (int?)x.Sum(y => y.SupervisorAssignedCount) ?? 0,
                    InterviewerAssignedCount = (int?)x.Sum(y => y.InterviewerAssignedCount) ?? 0,
                    CompletedCount = (int?)x.Sum(y => y.CompletedCount) ?? 0,
                    RejectedBySupervisorCount = (int?)x.Sum(y => y.RejectedBySupervisorCount) ?? 0,
                    ApprovedBySupervisorCount = (int?)x.Sum(y => y.ApprovedBySupervisorCount) ?? 0,
                    RejectedByHeadquartersCount = (int?)x.Sum(y => y.RejectedByHeadquartersCount) ?? 0,
                    ApprovedByHeadquartersCount = (int?)x.Sum(y => y.ApprovedByHeadquartersCount) ?? 0,
                    TotalCount = x.Count()
                }));

            var totalRow = QueryReportTotalRow(input);

            var totalCount = this.interviewsReader.CountDistinctWithRecursiveIndex(_ => _.Where(this.CreateFilterExpression(input)).Select(x => x.ResponsibleId));

            return new TeamsAndStatusesReportView
            {
                TotalCount = totalCount,
                Items = queryItems.OrderUsingSortExpression(input.Order)
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
                TotalRow = totalRow
            };
        }

    }
}