using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API.PublicApi
{
    /// <summary>
    /// Provides a methods for managing export related actions
    /// </summary>
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true)]
    [RoutePrefix(@"api/v1/export")]
    public class ExportController : ApiController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IExportServiceApi exportServiceApi;
        private readonly IDataExportStatusReader dataExportStatusReader;
        private readonly IExportSettings exportSettings;
        private readonly IAuditLog auditLog;

        public ExportController(
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDataExportStatusReader dataExportStatusReader,
            IExportServiceApi exportServiceApi,
            IExportSettings exportSettings,
            IAuditLog auditLog)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.dataExportStatusReader = dataExportStatusReader;
            this.exportServiceApi = exportServiceApi;
            this.exportSettings = exportSettings;
            this.auditLog = auditLog;
        }

        /// <summary>
        /// Starts export file creation
        /// </summary>
        /// <param name="id">Questionnaire id in format [QuestionnaireGuid$Version]</param>
        /// <param name="exportType">Format of export data to download</param>
        /// <param name="status">Status of exported interviews</param>
        /// <param name="from">Started date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// <param name="to">Finished date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// 
        /// <response code="200">Export started</response>
        /// <response code="400">Questionnaire id is malformed</response>
        /// <response code="404">Questionnaire was not found</response>
        [HttpPost]
        [Route(@"{exportType}/{id?}/start")]
        public async Task<IHttpActionResult> StartProcess(string id,
            DataExportFormat exportType,
            InterviewStatus? status = null,
            DateTime? from = null, DateTime? to = null)
        {
            switch (exportType)
            {
                case DataExportFormat.DDI:
                    return this.BadRequest(@"Not supported export type");
                default:
                    if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                        return this.Content(HttpStatusCode.BadRequest, @"Invalid questionnaire identity");

                    var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
                    if (questionnaireBrowseItem == null)
                        return this.Content(HttpStatusCode.NotFound, @"Questionnaire not found");

                    await this.exportServiceApi.RequestUpdate(questionnaireIdentity.ToString(),
                        exportType, status, from, to, GetPasswordFromSettings(), null, null);

                    this.auditLog.ExportStared(
                        $@"{questionnaireBrowseItem.Title} v{questionnaireBrowseItem.Version} {status?.ToString() ?? ""}",
                        exportType);

                    break;
            }

            return this.Ok();
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
        /// <param name="from">Started date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// <param name="to">Finished date for timeframe of exported interviews (when change was done to an interview). Should be in UTC date</param>
        /// 
        /// <response code="200">Canceled</response>
        /// <response code="400">Questionnaire id is malformed</response>
        /// <response code="404">Questionnaire was not found</response>
        [HttpPost]
        [Route(@"{exportType}/{id}/cancel")]
        public async Task<IHttpActionResult> CancelProcess(string id, DataExportFormat exportType, InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return this.Content(HttpStatusCode.BadRequest, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return this.Content(HttpStatusCode.NotFound, @"Questionnaire not found");

            var running = await this.exportServiceApi.GetDataExportStatusForQuestionnaireAsync(id, status, from, to);
            var toCancel = running.RunningDataExportProcesses.FirstOrDefault(p => p.Format == exportType);

            if (toCancel != null)
            {
                await this.exportServiceApi.DeleteProcess(toCancel.DataExportProcessId);
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
        [ResponseType(typeof(ExportDetails))]
        public async Task<IHttpActionResult> ProcessDetails(string id, DataExportFormat exportType, InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return this.Content(HttpStatusCode.BadRequest, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return this.Content(HttpStatusCode.NotFound, @"Questionnaire not found");

            var allExportStatuses = await this.dataExportStatusReader.GetDataExportStatusForQuestionnaireAsync(questionnaireIdentity);

            var exportStatusByExportType = allExportStatuses?.DataExports?.FirstOrDefault(x =>
                x.DataExportFormat == exportType);

            if (exportStatusByExportType == null)
                return this.NotFound();

            var runningExportStatus = allExportStatuses.RunningDataExportProcesses.FirstOrDefault(x =>
                x.QuestionnaireIdentity.Equals(questionnaireIdentity) && x.Format == exportType && x.FromDate == from &&
                x.ToDate == to && x.InterviewStatus == status);

            return this.Ok(new ExportDetails
            {
                HasExportedFile = exportStatusByExportType.HasDataToExport,
                LastUpdateDate = exportStatusByExportType.LastUpdateDate,
                ExportStatus = exportStatusByExportType.StatusOfLatestExportProcess,
                RunningProcess = runningExportStatus == null ? null : new RunningProcess
                {
                    StartDate = runningExportStatus.BeginDate,
                    ProgressInPercents = runningExportStatus.Progress
                }
            });
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
        public async Task<HttpResponseMessage> Get(string id, DataExportFormat exportType, InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, @"Invalid questionnaire identity");

            if (exportType == DataExportFormat.DDI)
            {
                var archivePassword =
                    this.exportSettings.EncryptionEnforced() ? this.exportSettings.GetPassword() : null;
                var ddiArchiveResponse = await exportServiceApi.GetDdiArchive(questionnaireIdentity.ToString(),
                    archivePassword);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(await ddiArchiveResponse.ReadAsByteArrayAsync());

                return response;
            }

            var result = await this.dataExportStatusReader.GetDataArchive(
                questionnaireIdentity, exportType, status, from, to);

            if (result == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (result.Redirect != null)
            {
                var redirect = Request.CreateResponse(HttpStatusCode.Redirect);
                redirect.Headers.Location = new Uri(result.Redirect);
                return redirect;
            }

            return new ProgressiveDownload(Request).ResultMessage(result.Data, "application/zip");
        }


        public class ExportDetails
        {
            [Required]
            public bool HasExportedFile { get; set; }
            public DateTime? LastUpdateDate { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
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
        }
    }
}
