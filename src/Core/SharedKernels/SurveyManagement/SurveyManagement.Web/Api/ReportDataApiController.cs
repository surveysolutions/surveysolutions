using System;
using System.Linq;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
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

        private readonly IViewFactory<MapReportInputModel, MapReportView> mapReport;

        private readonly IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory;
  
        private readonly IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView> quantityByInterviewersReport;
        private readonly IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView> quantityBySupervisorsReport;

        private readonly IViewFactory<SpeedByInterviewersReportInputModel, SpeedByResponsibleReportView> speedByInterviewersReport;
        private readonly IViewFactory<SpeedBySupervisorsReportInputModel, SpeedByResponsibleReportView> speedBySupervisorsReport;
        private readonly IViewFactory<SpeedBetweenStatusesBySupervisorsReportInputModel, SpeedByResponsibleReportView> speedBetweenStatusesBySupervisorsReport;
        private readonly IViewFactory<SpeedBetweenStatusesByInterviewersReportInputModel, SpeedByResponsibleReportView> speedBetweenStatusesByInterviewersReport;

        public ReportDataApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            ISurveysAndStatusesReport surveysAndStatusesReport,
            IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport,
            ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IViewFactory<MapReportInputModel, MapReportView> mapReport, 
            IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView> questionInforFactory,
            IChartStatisticsViewFactory chartStatisticsViewFactory, 
            IViewFactory<QuantityByInterviewersReportInputModel, QuantityByResponsibleReportView> quantityByInterviewersReport, 
            IViewFactory<QuantityBySupervisorsReportInputModel, QuantityByResponsibleReportView> quantityBySupervisorsReport, 
            IViewFactory<SpeedByInterviewersReportInputModel, SpeedByResponsibleReportView> speedByInterviewersReport, 
            IViewFactory<SpeedBySupervisorsReportInputModel, SpeedByResponsibleReportView> speedBySupervisorsReport, 
            IViewFactory<SpeedBetweenStatusesBySupervisorsReportInputModel, SpeedByResponsibleReportView> speedBetweenStatusesBySupervisorsReport, 
            IViewFactory<SpeedBetweenStatusesByInterviewersReportInputModel, SpeedByResponsibleReportView> speedBetweenStatusesByInterviewersReport)
            : base(commandService, provider, logger)
        {
            this.surveysAndStatusesReport = surveysAndStatusesReport;
            this.headquartersTeamsAndStatusesReport = headquartersTeamsAndStatusesReport;
            this.supervisorTeamsAndStatusesReport = supervisorTeamsAndStatusesReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.mapReport = mapReport;
            this.questionInforFactory = questionInforFactory;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.quantityByInterviewersReport = quantityByInterviewersReport;
            this.quantityBySupervisorsReport = quantityBySupervisorsReport;
            this.speedByInterviewersReport = speedByInterviewersReport;
            this.speedBySupervisorsReport = speedBySupervisorsReport;
            this.speedBetweenStatusesBySupervisorsReport = speedBetweenStatusesBySupervisorsReport;
            this.speedBetweenStatusesByInterviewersReport = speedBetweenStatusesByInterviewersReport;
        }

        [HttpPost]
        public TeamsAndStatusesReportView SupervisorTeamMembersAndStatusesReport(SummaryListViewModel data)
        {
            var input = new TeamsAndStatusesInputModel
            {
                ViewerId = this.GlobalInfo.GetCurrentUser().Id,
            };


            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.TemplateId = data.Request.TemplateId;
                    input.TemplateVersion = data.Request.TemplateVersion;
                }
            }

            var result = this.supervisorTeamsAndStatusesReport.Load(input);
            return result;
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
                Items = questionnaireBrowseView.Items
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

        public QuantityByResponsibleReportView QuantityByInterviewers(QuantityByInterviewersReportModel input)
        {
            if (input.Pager != null)
            {
                input.Request.Page = input.Pager.Page;
                input.Request.PageSize = input.Pager.PageSize;
            }
            if (input.Request.SupervisorId == Guid.Empty)
            {
                input.Request.SupervisorId = this.GlobalInfo.GetCurrentUser().Id;
            }

            input.Request.InterviewStatuses =
                GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(input.Request.ReportType);

            return this.quantityByInterviewersReport.Load(input.Request);
        }

        public QuantityByResponsibleReportView QuantityBySupervisors(QuantityBySupervisorsReportModel input)
        {
            if (input.Pager != null)
            {
                input.Request.Page = input.Pager.Page;
                input.Request.PageSize = input.Pager.PageSize;
            }

            input.Request.InterviewStatuses =
                GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(input.Request.ReportType);

            return this.quantityBySupervisorsReport.Load(input.Request);
        }

        public SpeedByResponsibleReportView SpeedByInterviewers(SpeedByInterviewersReportModel input)
        {
            if (input.Pager != null)
            {
                input.Request.Page = input.Pager.Page;
                input.Request.PageSize = input.Pager.PageSize;
            }
            if (input.Request.SupervisorId == Guid.Empty)
            {
                input.Request.SupervisorId = this.GlobalInfo.GetCurrentUser().Id;
            }

            input.Request.InterviewStatuses =
             GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(input.Request.ReportType);

            return this.speedByInterviewersReport.Load(input.Request);
        }

        private InterviewExportedAction[] GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(
            PeriodiceReportType reportType)
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

        private InterviewExportedAction[] GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(
           PeriodiceReportType reportType)
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

        private InterviewExportedAction[] GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(
           PeriodiceReportType reportType)
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

        private InterviewExportedAction[] GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(
        PeriodiceReportType reportType)
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

        public SpeedByResponsibleReportView SpeedBetweenStatusesBySupervisors(SpeedBySupervisorsReportModel input)
        {
            var inputParameters = new SpeedBetweenStatusesBySupervisorsReportInputModel()
            {
                ColumnCount = input.Request.ColumnCount,
                From = input.Request.From,
                Order = input.Request.Order,
                Orders = input.Request.Orders,
                Period = input.Request.Period,
                QuestionnaireId = input.Request.QuestionnaireId,
                QuestionnaireVersion = input.Request.QuestionnaireVersion
            };

            if (input.Pager != null)
            {
                inputParameters.Page = input.Pager.Page;
                inputParameters.PageSize = input.Pager.PageSize;
            }

            inputParameters.BeginInterviewStatuses = GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.Request.ReportType);
            inputParameters.EndInterviewStatuses = GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.Request.ReportType);

            return this.speedBetweenStatusesBySupervisorsReport.Load(inputParameters);
        }

        public SpeedByResponsibleReportView SpeedBetweenStatusesByInterviewers(SpeedByInterviewersReportModel input)
        {
            var inputParameters = new SpeedBetweenStatusesByInterviewersReportInputModel()
            {
                ColumnCount = input.Request.ColumnCount,
                From = input.Request.From,
                Order = input.Request.Order,
                Orders = input.Request.Orders,
                Period = input.Request.Period,
                QuestionnaireId = input.Request.QuestionnaireId,
                QuestionnaireVersion = input.Request.QuestionnaireVersion
            };

            if (input.Pager != null)
            {
                inputParameters.Page = input.Pager.Page;
                inputParameters.PageSize = input.Pager.PageSize;
            }

            inputParameters.BeginInterviewStatuses = GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.Request.ReportType);
            inputParameters.EndInterviewStatuses = GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.Request.ReportType);

            return this.speedBetweenStatusesByInterviewersReport.Load(inputParameters);
        }

        public SpeedByResponsibleReportView SpeedBySupervisors(SpeedBySupervisorsReportModel input)
        {
            if (input.Pager != null)
            {
                input.Request.Page = input.Pager.Page;
                input.Request.PageSize = input.Pager.PageSize;
            }

            input.Request.InterviewStatuses =
                GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(input.Request.ReportType);

            return this.speedBySupervisorsReport.Load(input.Request);
        }

        [HttpPost]
        public SurveysAndStatusesReportView SupervisorSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new SurveysAndStatusesReportInputModel { TeamLeadId = this.GlobalInfo.GetCurrentUser().Id };

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null && data.Request.UserId != input.TeamLeadId)
                {
                    input.ResponsibleId = data.Request.UserId;
                }
            }

            return this.surveysAndStatusesReport.Load(input);
        }

        [HttpPost]
        public TeamsAndStatusesReportView HeadquarterSupervisorsAndStatusesReport(SummaryListViewModel data)
        {
            var input = new TeamsAndStatusesInputModel();

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.TemplateId = data.Request.TemplateId;
                    input.TemplateVersion = data.Request.TemplateVersion;
                }
            }

            return this.headquartersTeamsAndStatusesReport.Load(input);
        }

        [HttpPost]
        public SurveysAndStatusesReportView HeadquarterSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new SurveysAndStatusesReportInputModel();

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.TeamLeadId = data.Request.UserId;
                }
            }

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
    }
}