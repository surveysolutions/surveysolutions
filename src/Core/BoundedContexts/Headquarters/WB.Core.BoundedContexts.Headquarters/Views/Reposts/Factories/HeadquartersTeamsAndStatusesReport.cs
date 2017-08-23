using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class HeadquartersTeamsAndStatusesReport : AbstractTeamsAndStatusesReport, IHeadquartersTeamsAndStatusesReport
    {
        private readonly INativeReadSideStorage<InterviewSummary> interviewsReader;

        public HeadquartersTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader)
        {
            this.interviewsReader = interviewsReader;
        }

        public override TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input)
        {
            var query = this.interviewsReader.Query(_ => FilterByQuestionnaireOrTeamLead(_, input).Select(x => new
                {
                    x.TeamLeadId,
                    x.TeamLeadName,
                    SupervisorAssigned = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                    InterviewerAssigned = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                    Completed = x.Status == InterviewStatus.Completed ? 1 : 0,
                    RejectedBySupervisor = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                    ApprovedBySupervisor = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                    RejectedByHeadquarters = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                    ApprovedByHeadquarters = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                })
                .GroupBy(x => new { x.TeamLeadName, x.TeamLeadId })
                .Select(x => new 
                {
                    ResponsibleId = x.Key.TeamLeadId,
                    Responsible = x.Key.TeamLeadName,
                    SupervisorAssignedCount = (int?) x.Sum(y => y.SupervisorAssigned) ?? 0,
                    InterviewerAssignedCount = (int?)x.Sum(y => y.InterviewerAssigned) ?? 0,
                    CompletedCount = (int?)x.Sum(y => y.Completed) ?? 0,
                    RejectedBySupervisorCount = (int?)x.Sum(y => y.RejectedBySupervisor) ?? 0,
                    ApprovedBySupervisorCount = (int?)x.Sum(y => y.ApprovedBySupervisor) ?? 0,
                    RejectedByHeadquartersCount = (int?)x.Sum(y => y.RejectedByHeadquarters) ?? 0,
                    ApprovedByHeadquartersCount = (int?)x.Sum(y => y.ApprovedByHeadquarters) ?? 0,
                    TotalCount = x.Count()
                }));

            var totalRaw = this.interviewsReader.Query(_ =>
                    FilterByQuestionnaireOrTeamLead(_, input)
                    .Select(x => new
                    {
                        SupervisorAssigned = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                        InterviewerAssigned = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                        Completed = x.Status == InterviewStatus.Completed ? 1 : 0,
                        RejectedBySupervisor = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                        ApprovedBySupervisor = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                        RejectedByHeadquarters = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                        ApprovedByHeadquarters = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                    })
                    .GroupBy(x => 1)
                    .Select(x => new
                    {
                        SupervisorAssignedCount = (int?)x.Sum(y => y.SupervisorAssigned) ?? 0,
                        InterviewerAssignedCount = (int?)x.Sum(y => y.InterviewerAssigned) ?? 0,
                        CompletedCount = (int?)x.Sum(y => y.Completed) ?? 0,
                        RejectedBySupervisorCount = (int?)x.Sum(y => y.RejectedBySupervisor) ?? 0,
                        ApprovedBySupervisorCount = (int?)x.Sum(y => y.ApprovedBySupervisor) ?? 0,
                        RejectedByHeadquartersCount = (int?)x.Sum(y => y.RejectedByHeadquarters) ?? 0,
                        ApprovedByHeadquartersCount = (int?)x.Sum(y => y.ApprovedByHeadquarters) ?? 0,
                        TotalCount = x.Count()
                    }))
                .ToList().FirstOrDefault();

            var totalStatistics = new TeamsAndStatusesReportLine
            {
                SupervisorAssignedCount = totalRaw?.SupervisorAssignedCount ?? 0,
                InterviewerAssignedCount = totalRaw?.InterviewerAssignedCount ?? 0,
                CompletedCount = totalRaw?.CompletedCount ?? 0,
                RejectedBySupervisorCount = totalRaw?.RejectedBySupervisorCount ?? 0,
                ApprovedBySupervisorCount = totalRaw?.ApprovedBySupervisorCount ?? 0,
                RejectedByHeadquartersCount = totalRaw?.RejectedByHeadquartersCount ?? 0,
                ApprovedByHeadquartersCount = totalRaw?.ApprovedByHeadquartersCount ?? 0,
                TotalCount = totalRaw?.TotalCount ?? 0
            };

            var totalCount = query.ToList().Count();

            return new TeamsAndStatusesReportView
            {
                TotalCount = totalCount,
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