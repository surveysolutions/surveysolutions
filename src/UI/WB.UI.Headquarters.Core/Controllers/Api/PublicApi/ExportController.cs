using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    /// <summary>
    /// Provides a methods for managing export related actions
    /// </summary>
    [Authorize(Roles = "ApiUser, Administrator")]
    [Route(@"api/v1/export")]
    [PublicApiJson]
    [Obsolete]
    public class ExportController : ControllerBase
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IExportServiceApi exportServiceApi;
        private readonly IDataExportStatusReader dataExportStatusReader;
        private readonly IExportSettings exportSettings;
        private readonly ISystemLog auditLog;
        private readonly IExportFileNameService exportFileNameService;

        public ExportController(
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDataExportStatusReader dataExportStatusReader,
            IExportServiceApi exportServiceApi,
            IExportSettings exportSettings,
            ISystemLog auditLog,
            IExportFileNameService exportFileNameService)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.dataExportStatusReader = dataExportStatusReader;
            this.exportServiceApi = exportServiceApi;
            this.exportSettings = exportSettings;
            this.auditLog = auditLog;
            this.exportFileNameService = exportFileNameService;
        }

        /// <summary>
        /// Starts export file creation
        /// </summary>
        /// <param name="id">Questionnaire id in format [QuestionnaireGuid$Version]</param>
        /// <param name="exportType">Format of export data to download</param>
        /// <param name="status">Status of exported interviews</param>
        /// <param name="from">Started date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// <param name="to">Finished date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// <param name="includeMeta">Should export result contain questionnaire meta information</param>
        /// 
        /// <response code="200">Export started</response>
        /// <response code="400">Questionnaire id is malformed</response>
        /// <response code="404">Questionnaire was not found</response>
        [HttpPost]
        [Route(@"{exportType}/{id}/start")]
        [ProducesResponseType(200, Type = typeof(StartNewExportResult))]
        public async Task<ActionResult<StartNewExportResult>> StartProcess(string id,
            DataExportFormat exportType,
            [FromQuery]ExportInterviewStatus? status = null,
            [FromQuery]DateTime? from = null, 
            [FromQuery]DateTime? to = null,
            [FromQuery]bool? includeMeta = null)
        {
            long jobId;
            switch (exportType)
            {
                case DataExportFormat.DDI:
                    return this.BadRequest(@"Not supported export type");
                default:
                    if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                        return StatusCode(StatusCodes.Status400BadRequest, @"Invalid questionnaire identity");

                    var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
                    if (questionnaireBrowseItem == null)
                        return StatusCode(StatusCodes.Status400BadRequest, @"Questionnaire not found");

                    var result = await this.exportServiceApi.RequestUpdate(questionnaireIdentity.ToString(),
                        exportType, status.ToInterviewStatus(), from, to, GetPasswordFromSettings(), 
                        null, null, null, null, includeMeta);

                    jobId = result.JobId;

                    this.auditLog.ExportStared(
                        $@"{questionnaireBrowseItem.Title} v{questionnaireBrowseItem.Version} {status?.ToString() ?? ""}",
                        exportType);

                    break;
            }

            return new StartNewExportResult
            {
                JobId = jobId
            };
        }

        private string GetPasswordFromSettings()
        {
            return this.exportSettings.EncryptionEnforced()
                ? this.exportSettings.GetPassword()
                : null;
        }

        /// <summary>
        /// Stops export file creation if one is in progress 
        /// </summary>
        /// <param name="id">Questionnaire id in format [QuestionnaireGuid$Version]</param>
        /// <param name="exportType">Format of export data to download</param>
        /// <param name="status">Status of exported interviews</param>
        /// <param name="from">Started date for timeframe of exported interviews (when change was done to an interview). Should be UTC date</param>
        /// <param name="to">Finished date for timeframe of exported interviews (when change was done to an interview). Should be UTC date</param>
        /// 
        /// <response code="200">Canceled</response>
        /// <response code="400">Questionnaire id is malformed</response>
        /// <response code="404">Questionnaire was not found</response>
        [HttpPost]
        [Route(@"{exportType}/{id}/cancel")]
        public async Task<ActionResult> CancelProcess(string id, 
            DataExportFormat exportType, 
            [FromQuery]ExportInterviewStatus? status = null, 
            [FromQuery]DateTime? from = null, 
            [FromQuery]DateTime? to = null)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return StatusCode(StatusCodes.Status400BadRequest, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return StatusCode(StatusCodes.Status400BadRequest, @"Questionnaire not found");

            var running = await this.exportServiceApi.GetDataExportStatusForQuestionnaireAsync(id, status.ToInterviewStatus(), from, to);
            var toCancel = running.RunningDataExportProcesses.FirstOrDefault(p => p.Format == exportType);

            if (toCancel != null)
            {
                await this.exportServiceApi.DeleteProcess(toCancel.ProcessId);
            }

            return this.Ok();
        }

        /// <summary>
        /// Gets info about currently running exports
        /// </summary>
        /// <param name="id">Questionnaire id in format [QuestionnaireGuid$Version]</param>
        /// <param name="exportType">Format of export data to download</param>
        /// <param name="status">Status of exported interviews</param>
        /// <param name="from">Started date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// <param name="to">Finished date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// 
        /// <response code="200"></response>
        /// <response code="400">Questionnaire id is malformed</response>
        /// <response code="404">Questionnaire was not found</response>
        [HttpGet]
        [Route(@"{exportType}/{id}/details")]
        public async Task<ActionResult<ExportDetails>> ProcessDetails(string id, 
            DataExportFormat exportType, 
            [FromQuery]ExportInterviewStatus? status = null, [FromQuery]DateTime? from = null, [FromQuery]DateTime? to = null)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return StatusCode(StatusCodes.Status400BadRequest, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return NotFound("Questionnaire not found");

            var allExportStatuses = await this.dataExportStatusReader.GetDataExportStatusForQuestionnaireAsync(questionnaireIdentity);

            var exportStatusByExportType = allExportStatuses?.DataExports?.FirstOrDefault(x =>
                x.DataExportFormat == exportType);

            if (exportStatusByExportType == null)
                return this.NotFound();

            var runningExportStatus = allExportStatuses.RunningDataExportProcesses.FirstOrDefault(x =>
                x.QuestionnaireIdentity.Equals(questionnaireIdentity) && x.Format == exportType && x.FromDate == from &&
                x.ToDate == to && x.InterviewStatus == status.ToInterviewStatus());

            return new ExportDetails
            {
                HasExportedFile = exportStatusByExportType.HasDataToExport,
                LastUpdateDate = exportStatusByExportType.LastUpdateDate,
                ExportStatus = exportStatusByExportType.StatusOfLatestExportProcess,
                RunningProcess = runningExportStatus == null ? null : new RunningProcess
                {
                    StartDate = runningExportStatus.BeginDate,
                    ProgressInPercents = runningExportStatus.Progress,
                    Status = runningExportStatus.ProcessStatus
                }
            };
        }

        /// <summary>
        /// Downloads export file. It will return either 200 status code with export file content or 302 redirect to export location.
        /// </summary>
        /// <param name="id">Questionnaire id in format [QuestionnaireGuid$Version]</param>
        /// <param name="exportType">Format of export data to download</param>
        /// <param name="status">Status of exported interviews</param>
        /// <param name="from">Started date for time frame of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// <param name="to">Finished date for time frame of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// 
        /// <response code="200">Returns content of the export file as zip archive</response>
        /// <response code="302">Location header contains location of export file for download</response>
        /// <response code="404">Export file was not generated yet</response>
        /// <response code="400">Questionnaire id is malformed</response>
        [HttpGet]
        [Route(@"{exportType}/{id}")]
        public async Task<ActionResult> Get(string id, DataExportFormat exportType, 
            [FromQuery]ExportInterviewStatus? status = null, 
            [FromQuery]DateTime? from = null, 
            [FromQuery]DateTime? to = null)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return StatusCode(StatusCodes.Status400BadRequest, @"Invalid questionnaire identity");

            if (exportType == DataExportFormat.DDI)
            {
                var archivePassword =
                    this.exportSettings.EncryptionEnforced() ? this.exportSettings.GetPassword() : null;
                var ddiArchiveResponse = await exportServiceApi.GetDdiArchive(questionnaireIdentity.ToString(),
                    archivePassword);

                var fileNameForDdiByQuestionnaire = this.exportFileNameService.GetFileNameForDdiByQuestionnaire(questionnaireIdentity);

                var content = await ddiArchiveResponse.ReadAsByteArrayAsync();

                return File(content, "application/zip", fileNameForDdiByQuestionnaire);
            }

            var result = await this.dataExportStatusReader.GetDataArchive(
                questionnaireIdentity, exportType, status.ToInterviewStatus(), from, to);

            if (result == null)
            {
                return NotFound();
            }

            if (result.Redirect != null)
            {
                return Redirect(result.Redirect);
            }

            return File(result.Data, "application/zip", result.FileName);
        }

        public class StartNewExportResult
        {
            public long JobId { get; set; }
        }

        public class ExportDetails
        {
            [Required]
            public bool HasExportedFile { get; set; }
            public DateTime? LastUpdateDate { get; set; }

            [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
            [Required]
            public DataExportStatus ExportStatus { get; set; }
            public RunningProcess RunningProcess { get; set; }
        }

        public class RunningProcess
        {
            [Required]
            public DateTime StartDate { get; set; }
            [Required]
            public int ProgressInPercents { get; set; }

            [Required]
            public DataExportStatus Status { get; set; }
        }
    }
}
