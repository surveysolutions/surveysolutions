using System;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api  
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class ReportDataApiController : BaseApiController
    {
        private readonly IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport;
        private readonly ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport;

        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        private readonly ISurveysAndStatusesReport surveysAndStatusesReport;

        private readonly IChartStatisticsViewFactory chartStatisticsViewFactory;

        private readonly IMapReport mapReport;

        private readonly IQuestionnaireQuestionInfoFactory questionInforFactory;
  
        private readonly IQuantityReportFactory quantityReport;

        private readonly ISpeedReportFactory speedReport;

        public ReportDataApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            ISurveysAndStatusesReport surveysAndStatusesReport,
            IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport,
            ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IMapReport mapReport, 
            IQuestionnaireQuestionInfoFactory questionInforFactory,
            IChartStatisticsViewFactory chartStatisticsViewFactory, 
            IQuantityReportFactory quantityReport, 
            ISpeedReportFactory speedReport)
            : base(commandService, provider, logger)
        {
            this.surveysAndStatusesReport = surveysAndStatusesReport;
            this.headquartersTeamsAndStatusesReport = headquartersTeamsAndStatusesReport;
            this.supervisorTeamsAndStatusesReport = supervisorTeamsAndStatusesReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.mapReport = mapReport;
            this.questionInforFactory = questionInforFactory;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.quantityReport = quantityReport;
            this.speedReport = speedReport;
        }

        [HttpPost]
        public TeamsAndStatusesReportView SupervisorTeamMembersAndStatusesReport(SummaryListViewModel data)
        {
            var input = new TeamsAndStatusesInputModel
            {
                ViewerId = this.GlobalInfo.GetCurrentUser().Id,
                Orders = data.SortOrder,
                Page = data.PageIndex,
                PageSize = data.PageSize,
                TemplateId = data.TemplateId,
                TemplateVersion = data.TemplateVersion
            };

            return this.supervisorTeamsAndStatusesReport.Load(input);
        }

        [HttpPost]
        public MapReportView MapReport(MapReportInputModel data)
        {
            return this.mapReport.Load(data);
        }

        [HttpPost]
        public QuestionnaireAndVersionsView Questionnaires(QuestionnaireBrowseInputModel input)
        {
            QuestionnaireBrowseView questionnaireBrowseView = this.questionnaireBrowseViewFactory.Load(input);
            var result = new QuestionnaireAndVersionsView
            {
                Items = questionnaireBrowseView.Items.GroupBy(x => x.QuestionnaireId).Select(x => x.First())
                                               .Select(x => new QuestionnaireAndVersionsItem {
                                                        QuestionnaireId = x.QuestionnaireId,
                                                        Title = x.Title,
                                                        Versions = questionnaireBrowseView.Items
                                                                                          .Where(q => q.QuestionnaireId == x.QuestionnaireId)
                                                                                          .Select(y => y.Version)
                                                                                          .ToArray()
                                                    }).ToArray(),
                TotalCount = questionnaireBrowseView.TotalCount
            };
            return result;
        }

        public QuestionnaireQuestionInfoView QuestionInfo(QuestionnaireQuestionInfoInputModel input)
        {
            return this.questionInforFactory.Load(input);
        }

        public QuantityByResponsibleReportView QuantityByInterviewers(QuantityByInterviewersReportModel data)
        {

            var input = new QuantityByInterviewersReportInputModel
            {
               SupervisorId = data.SupervisorId ?? this.GlobalInfo.GetCurrentUser().Id,
               InterviewStatuses = GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(data.ReportType),
               Page = data.PageIndex,
               PageSize = data.PageSize,
               QuestionnaireVersion = data.QuestionnaireVersion,
               QuestionnaireId = data.QuestionnaireId,
               ColumnCount = data.ColumnCount,
               From = data.From,
               Orders = data.SortOrder ?? Enumerable.Empty<OrderRequestItem>(),
               Period = data.Period,
               ReportType = data.ReportType
            }; 

            return this.quantityReport.Load(input);
        }

        public QuantityByResponsibleReportView QuantityBySupervisors(QuantityBySupervisorsReportModel data)
        {
            var input = new QuantityBySupervisorsReportInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                InterviewStatuses = GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(data.ReportType),
                Period = data.Period,
                ReportType = data.ReportType,
                QuestionnaireVersion = data.QuestionnaireVersion,
                QuestionnaireId = data.QuestionnaireId,
                ColumnCount = data.ColumnCount,
                From = data.From,
                Orders = data.SortOrder
            };

            return this.quantityReport.Load(input);
        }

        public SpeedByResponsibleReportView SpeedByInterviewers(SpeedByInterviewersReportModel data)
        {
            var input = new SpeedByInterviewersReportInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                SupervisorId = data.SupervisorId ?? this.GlobalInfo.GetCurrentUser().Id,
                InterviewStatuses = GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(data.ReportType),
                QuestionnaireVersion = data.QuestionnaireVersion,
                QuestionnaireId = data.QuestionnaireId,
                ColumnCount = data.ColumnCount,
                From = data.From,
                ReportType = data.ReportType,
                Period = data.Period,
                Orders = data.SortOrder
            };

            return this.speedReport.Load(input);
        }

        public SpeedByResponsibleReportView SpeedBetweenStatusesBySupervisors(SpeedBySupervisorsReportModel input)
        {
            var inputParameters = new SpeedBetweenStatusesBySupervisorsReportInputModel
            {
                Page = input.PageIndex,
                PageSize = input.PageSize,
                ColumnCount = input.ColumnCount,
                From = input.From,
                Orders = input.SortOrder,
                Period = input.Period,
                QuestionnaireId = input.QuestionnaireId,
                QuestionnaireVersion = input.QuestionnaireVersion,
                BeginInterviewStatuses = GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType),
                EndInterviewStatuses = GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType)
            };

            return this.speedReport.Load(inputParameters);
        }

        public SpeedByResponsibleReportView SpeedBetweenStatusesByInterviewers(SpeedByInterviewersReportModel input)
        {
            var inputParameters = new SpeedBetweenStatusesByInterviewersReportInputModel
            {
                Page = input.PageIndex,
                PageSize = input.PageSize,
                ColumnCount = input.ColumnCount,
                From = input.From,
                Orders = input.SortOrder,
                Period = input.Period,
                QuestionnaireId = input.QuestionnaireId,
                QuestionnaireVersion = input.QuestionnaireVersion,
                BeginInterviewStatuses = GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType),
                EndInterviewStatuses = GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType),
                SupervisorId = input.SupervisorId ?? this.GlobalInfo.GetCurrentUser().Id
            };

            return this.speedReport.Load(inputParameters);
        }

        public SpeedByResponsibleReportView SpeedBySupervisors(SpeedBySupervisorsReportModel data)
        {
            var input = new SpeedBySupervisorsReportInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                InterviewStatuses = GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(data.ReportType),
                ColumnCount = data.ColumnCount,
                From = data.From,
                QuestionnaireVersion = data.QuestionnaireVersion,
                QuestionnaireId = data.QuestionnaireId,
                ReportType = data.ReportType,
                Period = data.Period,
                Orders = data.SortOrder
            };

            return this.speedReport.Load(input);
        }

        [HttpPost]
        public SurveysAndStatusesReportView SupervisorSurveysAndStatusesReport(SurveysAndStatusesReportRequest request)
        {
            var teamLeadName = this.GlobalInfo.GetCurrentUser().Name;

            var input = new SurveysAndStatusesReportInputModel
            {
                TeamLeadName = teamLeadName,
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.SortOrder,
                ResponsibleName = request.ResponsibleName == teamLeadName ? null : request.ResponsibleName 
            };

            return this.surveysAndStatusesReport.Load(input);
        }

        [HttpPost]
        public TeamsAndStatusesReportView HeadquarterSupervisorsAndStatusesReport(SummaryListViewModel data)
        {
            var input = new TeamsAndStatusesInputModel
            {
                Orders = data.SortOrder,
                Page = data.PageIndex,
                PageSize = data.PageSize,
                TemplateId = data.TemplateId,
                TemplateVersion = data.TemplateVersion
            };

            return this.headquartersTeamsAndStatusesReport.Load(input);
        }

        [HttpPost]
        public SurveysAndStatusesReportView HeadquarterSurveysAndStatusesReport(SurveysAndStatusesReportRequest data)
        {
            var input = new SurveysAndStatusesReportInputModel
            {
                Orders = data.SortOrder,
                Page = data.PageIndex,
                PageSize = data.PageSize,
                TeamLeadName = data.ResponsibleName
            };

            return this.surveysAndStatusesReport.Load(input);
        }

        [HttpPost]
        public ChartStatisticsView ChartStatistics(InterviewsStatisticsInputModel data)
        {
            var input = new ChartStatisticsInputModel
            {
                QuestionnaireId = data.TemplateId,
                QuestionnaireVersion = data.TemplateVersion,
                CurrentDate = DateTime.Now,
                From = data.From,
                To = data.To
            };

            return this.chartStatisticsViewFactory.Load(input);
        }

        private InterviewExportedAction[] GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(PeriodiceReportType reportType)
        {
            switch (reportType)
            {
                case PeriodiceReportType.NumberOfInterviewTransactionsBySupervisor:
                    return new[] { InterviewExportedAction.ApprovedByHeadquarter, InterviewExportedAction.RejectedBySupervisor };
                case PeriodiceReportType.NumberOfCompletedInterviews:
                    return new[] { InterviewExportedAction.Completed };
                case PeriodiceReportType.NumberOfInterviewTransactionsByHQ:
                    return new[] { InterviewExportedAction.ApprovedByHeadquarter, InterviewExportedAction.RejectedByHeadquarter };
                case PeriodiceReportType.NumberOfInterviewsApprovedByHQ:
                    return new[] { InterviewExportedAction.ApprovedByHeadquarter };
            }
            return new InterviewExportedAction[0];
        }

        private InterviewExportedAction[] GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(PeriodiceReportType reportType)
        {
            switch (reportType)
            {
                case PeriodiceReportType.AverageInterviewDuration:
                    return new[] { InterviewExportedAction.Completed };
                case PeriodiceReportType.AverageSupervisorProcessingTime:
                    return new[] { InterviewExportedAction.ApprovedBySupervisor, InterviewExportedAction.RejectedBySupervisor };
                case PeriodiceReportType.AverageHQProcessingTime:
                    return new[] { InterviewExportedAction.ApprovedByHeadquarter, InterviewExportedAction.RejectedByHeadquarter };
            }
            return new InterviewExportedAction[0];
        }

        private InterviewExportedAction[] GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(PeriodiceReportType reportType)
        {
            switch (reportType)
            {
                case PeriodiceReportType.AverageOverallCaseProcessingTime:
                    return new[] { InterviewExportedAction.InterviewerAssigned };
                case PeriodiceReportType.AverageCaseAssignmentDuration:
                    return new[] { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };
            }
            return new InterviewExportedAction[0];
        }

        private InterviewExportedAction[] GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(PeriodiceReportType reportType)
        {
            switch (reportType)
            {
                case PeriodiceReportType.AverageOverallCaseProcessingTime:
                    return new[] { InterviewExportedAction.ApprovedByHeadquarter };
                case PeriodiceReportType.AverageCaseAssignmentDuration:
                    return new[] { InterviewExportedAction.Completed };
            }
            return new InterviewExportedAction[0];
        }
    }
}