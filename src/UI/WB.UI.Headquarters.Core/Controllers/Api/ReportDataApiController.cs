using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reports;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api  
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    [ResponseCache(NoStore = true)]
    public partial class ReportDataApiController : ControllerBase
    {
        const int MaxPageSize = 50000;

        private readonly ITeamsAndStatusesReport teamsAndStatusesReport;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ISurveysAndStatusesReport surveysAndStatusesReport;
        private readonly IChartStatisticsViewFactory chartStatisticsViewFactory;
        private readonly IMapReport mapReport;
        private readonly IQuantityReportFactory quantityReport;
        private readonly ISpeedReportFactory speedReport;

        private readonly IStatusDurationReport statusDurationReport;
        private readonly IDeviceInterviewersReport deviceInterviewersReport;
        private readonly IExportFactory exportFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ReportDataApiController(IAuthorizedUser authorizedUser,
            ISurveysAndStatusesReport surveysAndStatusesReport,
            ITeamsAndStatusesReport teamsAndStatusesReport,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IMapReport mapReport,
            IChartStatisticsViewFactory chartStatisticsViewFactory, 
            IQuantityReportFactory quantityReport, 
            ISpeedReportFactory speedReport,
            IStatusDurationReport statusDurationReport,
            IDeviceInterviewersReport deviceInterviewersReport,
            IExportFactory exportFactory,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.authorizedUser = authorizedUser;
            this.surveysAndStatusesReport = surveysAndStatusesReport;
            this.teamsAndStatusesReport = teamsAndStatusesReport;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.mapReport = mapReport;
            this.chartStatisticsViewFactory = chartStatisticsViewFactory;
            this.quantityReport = quantityReport;
            this.speedReport = speedReport;
            this.statusDurationReport = statusDurationReport;
            this.deviceInterviewersReport = deviceInterviewersReport;
            this.exportFactory = exportFactory;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [HttpGet]
        public IActionResult SupervisorTeamMembersAndStatusesReport([FromQuery]TeamsAndStatusesFilter filter, [FromQuery]string exportType = null)
        {
            var input = new TeamsAndStatusesInputModel
            {
                ViewerId = this.authorizedUser.Id,
                Orders = filter.GetSortOrderRequestItems(),
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                TemplateId = filter.TemplateId,
                TemplateVersion = filter.TemplateVersion
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.teamsAndStatusesReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Number_of_Completed_Interviews);
            }
            
            var view = this.teamsAndStatusesReport.GetBySupervisorAndDependentInterviewers(input);
            return new JsonResult(new TeamsAndStatusesReportResponse
            {
                Draw = filter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalRow = view.TotalRow
            });
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
        public List<string> QuestionInfo(Guid id, long? version)
        {
            var variables = this.mapReport.GetGpsQuestionsByQuestionnaire(id, version);
            return variables;
        }

        [HttpGet]
        public IActionResult QuantityByInterviewers([FromQuery]QuantityByInterviewersReportModel data, [FromQuery]string exportType = null)
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
               ReportType = data.ReportType,
               TimezoneOffsetMinutes = data.TimezoneOffsetMinutes
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.quantityReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Number_of_Completed_Interviews);
            }

            return new JsonResult(this.quantityReport.Load(input));
        }

        [HttpGet]
        public IActionResult QuantityBySupervisors([FromQuery]QuantityBySupervisorsReportModel data, [FromQuery]string exportType = null)
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
                Orders = data.SortOrder,
                TimezoneOffsetMinutes = data.TimezoneOffsetMinutes
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.quantityReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Number_of_Completed_Interviews);
            }

            return new JsonResult(this.quantityReport.Load(input));
        }

        [HttpGet]
        public IActionResult SpeedByInterviewers([FromQuery]SpeedByInterviewersReportModel data, [FromQuery]string exportType = null)
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
                Orders = data.SortOrder,
                TimezoneOffsetMinutes = data.TimezoneOffsetMinutes
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.speedReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Average_Interview_Duration);
            }

            return new JsonResult(this.speedReport.Load(input));
        }

        [HttpGet]
        public IActionResult SpeedBetweenStatusesBySupervisors([FromQuery]SpeedBySupervisorsReportModel filter, [FromQuery]string exportType = null)
        {
            var input = new SpeedBetweenStatusesBySupervisorsReportInputModel
            {
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                ColumnCount = filter.ColumnCount,
                From = filter.From,
                Orders = filter.SortOrder,
                Period = filter.Period,
                QuestionnaireId = filter.QuestionnaireId,
                QuestionnaireVersion = filter.QuestionnaireVersion,
                BeginInterviewStatuses = this.GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(filter.ReportType),
                EndInterviewStatuses = this.GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(filter.ReportType),
                TimezoneOffsetMinutes = filter.TimezoneOffsetMinutes
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.speedReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Speed_Between_Statuses_By_Supervisors);
            }

            return new JsonResult(this.speedReport.Load(input));
        }

        [HttpGet]
        public IActionResult SpeedBetweenStatusesByInterviewers([FromQuery]SpeedByInterviewersReportModel filter, [FromQuery]string exportType = null)
        {
            var input = new SpeedBetweenStatusesByInterviewersReportInputModel
            {
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                ColumnCount = filter.ColumnCount,
                From = filter.From,
                Orders = filter.SortOrder,
                Period = filter.Period,
                QuestionnaireId = filter.QuestionnaireId,
                QuestionnaireVersion = filter.QuestionnaireVersion,
                BeginInterviewStatuses = this.GetBeginInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(filter.ReportType),
                EndInterviewStatuses = this.GetEndInterviewExportedActionsAccordingToReportTypeForSpeedBetweenStatusesReports(filter.ReportType),
                SupervisorId = filter.SupervisorId ?? this.authorizedUser.Id,
                TimezoneOffsetMinutes = filter.TimezoneOffsetMinutes
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.speedReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Speed_Between_Statuses_By_Interviewers);
            }

            return new JsonResult(this.speedReport.Load(input));
        }

        [HttpGet]
        public IActionResult SpeedBySupervisors([FromQuery]SpeedBySupervisorsReportModel data, [FromQuery]string exportType = null)
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
                Orders = data.SortOrder,
                TimezoneOffsetMinutes = data.TimezoneOffsetMinutes
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.speedReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Average_Interview_Duration);
            }

            return new JsonResult(this.speedReport.Load(input));
        }

        [HttpGet]
        public IActionResult HeadquarterSupervisorsAndStatusesReport([FromQuery]TeamsAndStatusesFilter filter, [FromQuery]string exportType = null)
        {

            var input = new TeamsAndStatusesByHqInputModel
            {
                Orders = filter.GetSortOrderRequestItems(),
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                TemplateId = filter.TemplateId,
                TemplateVersion = filter.TemplateVersion
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.teamsAndStatusesReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Teams_and_Statuses);
            }

            var view = this.teamsAndStatusesReport.GetBySupervisors(input);
            return new JsonResult(new TeamsAndStatusesReportResponse
            {
                Draw = filter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalRow = view.TotalRow
            });
        }

        [HttpGet]
        public IActionResult SupervisorSurveysAndStatusesReport(Guid? id = null,[FromQuery]ReportDataApiController.SurveysAndStatusesFilter filter = null, [FromQuery]string exportType = null)
        {
            var teamLeadName = this.authorizedUser.UserName;
            var input = new SurveysAndStatusesReportInputModel
            {
                TeamLeadName = teamLeadName,
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                Orders = filter.ToOrderRequestItems(),
                ResponsibleName = filter.ResponsibleName == teamLeadName ? null : filter.ResponsibleName,
                QuestionnaireId = id
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.surveysAndStatusesReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Surveys_and_Statuses);
            }
            
            var view = this.surveysAndStatusesReport.Load(input);

            return new JsonResult(new ReportDataApiController.SurveysAndStatusesDataTableResponse
            {
                Draw = filter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalRow = view.TotalRow
            });
        }

        [HttpGet]
        public IActionResult HeadquarterSurveysAndStatusesReport(Guid? id = null, [FromQuery]ReportDataApiController.SurveysAndStatusesFilter filter = null, [FromQuery]string exportType = null)
        {
            var input = new SurveysAndStatusesReportInputModel
            {
                Orders = filter.ToOrderRequestItems(),
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                TeamLeadName = filter.ResponsibleName,
                QuestionnaireId = id
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 1;
                input.PageSize = MaxPageSize;

                var report = this.surveysAndStatusesReport.GetReport(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Surveys_and_Statuses);
            }

            var view = this.surveysAndStatusesReport.Load(input);

            return new JsonResult(new ReportDataApiController.SurveysAndStatusesDataTableResponse
            {
                Draw = filter.Draw + 1,
                RecordsTotal = view.TotalCount,
                RecordsFiltered = view.TotalCount,
                Data = view.Items,
                TotalRow = view.TotalRow
            });
        }


        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<IActionResult> DeviceInterviewers([FromQuery]ReportDataApiController.DeviceInterviewersFilter request, Guid? id = null, 
            [FromQuery]string exportType = null)
        {
            var input = new DeviceByInterviewersReportInputModel
            {
                Filter = request.Search.Value,
                Orders = request.GetSortOrderRequestItems(),
                Page = request.Start,
                PageSize = request.Length,
                SupervisorId = id
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                input.Page = 0;
                input.PageSize = MaxPageSize;

                var report = await this.deviceInterviewersReport.GetReportAsync(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Devices_and_Interviewers);
            }

            var data = await this.deviceInterviewersReport.LoadAsync(input);

            return new JsonResult(new ReportDataApiController.DeviceInterviewersDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = data.TotalCount,
                RecordsFiltered = data.TotalCount,
                Data = data.Items,
                TotalRow = data.TotalRow
            });
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
        public async Task<IActionResult> StatusDuration([FromQuery] ReportDataApiController.StatusDurationRequest request, [FromQuery] string exportType = null)
        {
            var input = new StatusDurationInputModel
            {
                Orders = request.GetSortOrderRequestItems(),
                MinutesOffsetToUtc = request.Timezone,
                SupervisorId = request.SupervisorId,
                TemplateId = request.QuestionnaireId,
                TemplateVersion = request.QuestionnaireVersion
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                var report = await this.statusDurationReport.GetReportAsync(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Status_Duration);
            }

            var data = await this.statusDurationReport.LoadAsync(input);

            return new JsonResult(new ReportDataApiController.StatusDurationDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = data.TotalCount,
                RecordsFiltered = data.TotalCount,
                Data = data.Items,
                TotalRow = data.TotalRow
            });
        }


        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> TeamStatusDuration([FromQuery] ReportDataApiController.StatusDurationRequest request, [FromQuery] string exportType = null)
        {
            var input = new StatusDurationInputModel
            {
                SupervisorId = this.authorizedUser.Id,
                Orders = request.GetSortOrderRequestItems(),
                MinutesOffsetToUtc = request.Timezone,
                TemplateId = request.QuestionnaireId,
                TemplateVersion = request.QuestionnaireVersion
            };

            if (!string.IsNullOrEmpty(exportType))
            {
                var report = await this.statusDurationReport.GetReportAsync(input);

                return this.CreateReportResponse(exportType, report, Reports.Report_Status_Duration);
            }

            var data = await this.statusDurationReport.LoadAsync(input);

            return new JsonResult(new ReportDataApiController.StatusDurationDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = data.TotalCount,
                RecordsFiltered = data.TotalCount,
                Data = data.Items,
                TotalRow = data.TotalRow
            });
        }
    }
}
