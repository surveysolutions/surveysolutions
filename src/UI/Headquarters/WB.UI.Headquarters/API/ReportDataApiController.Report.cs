using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.API;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public partial class ReportDataApiController
    {
        const int MaxPageSize = 1024;

        [HttpGet]
        public HttpResponseMessage HeadquarterSurveysAndStatusesReport([FromUri]string exportType, [FromUri]SurveysAndStatusesFilter filter)
        {
            filter.Start = 0;
            filter.Length = MaxPageSize;

            var report = this.surveysAndStatusesReport.GetReport(new SurveysAndStatusesReportInputModel
            {
                Orders = filter.ToOrderRequestItems(),
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                TeamLeadName = filter.ResponsibleName
            });

            return this.CreateReportResponse(exportType, report, Reports.Report_Surveys_and_Statuses);
        }

        [HttpGet]
        public HttpResponseMessage SupervisorSurveysAndStatusesReport([FromUri]string exportType, [FromUri]SurveysAndStatusesFilter filter)
        {
            filter.Start = 0;
            filter.Length = MaxPageSize;

            var teamLeadName = this.authorizedUser.UserName;

            var report = this.surveysAndStatusesReport.GetReport(
                new SurveysAndStatusesReportInputModel
                {
                    TeamLeadName = teamLeadName,
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    Orders = filter.ToOrderRequestItems(),
                    ResponsibleName = filter.ResponsibleName == teamLeadName ? null : filter.ResponsibleName
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Surveys_and_Statuses);
        }

        [HttpGet]
        public HttpResponseMessage SupervisorTeamMembersAndStatusesReport([FromUri]string exportType, [FromUri]TeamsAndStatusesFilter filter)
        {
            filter.Start = 0;
            filter.Length = MaxPageSize;

            var report = this.supervisorTeamsAndStatusesReport.GetReport(
                new TeamsAndStatusesInputModel
                {
                    ViewerId = this.authorizedUser.Id,
                    Orders = filter.GetSortOrderRequestItems(),
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TemplateId = filter.TemplateId,
                    TemplateVersion = filter.TemplateVersion
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Teams_and_Statuses);
        }

        [HttpGet]
        public HttpResponseMessage QuantityByInterviewers([FromUri]string exportType, [FromUri]QuantityByInterviewersReportModel filter)
        {
            filter.PageIndex = 1;
            filter.PageSize = MaxPageSize;

            var report = this.quantityReport.GetReport(
                new QuantityByInterviewersReportInputModel
                {
                    SupervisorId = filter.SupervisorId ?? this.authorizedUser.Id,
                    InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(filter.ReportType),
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    QuestionnaireVersion = filter.QuestionnaireVersion,
                    QuestionnaireId = filter.QuestionnaireId,
                    ColumnCount = filter.ColumnCount,
                    From = filter.From,
                    Orders = filter.SortOrder ?? Enumerable.Empty<OrderRequestItem>(),
                    Period = filter.Period,
                    ReportType = filter.ReportType,
                    TimezoneOffsetMinutes = filter.TimezoneOffsetMinutes
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Quantity_By_Interviewers);
        }

        [HttpGet]
        public HttpResponseMessage QuantityBySupervisors([FromUri]string exportType, [FromUri]QuantityBySupervisorsReportModel filter)
        {
            filter.PageIndex = 1;
            filter.PageSize = MaxPageSize;

            var report = this.quantityReport.GetReport(
                new QuantityBySupervisorsReportInputModel
                {
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForQuantityReports(filter.ReportType),
                    Period = filter.Period,
                    ReportType = filter.ReportType,
                    QuestionnaireVersion = filter.QuestionnaireVersion,
                    QuestionnaireId = filter.QuestionnaireId,
                    ColumnCount = filter.ColumnCount,
                    From = filter.From,
                    Orders = filter.SortOrder,
                    TimezoneOffsetMinutes = filter.TimezoneOffsetMinutes
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Quantity_By_Supervisors);
        }

        [HttpGet]
        public HttpResponseMessage SpeedByInterviewers([FromUri]string exportType, [FromUri]SpeedByInterviewersReportModel filter)
        {
            filter.PageIndex = 1;
            filter.PageSize = MaxPageSize;

            var report = this.speedReport.GetReport(
                new SpeedByInterviewersReportInputModel
                {
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    SupervisorId = filter.SupervisorId ?? this.authorizedUser.Id,
                    InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(filter.ReportType),
                    QuestionnaireVersion = filter.QuestionnaireVersion,
                    QuestionnaireId = filter.QuestionnaireId,
                    ColumnCount = filter.ColumnCount,
                    From = filter.From,
                    ReportType = filter.ReportType,
                    Period = filter.Period,
                    Orders = filter.SortOrder,
                    TimezoneOffsetMinutes = filter.TimezoneOffsetMinutes
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Speed_By_Interviewers);
        }

        [HttpGet]
        public HttpResponseMessage SpeedBetweenStatusesBySupervisors([FromUri]string exportType, [FromUri]SpeedBySupervisorsReportModel filter)
        {
            filter.PageIndex = 1;
            filter.PageSize = MaxPageSize;

            var report = this.speedReport.GetReport(
                new SpeedBetweenStatusesBySupervisorsReportInputModel
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
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Speed_Between_Statuses_By_Supervisors);
        }

        [HttpGet]
        public HttpResponseMessage SpeedBetweenStatusesByInterviewers([FromUri]string exportType, [FromUri]SpeedByInterviewersReportModel filter)
        {
            filter.PageIndex = 1;
            filter.PageSize = MaxPageSize;

            var report = this.speedReport.GetReport(
                new SpeedBetweenStatusesByInterviewersReportInputModel
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
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Speed_Between_Statuses_By_Interviewers);
        }

        [HttpGet]
        public HttpResponseMessage SpeedBySupervisors([FromUri]string exportType, [FromUri]SpeedBySupervisorsReportModel filter)
        {
            filter.PageIndex = 1;
            filter.PageSize = MaxPageSize;

            var report = this.speedReport.GetReport(
                new SpeedBySupervisorsReportInputModel
                {
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    InterviewStatuses = this.GetInterviewExportedActionsAccordingToReportTypeForSpeedReports(filter.ReportType),
                    ColumnCount = filter.ColumnCount,
                    From = filter.From,
                    QuestionnaireVersion = filter.QuestionnaireVersion,
                    QuestionnaireId = filter.QuestionnaireId,
                    ReportType = filter.ReportType,
                    Period = filter.Period,
                    Orders = filter.SortOrder,
                    TimezoneOffsetMinutes = filter.TimezoneOffsetMinutes
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Speed_By_Supervisors);
        }

        [HttpGet]
        public HttpResponseMessage HeadquarterSupervisorsAndStatusesReport([FromUri]string exportType, [FromUri]TeamsAndStatusesFilter filter)
        {
            filter.Start = 0;
            filter.Length = MaxPageSize;

            var report = this.headquartersTeamsAndStatusesReport.GetReport(
                new TeamsAndStatusesInputModel
                {
                    Orders = filter.GetSortOrderRequestItems(),
                    Page = filter.PageIndex,
                    PageSize = filter.PageSize,
                    TemplateId = filter.TemplateId,
                    TemplateVersion = filter.TemplateVersion
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Supervisors_And_Statuses);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<HttpResponseMessage> DeviceInterviewers([FromUri]string exportType, [FromUri]DeviceInterviewersFilter request)
        {
            request.Start = 0;
            request.Length = MaxPageSize;

            var report = await this.deviceInterviewersReport.GetReportAsync(
                new DeviceByInterviewersReportInputModel
                {
                    Filter = request.Search.Value,
                    Orders = request.GetSortOrderRequestItems(),
                    Page = request.Start,
                    PageSize = request.Length
                });

            return this.CreateReportResponse(exportType, report, Reports.Report_Devices_and_Interviewers);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> CountDaysOfInterviewInStatus(
            [FromUri] string exportType, [FromUri] CountDaysOfInterviewInStatusRequest request)
        {
            var input = new CountDaysOfInterviewInStatusInputModel
            {
                Orders = request.GetSortOrderRequestItems()
            };

            if (!string.IsNullOrEmpty(request.QuestionnaireId))
            {
                var questionnaireIdentity = QuestionnaireIdentity.Parse(request.QuestionnaireId);
                input.TemplateVersion = questionnaireIdentity.Version;
                input.TemplateId = questionnaireIdentity.QuestionnaireId;
            }

            var report = await this.countDaysOfInterviewInStatusReport.GetReportAsync(input);

            return this.CreateReportResponse(exportType, report, Reports.Report_Days_by_Interview_Status);
        }

        private HttpResponseMessage CreateReportResponse(string exportType, ReportView report, string reportName)
        {
            ExportFileType type;

            Enum.TryParse(exportType, true, out type);

            var exportFile = this.exportFactory.CreateExportFile(type);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report.Headers, report.Data));
            var result = new ProgressiveDownload(this.Request).ResultMessage(exportFileStream, exportFile.MimeType);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = $@"{reportName}{exportFile.FileExtension}"
            };
            return result;
        }
    }
}