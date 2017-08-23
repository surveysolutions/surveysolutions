using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal abstract class AbstractTeamsAndStatusesReport
    {
        public abstract TeamsAndStatusesReportView Load(TeamsAndStatusesInputModel input);

        public ReportView GetReport(TeamsAndStatusesInputModel model)
        {
            var view = this.Load(model);
            view.TotalRow.Responsible = Report.COLUMN_TOTAL;

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_TEAM_MEMBER, Report.COLUMN_SUPERVISOR_ASSIGNED, Report.COLUMN_INTERVIEWER_ASSIGNED,
                    Report.COLUMN_COMPLETED, Report.COLUMN_REJECTED_BY_SUPERVISOR,
                    Report.COLUMN_APPROVED_BY_SUPERVISOR, Report.COLUMN_REJECTED_BY_HQ, Report.COLUMN_APPROVED_BY_HQ,
                },
                Data = new[] {view.TotalRow}.Concat(view.Items).Select(x => new object[]
                {
                    x.Responsible, x.SupervisorAssignedCount, x.InterviewerAssignedCount, x.CompletedCount,
                    x.RejectedBySupervisorCount, x.ApprovedBySupervisorCount, x.RejectedByHeadquartersCount,
                    x.ApprovedByHeadquartersCount, x.TotalCount
                }).ToArray()
            };
        }
    }
}