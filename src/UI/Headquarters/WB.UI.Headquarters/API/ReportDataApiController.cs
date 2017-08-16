using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reports;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Models.ComponentModels;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api  
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public partial class ReportDataApiController : BaseApiController
    {
        private readonly IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport;
        private readonly ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ISurveysAndStatusesReport surveysAndStatusesReport;
        private readonly IChartStatisticsViewFactory chartStatisticsViewFactory;
        private readonly IMapReport mapReport;
        private readonly IQuantityReportFactory quantityReport;
        private readonly ISpeedReportFactory speedReport;

        private readonly ICountDaysOfInterviewInStatusReport countDaysOfInterviewInStatusReport;
        private readonly IDeviceInterviewersReport deviceInterviewersReport;
        private readonly IExportFactory exportFactory;

        public ReportDataApiController(
            ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            ISurveysAndStatusesReport surveysAndStatusesReport,
            IHeadquartersTeamsAndStatusesReport headquartersTeamsAndStatusesReport,
            ISupervisorTeamsAndStatusesReport supervisorTeamsAndStatusesReport,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IMapReport mapReport,
            IChartStatisticsViewFactory chartStatisticsViewFactory, 
            IQuantityReportFactory quantityReport, 
            ISpeedReportFactory speedReport,
            ICountDaysOfInterviewInStatusReport countDaysOfInterviewInStatusReport,
            IDeviceInterviewersReport deviceInterviewersReport,
            IExportFactory exportFactory)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.surveysAndStatusesReport = surveysAndStatusesReport;
            this.headquartersTeamsAndStatusesReport = headquartersTeamsAndStatusesReport;
            this.supervisorTeamsAndStatusesReport = supervisorTeamsAndStatusesReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.mapReport = mapReport;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.quantityReport = quantityReport;
            this.speedReport = speedReport;
            this.countDaysOfInterviewInStatusReport = countDaysOfInterviewInStatusReport;
            this.deviceInterviewersReport = deviceInterviewersReport;
            this.exportFactory = exportFactory;
        }

        [HttpPost]
        [CamelCase]
        public TeamsAndStatusesReportResponse SupervisorTeamMembersAndStatusesReport(TeamsAndStatusesFilter teamsAndStatusesFilter)
        {
            var input = new TeamsAndStatusesInputModel
            {
                ViewerId = this.authorizedUser.Id,
                Orders = teamsAndStatusesFilter.GetSortOrderRequestItems(),
                Page = teamsAndStatusesFilter.PageIndex,
                PageSize = teamsAndStatusesFilter.PageSize,
                TemplateId = teamsAndStatusesFilter.TemplateId,
                TemplateVersion = teamsAndStatusesFilter.TemplateVersion
            };

            var view = this.supervisorTeamsAndStatusesReport.Load(input);
            return new TeamsAndStatusesReportResponse
            {
                Draw = teamsAndStatusesFilter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalRow = view.TotalRow
            };
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

        [HttpGet]
        public ComboboxModel QuestionInfo(string questionnaireId)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);
            var variables = this.mapReport.GetVariablesForQuestionnaire(questionnaireIdentity);
            return new ComboboxModel(variables.Select(x => new ComboboxOptionModel(x, x)).ToArray(), variables.Count);
        }

        public QuantityByResponsibleReportView QuantityByInterviewers(QuantityByInterviewersReportModel data)
        {
            var input = new QuantityByInterviewersReportInputModel
            {
               SupervisorId = data.SupervisorId ?? this.authorizedUser.Id,
               InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(data.ReportType),
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
                InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(data.ReportType),
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
                SupervisorId = data.SupervisorId ?? this.authorizedUser.Id,
                InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(data.ReportType),
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
                BeginInterviewStatuses = this.GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType),
                EndInterviewStatuses = this.GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType)
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
                BeginInterviewStatuses = this.GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType),
                EndInterviewStatuses = this.GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(input.ReportType),
                SupervisorId = input.SupervisorId ?? this.authorizedUser.Id
            };

            return this.speedReport.Load(inputParameters);
        }

        public SpeedByResponsibleReportView SpeedBySupervisors(SpeedBySupervisorsReportModel data)
        {
            var input = new SpeedBySupervisorsReportInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(data.ReportType),
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
        [CamelCase]
        public DataTableResponse<TeamsAndStatusesReportLine> HeadquarterSupervisorsAndStatusesReport(TeamsAndStatusesFilter teamsAndStatusesFilter)
        {
            var input = new TeamsAndStatusesInputModel
            {
                Orders = teamsAndStatusesFilter.GetSortOrderRequestItems(),
                Page = teamsAndStatusesFilter.PageIndex,
                PageSize = teamsAndStatusesFilter.PageSize,
                TemplateId = teamsAndStatusesFilter.TemplateId,
                TemplateVersion = teamsAndStatusesFilter.TemplateVersion
            };

            var view = this.headquartersTeamsAndStatusesReport.Load(input);
            return new TeamsAndStatusesReportResponse
            {
                Draw = teamsAndStatusesFilter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalRow = view.TotalRow
            };
        }

        [HttpPost]
        [CamelCase]
        public SurveysAndStatusesDataTableResponse SupervisorSurveysAndStatusesReport(SurveysAndStatusesFilter surveysAndStatusesFilter)
        {
            var teamLeadName = this.authorizedUser.UserName;

            var view = this.surveysAndStatusesReport.Load(new SurveysAndStatusesReportInputModel
            {
                TeamLeadName = teamLeadName,
                Page = surveysAndStatusesFilter.PageIndex,
                PageSize = surveysAndStatusesFilter.PageSize,
                Orders = surveysAndStatusesFilter.GetSortOrderRequestItems(),
                ResponsibleName = surveysAndStatusesFilter.ResponsibleName == teamLeadName ? null : surveysAndStatusesFilter.ResponsibleName
            });

            return new SurveysAndStatusesDataTableResponse
            {
                Draw = surveysAndStatusesFilter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalResponsibleCount = view.TotalResponsibleCount,
                TotalInterviewCount = view.TotalInterviewCount
            };
        }

        [HttpPost]
        [CamelCase]
        public SurveysAndStatusesDataTableResponse HeadquarterSurveysAndStatusesReport(SurveysAndStatusesFilter surveysAndStatusesFilter)
        {
            var view = this.surveysAndStatusesReport.Load(new SurveysAndStatusesReportInputModel
            {
                Orders = surveysAndStatusesFilter.GetSortOrderRequestItems(),
                Page = surveysAndStatusesFilter.PageIndex,
                PageSize = surveysAndStatusesFilter.PageSize,
                TeamLeadName = surveysAndStatusesFilter.ResponsibleName
            });

            return new SurveysAndStatusesDataTableResponse
            {
                Draw = surveysAndStatusesFilter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalResponsibleCount = view.TotalResponsibleCount,
                TotalInterviewCount = view.TotalInterviewCount
            };
        }


        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public async Task<DeviceInterviewersDataTableResponse> DeviceInterviewers([FromUri]DeviceInterviewersFilter request)
        {
            var data = await this.deviceInterviewersReport.LoadAsync(new DeviceByInterviewersReportInputModel
            {
                Filter = request.Search.Value,
                Orders = request.GetSortOrderRequestItems(),
                Page = request.Start,
                PageSize = request.Length
            });

            return new DeviceInterviewersDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = data.TotalCount,
                RecordsFiltered = data.TotalCount,
                Data = data.Items
            };
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

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [CamelCase]
        public async Task<CountDaysOfInterviewInStatusDataTableResponse> CountDaysOfInterviewInStatus([FromUri] CountDaysOfInterviewInStatusRequest request)
        {
            var input = new CountDaysOfInterviewInStatusInputModel();

            if (!string.IsNullOrEmpty(request.QuestionnaireId))
            {
                var questionnaireIdentity = QuestionnaireIdentity.Parse(request.QuestionnaireId);
                input.TemplateVersion = questionnaireIdentity.Version;
                input.TemplateId = questionnaireIdentity.QuestionnaireId;
            }

            var data = await this.countDaysOfInterviewInStatusReport.LoadAsync(input);
            return new CountDaysOfInterviewInStatusDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = data.Length,
                RecordsFiltered = data.Length,
                Data = data
            };
        }

        public class SurveysAndStatusesDataTableResponse : DataTableResponse<HeadquarterSurveysAndStatusesReportLine>
        {
            public int TotalInterviewCount { get; set; }
            public int TotalResponsibleCount { get; set; }
        }


        public class CountDaysOfInterviewInStatusDataTableResponse : DataTableResponse<CountDaysOfInterviewInStatusRow>
        {
        }

        public class DeviceInterviewersDataTableResponse : DataTableResponse<DeviceInterviewersReportLine>
        {
        }

        public class CountDaysOfInterviewInStatusRequest : DataTableRequest
        {
            public string QuestionnaireId { get; set; }
        }

        public class DeviceInterviewersFilter : DataTableRequest
        {
            
        }

        public class SurveysAndStatusesFilter : DataTableRequest
        {
            public string ResponsibleName { get; set; }
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