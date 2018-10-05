using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public partial class ReportDataApiController
    {
        public class SurveysAndStatusesDataTableResponse : DataTableResponse<HeadquarterSurveysAndStatusesReportLine>
        {
            public long TotalInterviewCount { get; set; }
            public HeadquarterSurveysAndStatusesReportLine TotalRow { get; set; }
        }


        public class StatusDurationDataTableResponse : DataTableResponse<StatusDurationRow>
        {
            public StatusDurationRow TotalRow { get; set; }
        }

        public class DeviceInterviewersDataTableResponse : DataTableResponse<DeviceInterviewersReportLine>
        {
            public DeviceInterviewersReportLine TotalRow { get; set; }
        }

        public class StatusDurationRequest : DataTableRequest
        {
            public string QuestionnaireId { get; set; }
            public Guid? SupervisorId { get; set; }
            public int Timezone { get; set; }
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
                    return new[] { InterviewExportedAction.ApprovedBySupervisor, InterviewExportedAction.RejectedBySupervisor };
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

        private HttpResponseMessage CreateReportResponse(string exportType, ReportView report, string reportName)
        {
            if(!Enum.TryParse(exportType, true, out ExportFileType type))
            {
                type = ExportFileType.Csv;
            }

            var exportFile = this.exportFactory.CreateExportFile(type);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));
            var result = new ProgressiveDownload(this.Request).ResultMessage(exportFileStream, exportFile.MimeType);

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = $@"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };
            return result;
        }
    }
}
