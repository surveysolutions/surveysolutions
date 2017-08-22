using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class SurveysAndStatusesReport : ISurveysAndStatusesReport
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public SurveysAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public SurveysAndStatusesReportView Load(SurveysAndStatusesReportInputModel input)
        {
            var query = this.interviewSummaryReader.Query(_ =>
            {
                if (!string.IsNullOrWhiteSpace(input.ResponsibleName))
                    _ = _.Where(x => x.ResponsibleName.ToLower() == input.ResponsibleName.ToLower());

                if (!string.IsNullOrWhiteSpace(input.TeamLeadName))
                    _ = _.Where(x => x.TeamLeadName.ToLower() == input.TeamLeadName.ToLower());

                return _.Select(x => new
                    {
                        x.QuestionnaireId,
                        x.QuestionnaireVersion,
                        x.QuestionnaireTitle,
                        x.ResponsibleName,
                        x.TeamLeadName,
                        SupervisorAssigned = x.Status == InterviewStatus.SupervisorAssigned ? 1 : 0,
                        InterviewerAssigned = x.Status == InterviewStatus.InterviewerAssigned ? 1 : 0,
                        Completed = x.Status == InterviewStatus.Completed ? 1 : 0,
                        RejectedBySupervisor = x.Status == InterviewStatus.RejectedBySupervisor ? 1 : 0,
                        ApprovedBySupervisor = x.Status == InterviewStatus.ApprovedBySupervisor ? 1 : 0,
                        RejectedByHeadquarters = x.Status == InterviewStatus.RejectedByHeadquarters ? 1 : 0,
                        ApprovedByHeadquarters = x.Status == InterviewStatus.ApprovedByHeadquarters ? 1 : 0
                    })
                    .GroupBy(x => new {x.QuestionnaireId, x.QuestionnaireVersion, x.QuestionnaireTitle})
                    .Select(x => new 
                    {
                        QuestionnaireId = x.Key.QuestionnaireId,
                        QuestionnaireVersion = x.Key.QuestionnaireVersion,
                        QuestionnaireTitle = x.Key.QuestionnaireTitle,
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

            return new SurveysAndStatusesReportView
            {
                TotalCount = query.Count(),
                Items = query.OrderUsingSortExpression(input.Order).Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize).ToList().Select(x=> new HeadquarterSurveysAndStatusesReportLine
                    {
                        QuestionnaireId = x.QuestionnaireId,
                        QuestionnaireVersion = x.QuestionnaireVersion,
                        QuestionnaireTitle = x.QuestionnaireTitle,
                        SupervisorAssignedCount = x.SupervisorAssignedCount,
                        InterviewerAssignedCount = x.InterviewerAssignedCount,
                        CompletedCount = x.CompletedCount,
                        RejectedBySupervisorCount = x.RejectedBySupervisorCount,
                        ApprovedBySupervisorCount = x.ApprovedBySupervisorCount,
                        RejectedByHeadquartersCount = x.RejectedByHeadquartersCount,
                        ApprovedByHeadquartersCount = x.ApprovedByHeadquartersCount,
                        TotalCount = x.TotalCount
                    })
            };
        }

        public ReportView GetReport(SurveysAndStatusesReportInputModel model)
        {
            var view = this.Load(model);

            return new ReportView
            {
                Headers = new[]
                {
                    Report.COLUMN_TEMPLATE_VERSION, Report.COLUMN_QUESTIONNAIRE_TEMPLATE, Report.COLUMN_SUPERVISOR_ASSIGNED, Report.COLUMN_INTERVIEWER_ASSIGNED,
                    Report.COLUMN_COMPLETED, Report.COLUMN_REJECTED_BY_SUPERVISOR, Report.COLUMN_APPROVED_BY_SUPERVISOR, Report.COLUMN_REJECTED_BY_HQ, Report.COLUMN_APPROVED_BY_HQ,
                    Report.COLUMN_TOTAL
                },
                Data = view.Items.Select(x => new object[]
                {
                    x.QuestionnaireVersion, x.QuestionnaireTitle, x.SupervisorAssignedCount, x.InterviewerAssignedCount,
                    x.CompletedCount, x.RejectedBySupervisorCount, x.ApprovedBySupervisorCount,
                    x.RejectedByHeadquartersCount, x.ApprovedByHeadquartersCount,
                    x.TotalCount
                }).ToArray()
            };
        }
    }
}
