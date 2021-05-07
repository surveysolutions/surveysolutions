using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers.Api
{
    [ApiValidationAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrator, Headquarter")]
    [Route("api/[controller]/[action]")]
    [ResponseCache(NoStore = true)]
    public class DataExportApiController : ControllerBase
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDataExportStatusReader dataExportStatusReader;
        private readonly IExportFileNameService exportFileNameService;
        private readonly IExportServiceApi exportServiceApi;
        private readonly IExportSettings exportSettings;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly ISystemLog auditLog;
        private readonly ISerializer serializer;
        private readonly ExternalStoragesSettings externalStoragesSettings;
        private readonly ILogger<DataExportApiController> logger;

        public DataExportApiController(
            IFileSystemAccessor fileSystemAccessor,
            IDataExportStatusReader dataExportStatusReader,
            ISerializer serializer,
            IExportSettings exportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IExportFileNameService exportFileNameService,
            IExportServiceApi exportServiceApi,
            ISystemLog auditLog, 
            ExternalStoragesSettings externalStoragesSettings,
            ILogger<DataExportApiController> logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportStatusReader = dataExportStatusReader;
            this.exportSettings = exportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.exportFileNameService = exportFileNameService;
            this.exportServiceApi = exportServiceApi;
            this.auditLog = auditLog;
            this.externalStoragesSettings = externalStoragesSettings;
            this.serializer = serializer;
            this.logger = logger;
        }

        [HttpGet]
        [ObservingNotAllowed]
        public async Task<ActionResult<List<long>>> GetRunningJobs()
        {
            try
            {
                return (await this.exportServiceApi.GetRunningExportJobs()).OrderByDescending(x => x).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        [ObservingNotAllowed]
        public async Task<ActionResult<List<ExportStatusItem>>> ExportStatus()
        {
            try
            {
                var jobs = (await this.exportServiceApi.GetAllJobsList()).OrderByDescending(x => x).ToList();
                var runningJobs = (await this.exportServiceApi.GetRunningExportJobs());

                var allJobs = jobs.Union(runningJobs).ToList();

                var result = new List<ExportStatusItem>();

                foreach (var job in allJobs)
                {
                    result.Add(new ExportStatusItem
                    {
                        Id = job,
                        Running = runningJobs.Contains(job)
                    });
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class ExportStatusItem
        {
            public long Id { get; set; }
            public bool Running { get; set; }
        }

        [HttpPost]
        [ObservingNotAllowed]
        public async Task<ActionResult<List<DataExportProcessView>>> Status([FromBody] long[] ids)
        {
            var statuses = await this.dataExportStatusReader.GetProcessStatuses(ids);
            return statuses;
        }

        [HttpGet]
        [ObservingNotAllowed]
        public async Task<ActionResult<ExportDataAvailabilityView>> DataAvailability(Guid id, long version)
        {
            ExportDataAvailabilityView result = await dataExportStatusReader.GetDataAvailabilityAsync(new QuestionnaireIdentity(id, version));
            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpGet]
        [ObservingNotAllowed]
        public async Task<ActionResult<bool>> WasExportFileRecreated(long id)
        {
            bool result = await dataExportStatusReader.WasExportFileRecreated(id);
            return result;
        }

        [HttpGet]
        [ObservingNotAllowed]
        
        public async Task<ActionResult> DownloadData(long id)
        {
            var processView = await dataExportStatusReader.GetProcessStatus(id);
            if (processView == null)
            {
                return NotFound();
            }

            DataExportArchive result = await this.dataExportStatusReader.GetDataArchive(id);
            if (result == null)
            {
                return NotFound();
            }

            if (result.Redirect != null)
            {
                return Redirect(result.Redirect);
            }
            
            return File(result.Data, @"applications/octet-stream", WebUtility.UrlDecode(result.FileName));
        }

        [HttpGet]
        [ObservingNotAllowed]
        
        public async Task<ActionResult> DDIMetadata(Guid id, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var archivePassword = this.exportSettings.EncryptionEnforced() ? this.exportSettings.GetPassword() : null;
            var result = await exportServiceApi.GetDdiArchive(questionnaireIdentity.ToString(), archivePassword);

            var fileName = this.exportFileNameService.GetFileNameForDdiByQuestionnaire(questionnaireIdentity);

            return File(await result.ReadAsStreamAsync(), "text/xml", fileSystemAccessor.GetFileName(fileName));
        }

        [HttpPost]
        [ObservingNotAllowed]
        public async Task<ActionResult<DataExportUpdateRequestResult>> Regenerate(long id, string accessToken = null)
        {
            var result = await this.exportServiceApi.Regenerate(id, GetPasswordFromSettings(), null, null);
            return result;
        }

        [HttpPost]
        [ObservingNotAllowed]
        public async Task<ActionResult<long>> RequestUpdate(Guid id, long version,
            DataExportFormat format,
            InterviewStatus? status = null,
            DateTime? from = null,
            DateTime? to = null,
            Guid? translationId = null,
            bool? includeMeta = null)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return NotFound("Questionnaire not found");

            return await RequestExportUpdateAsync(questionnaireBrowseItem, format, status, @from, to, 
                translation: translationId, includeMeta: includeMeta);
        }

        private async Task<ActionResult<long>> RequestExportUpdateAsync(
            QuestionnaireBrowseItem questionnaireBrowseItem,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? @from,
            DateTime? to,
            string accessToken = null,
            string refreshToken = null,
            ExternalStorageType? externalStorageType = null,
            Guid? translation = null,
            bool? includeMeta = null)
        {
            long jobId = 0;
            try
            {
                DataExportUpdateRequestResult result = await this.exportServiceApi.RequestUpdate(
                    questionnaireBrowseItem.Id,
                    format,
                    status,
                    @from?.ToUniversalTime(),
                    to?.ToUniversalTime(),
                    GetPasswordFromSettings(),
                    accessToken,
                    refreshToken,
                    externalStorageType,
                    translation,
                    includeMeta);

                jobId = result?.JobId ?? 0;

                this.auditLog.ExportStared(
                    $@"{questionnaireBrowseItem.Title} v{questionnaireBrowseItem.Version} {status?.ToString() ?? ""}",
                    format);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }

            return jobId;
        }

        [HttpPost]
        [ObservingNotAllowed]
        public async Task<ActionResult<bool>> DeleteDataExportProcess([FromQuery] long id)
        {
            try
            {
                await this.exportServiceApi.DeleteProcess(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        [ObservingNotAllowed]
        public Task<DataExportStatusView> GetExportStatus(Guid id, long version, InterviewStatus? status, DateTime? from = null, DateTime? to = null)
            => this.dataExportStatusReader.GetDataExportStatusForQuestionnaireAsync(new QuestionnaireIdentity(id, version),
                status,
                fromDate: @from?.ToUniversalTime(),
                toDate: to?.ToUniversalTime());

        [HttpPost]
        [EnableCors("export")]
        [AllowAnonymous]
        public async Task<ActionResult> ExportToExternalStorage(ExportToExternalStorageModel model)
        {
            var state = this.serializer.Deserialize<ExternalStorageStateModel>(model.State);
            if (state == null)
                return BadRequest("Export parameters not found");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(state.QuestionnaireIdentity);
            if (questionnaireBrowseItem == null || questionnaireBrowseItem.IsDeleted)
                return NotFound("@Questionnaire not found");

            try
            {
                var response = await GetExternalStorageAuthTokenAsync(state, model.Code);

                if (string.IsNullOrEmpty(response.AccessToken) || string.IsNullOrEmpty(response.RefreshToken))
                {
                    logger.LogError($"Access and/or refresh token tokens for {state.Type} are null or empty.");
                    return BadRequest($"Could not get tokens for {state.Type} by code. Result is empty.");
                }

                await RequestExportUpdateAsync(questionnaireBrowseItem,
                    state.Format ?? DataExportFormat.Binary,
                    state.InterviewStatus,
                    state.FromDate?.ToUniversalTime(),
                    state.ToDate?.ToUniversalTime(),
                    response.AccessToken,
                    response.RefreshToken,
                    state.Type,
                    translation: state.TranslationId);

                return Ok();
            }
            catch (ApiException)
            {
                return BadRequest($"Could not get access token for {state.Type} by code");
            }
        }

        private Task<ExternalStorageTokenResponse> GetExternalStorageAuthTokenAsync(ExternalStorageStateModel state, string code)
        {
            var storageSettings = this.GetExternalStorageSettings(state.Type);
            var client =  RestService.For<IOAuth2Api>(new HttpClient()
            {
                BaseAddress = new Uri(storageSettings.TokenUri)
            });
            var request = new ExternalStorageAccessTokenRequest
            {
                Code = code,
                ClientId = storageSettings.ClientId,
                ClientSecret = storageSettings.ClientSecret,
                RedirectUri = this.externalStoragesSettings.OAuth2.RedirectUri,
                GrantType = "authorization_code"
            };
            
            return client.GetTokensByAuthorizationCodeAsync(request);
        }

        private ExternalStoragesSettings.ExternalStorageOAuth2Settings GetExternalStorageSettings(ExternalStorageType type)
        {
            switch (type)
            {
                case ExternalStorageType.Dropbox:
                    return this.externalStoragesSettings.OAuth2.Dropbox;
                case ExternalStorageType.GoogleDrive :
                    return this.externalStoragesSettings.OAuth2.GoogleDrive;
                case ExternalStorageType.OneDrive:
                    return this.externalStoragesSettings.OAuth2.OneDrive;
                default:
                    throw new NotSupportedException($"<{type}> not supported external storage type");
            }
        }

        private string GetPasswordFromSettings()
        {
            return this.exportSettings.EncryptionEnforced()
                ? this.exportSettings.GetPassword()
                : null;
        }

        public class ExportToExternalStorageModel
        {
            public string Code { get; set; }
            public string State { get; set; }
        }

        public class ExternalStorageStateModel
        {
            public ExternalStorageType Type { get; set; }
            public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
            public InterviewStatus? InterviewStatus { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public DataExportFormat? Format { get; set; }
            public Guid? TranslationId { get; set; }
        }
    }
}
