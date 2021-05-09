using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.ServicesIntegration.Export;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api
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
            public Guid? QuestionnaireId { get; set; }
            public long? QuestionnaireVersion { get; set; }
            public Guid? SupervisorId { get; set; }
            public int Timezone { get; set; }
        }

        public class DeviceInterviewersFilter : DataTableRequest
        {
            public Guid? SupervisorId { get; set; }
        }

        public class SurveysAndStatusesFilter : DataTableRequest
        {
            public string ResponsibleName { get; set; }
            public Guid? QuestionnaireId { get; set; }
        }

        public class QuantityDataTableResponse : DataTableResponse<QuantityByResponsibleReportRow>
        {
            public DateTimeRange[] DateTimeRanges { get; set; }
            public QuantityTotalRow TotalRow { get; set; }
        }

        public class SpeedDataTableResponse : DataTableResponse<SpeedByResponsibleReportRow>
        {
            public DateTimeRange[] DateTimeRanges { get; set; }
            public SpeedByResponsibleTotalRow TotalRow { get; set; }
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

        private IActionResult CreateReportResponse(string exportType, ReportView report, string reportName)
        {
            if(!Enum.TryParse(exportType, true, out ExportFileType type))
            {
                type = ExportFileType.Csv;
            }

            var exportFile = this.exportFactory.CreateExportFile(type);

            Stream exportFileStream = new MemoryStream(exportFile.GetFileBytes(report));
            var result = new FileStreamResult(exportFileStream, exportFile.MimeType)
            {
                FileDownloadName = $"{this.fileSystemAccessor.MakeValidFileName(reportName)}{exportFile.FileExtension}"
            };
            return result;
        }
    }
}
