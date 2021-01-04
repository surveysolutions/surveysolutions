using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.v2
{
    /// <summary>
    /// Provides a methods for managing export related actions
    /// </summary>
    [Authorize(Roles = "ApiUser, Administrator")]
    [Route(@"api/v2/export")]
    [PublicApiJson]
    public class ExportController : ControllerBase
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDataExportStatusReader dataExportStatusReader;
        private readonly IExportServiceApi exportServiceApi;
        private readonly IExportSettings exportSettings;
        private readonly ISystemLog auditLog;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IVirtualPathService env;

        public ExportController(
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IQuestionnaireStorage questionnaireStorage,
            IDataExportStatusReader dataExportStatusReader,
            IExportServiceApi exportServiceApi,
            IExportSettings exportSettings,
            ISystemLog auditLog,
            IExportFileNameService exportFileNameService,
            IVirtualPathService env)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.dataExportStatusReader = dataExportStatusReader;
            this.exportServiceApi = exportServiceApi;
            this.exportSettings = exportSettings;
            this.auditLog = auditLog;
            this.exportFileNameService = exportFileNameService;
            this.env = env;
        }

        /// <summary>
        /// Start export file creation
        /// </summary>
        /// 
        /// <response code="201">Export started</response>
        /// <response code="400">Questionnaire id is malformed</response>
        /// <response code="404">Questionnaire was not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ExportProcess>> PostExports([FromBody]CreateExportProcess request)
        {
            if (!QuestionnaireIdentity.TryParse(request.QuestionnaireId, out var questionnaireIdentity))
                return StatusCode(StatusCodes.Status400BadRequest, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return StatusCode(StatusCodes.Status404NotFound, @"Questionnaire not found");

            var password = this.exportSettings.EncryptionEnforced()
                ? this.exportSettings.GetPassword()
                : null;

            if (request.TranslationId != null)
            {
                var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

                if (questionnaire == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, @"Questionnaire not found");
                }

                if (questionnaire.Translations.All(t => t.Id != request.TranslationId))
                {
                    return StatusCode(StatusCodes.Status404NotFound, @"Translation not found");
                }
            }

            var interviewStatus = request.InterviewStatus == ExportInterviewType.All
                ? (InterviewStatus?) null
                : (InterviewStatus) request.InterviewStatus;
            
            var result = await this.exportServiceApi.RequestUpdate(questionnaireIdentity.ToString(),
                (DataExportFormat) request.ExportType, interviewStatus, request.From, request.To, password,
                request.AccessToken, request.RefreshToken, 
                (WB.Core.BoundedContexts.Headquarters.DataExport.Dtos.ExternalStorageType?) request.StorageType,
                request.TranslationId,
                request.IncludeMeta);

            this.auditLog.ExportStared(
                $@"{questionnaireBrowseItem.Title} v{questionnaireBrowseItem.Version} {request.InterviewStatus.ToString() ?? ""}",
                (DataExportFormat) request.ExportType);

            var createdExportProcess = await this.dataExportStatusReader.GetProcessStatus(result.JobId);

            return CreatedAtAction(nameof(GetExports), new {id = result.JobId}, ToExportProcess(createdExportProcess));
        }

        /// <summary>
        /// Get detailed information about export process
        /// </summary>
        /// <param name="id">Export process id</param>
        /// 
        /// <response code="200"></response>
        /// <response code="404">Export process was not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ExportProcess>> GetExports(long id)
        {
            var exportProcess = await this.dataExportStatusReader.GetProcessStatus(id);
            if (exportProcess == null) return NotFound();

            return this.Ok(ToExportProcess(exportProcess));
        }

        /// <summary>
        /// Get list of export processes
        /// </summary>
        /// <param name="exportType">Format of export data to download</param>
        /// <param name="interviewStatus">Status of exported interviews</param>
        /// <param name="questionnaireIdentity">Questionnaire id</param>
        /// <param name="exportStatus">Status of export process</param>
        /// <param name="hasFile">Has export process file to download</param>
        /// <param name="limit">Select a limited number of records</param>
        /// <param name="offset">Skip number of records before beginning to return records</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExportProcess>>> GetExports(ExportType? exportType,
            ExportInterviewType? interviewStatus, string questionnaireIdentity, ExportStatus? exportStatus,
            bool? hasFile, int? limit, int? offset)
        {
            var status = interviewStatus == ExportInterviewType.All ? null : (InterviewStatus?) interviewStatus;

            string questionnaireId = null;
            if (!string.IsNullOrEmpty(questionnaireIdentity))
            {
                questionnaireId = QuestionnaireIdentity.Parse(questionnaireIdentity).ToString();
            }
            
            var filteredProcesses = await this.exportServiceApi.GetJobsByQuery((DataExportFormat?) exportType,
                status, questionnaireId, (DataExportJobStatus?) exportStatus, hasFile, limit, offset);

            return this.Ok(filteredProcesses.Select(ToExportProcess));
        }

        /// <summary>
        /// Cancel export process
        /// </summary>
        ///
        /// <response code="200">Export deleted</response>
        /// <response code="404">Export process was not found</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ExportProcess>> CancelExports(long id)
        {
            var exportProcess = await this.dataExportStatusReader.GetProcessStatus(id);
            if (exportProcess == null) return NotFound();

            await this.exportServiceApi.DeleteProcess(id);

            exportProcess = await this.dataExportStatusReader.GetProcessStatus(id);

            return this.Ok(ToExportProcess(exportProcess));
        }

        /// <summary>
        /// Downloads export file. It will return either 200 status code with export file content or 302 redirect to export location.
        /// </summary>
        /// <param name="id">Export process id</param>
        /// 
        /// <response code="200">Returns content of the export file as zip archive</response>
        /// <response code="302">Location header contains location of export file for download</response>
        /// <response code="404">Export process was not found</response>
        /// <response code="400">Export file was not generated yet</response>
        [HttpGet]
        [Route(@"{id}/file")]
        public async Task<ActionResult> GetExportFile(long id)
        {
            var exportProcess = await this.dataExportStatusReader.GetProcessStatus(id);
            if (exportProcess == null) return NotFound();

            if (exportProcess.Format == DataExportFormat.DDI)
            {
                var archivePassword =
                    this.exportSettings.EncryptionEnforced() ? this.exportSettings.GetPassword() : null;
                var ddiArchiveResponse = await exportServiceApi.GetDdiArchive(
                    exportProcess.QuestionnaireIdentity.ToString(),
                    archivePassword);

                var fileNameForDdiByQuestionnaire =
                    this.exportFileNameService.GetFileNameForDdiByQuestionnaire(exportProcess.QuestionnaireIdentity);

                var content = await ddiArchiveResponse.ReadAsStreamAsync();

                return File(content, "application/zip", fileNameForDdiByQuestionnaire);
            }

            var result = await this.dataExportStatusReader.GetDataArchive(id);

            return result == null
                ? BadRequest()
                : result.Redirect != null
                    ? (ActionResult) Redirect(result.Redirect)
                    : File(result.Data, "application/zip", result.FileName);
        }

        private ExportProcess ToExportProcess(DataExportProcessView exportProcess)
        {
            var process = new ExportProcess
            {
                JobId = exportProcess.Id,
                QuestionnaireId = exportProcess.QuestionnaireIdentity.ToString(),
                From = exportProcess.FromDate,
                To = exportProcess.ToDate,
                StartDate = exportProcess.BeginDate,
                CompleteDate = exportProcess.EndDate,
                TranslationId = exportProcess.TranslationId,
                ExportStatus = (ExportStatus) exportProcess.JobStatus,
                Progress = exportProcess.Progress,
                ExportType = (ExportType) exportProcess.Format,
                ETA = exportProcess.TimeEstimation,
                Error = exportProcess.Error?.Message,
                HasExportFile = exportProcess.HasFile,
                InterviewStatus = exportProcess.InterviewStatus == null
                    ? ExportInterviewType.All
                    : (ExportInterviewType) exportProcess.InterviewStatus
            };

            if (exportProcess.IsRunning || exportProcess.HasFile)
                process.Links = new ExportJobLinks();
            
            if (exportProcess.IsRunning)
                process.Links.Cancel = this.env.GetAbsolutePath(Url.Action("CancelExports", new { id = exportProcess.Id }));
            if (exportProcess.HasFile)
            {
                process.Links.Download = this.env.GetAbsolutePath(Url.Action("GetExportFile", new { id = exportProcess.Id }));
            }

            return process;
        }

        public class CreateExportProcess
        {
            /// <summary>
            /// Format of export data to download
            /// </summary>
            public ExportType ExportType { get; set; }
            /// <summary>
            /// Questionnaire id in format [QuestionnaireGuid$Version]
            /// </summary>
            public string QuestionnaireId { get; set; }
            /// <summary>
            /// Status of exported interviews
            /// </summary>
            public ExportInterviewType InterviewStatus { get; set; }
            /// <summary>
            /// Started date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date
            /// </summary>
            public DateTime? From { get; set; }
            /// <summary>
            /// Finished date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date
            /// </summary>
            public DateTime? To { get; set; }
            /// <summary>
            /// Access token to external storage
            /// </summary>
            public string AccessToken { get; set; }
            /// <summary>
            /// Refresh token to external storage
            /// </summary>
            public string RefreshToken { get; set; }
            /// <summary>
            /// External storage type
            /// </summary>
            public ExternalStorageType? StorageType { get; set; }

            /// <summary>
            /// Translation Id of the questionnaire
            /// </summary>
            public Guid? TranslationId { get; set; }

            public bool? IncludeMeta { get; set; }
        }

        public class ExportProcess : CreateExportProcess
        {
            /// <summary>
            /// Export process id
            /// </summary>
            public long JobId { get; set; }
            /// <summary>
            /// Format of export data to download
            /// </summary>
            public ExportStatus ExportStatus { get; set; }
            /// <summary>
            /// Export process stated date
            /// </summary>
            public DateTime? StartDate { get; set; }
            /// <summary>
            /// Export process completed date
            /// </summary>
            public DateTime? CompleteDate { get; set; }
            /// <summary>
            /// Progress of export in percents
            /// </summary>
            public int Progress { get; set; }
            /// <summary>
            /// Estimated time to finish of export
            /// </summary>
            public TimeSpan? ETA { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Error { get; set; }
            /// <summary>
            /// Links for cancelling export process and downloading data file
            /// </summary>
            public ExportJobLinks Links { get; set; }
            /// <summary>
            /// True, if export process is finished and exported file ready for download, otherwise false 
            /// </summary>
            public bool HasExportFile { get; set; }
        }

        public class ExportJobLinks
        {
            /// <summary>
            /// Link for cancelling export process
            /// </summary>
            public string Cancel { get; set; }
            /// <summary>
            /// Link for downloading file with data
            /// </summary>
            public string Download { get; set; }
        }

        public enum ExportType
        {
            Tabular = 1,
            STATA = 2,
            SPSS = 3,
            Binary = 4,
            DDI = 5,
            Paradata = 6
        }

        public enum ExportStatus
        {
            Created = 0,
            Running = 1,
            Completed = 2,
            Fail = 3,
            Canceled = 4
        }

        public enum ExportInterviewType
        {
            All,
            SupervisorAssigned = 40,
            InterviewerAssigned = 60,
            Completed = 100,

            RejectedBySupervisor = 65,
            ApprovedBySupervisor = 120,

            RejectedByHeadquarters = 125,
            ApprovedByHeadquarters = 130
        }

        public enum ExternalStorageType
        {
            Dropbox = 1,
            OneDrive = 2,
            GoogleDrive = 3
        }
    }
}
