using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class DataExportApiController : ApiController
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

        public DataExportApiController(
            IFileSystemAccessor fileSystemAccessor,
            IDataExportStatusReader dataExportStatusReader,
            ISerializer serializer,
            IExportSettings exportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IExportFileNameService exportFileNameService,
            IExportServiceApi exportServiceApi,
            ISystemLog auditLog, ExternalStoragesSettings externalStoragesSettings)
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
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<List<long>> Status()
        {
            try
            {
                var jobs = (await this.exportServiceApi.GetAllJobsList()).OrderByDescending(x => x).ToList();
                return jobs;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<List<long>> GetRunningJobs()
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
        [ObserverNotAllowedApi]
        [ApiNoCache]
        [CamelCase]
        public async Task<HttpResponseMessage> Status(long id)
        {
            var result = await dataExportStatusReader.GetProcessStatus(id);
            if (result == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(result);
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        [CamelCase]
        public async Task<HttpResponseMessage> DataAvailability(Guid id, long version)
        {
            var result = await dataExportStatusReader.GetDataAvailabilityAsync(new QuestionnaireIdentity(id, version));
            if (result == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        [CamelCase]
        public async Task<HttpResponseMessage> WasExportFileRecreated(long id)
        {
            var result = await dataExportStatusReader.WasExportFileRecreated(id);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<HttpResponseMessage> DownloadData(long id)
        {
            var processView = await dataExportStatusReader.GetProcessStatus(id);
            if (processView == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return await this.AllData(
                processView.QuestionnaireIdentity.QuestionnaireId,
                processView.QuestionnaireIdentity.Version,
                processView.Format,
                processView.InterviewStatus,
                processView.FromDate,
                processView.ToDate);
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<HttpResponseMessage> AllData(Guid id, long version, DataExportFormat format,
            InterviewStatus? status = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            DataExportArchive result = await this.dataExportStatusReader.GetDataArchive(new QuestionnaireIdentity(id, version), format, status, from, to);
            if (result == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else if (result.Redirect != null)
            {
                var response = Request.CreateResponse(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri(result.Redirect);
                return response;
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(result.Data);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(@"application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
                {
                    FileName = WebUtility.UrlDecode(result.FileName)
                };
                return response;
            }
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<HttpResponseMessage> DDIMetadata(Guid id, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var archivePassword = this.exportSettings.EncryptionEnforced() ? this.exportSettings.GetPassword() : null;
            var result = await exportServiceApi.GetDdiArchive(questionnaireIdentity.ToString(),
                archivePassword);

            var fileName = this.exportFileNameService.GetFileNameForDdiByQuestionnaire(questionnaireIdentity);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(await result.ReadAsByteArrayAsync());
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = fileSystemAccessor.GetFileName(fileName)
            };

            return response;
        }


        [HttpPost]
        [ObserverNotAllowedApi]
        public async Task<HttpResponseMessage> Regenerate(long id, string accessToken = null)
        {
            var result = await this.exportServiceApi.Regenerate(id, GetPasswordFromSettings(), null);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public async Task<HttpResponseMessage> RequestUpdate(Guid id, long version,
            DataExportFormat format, InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                throw new HttpException(404, @"Questionnaire not found");

            return await RequestExportUpdateAsync(questionnaireBrowseItem, format, status, @from, to);
        }

        private async Task<HttpResponseMessage> RequestExportUpdateAsync(
            QuestionnaireBrowseItem questionnaireBrowseItem,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? @from,
            DateTime? to,
            string accessToken = null,
            ExternalStorageType? externalStorageType = null)
        {
            long jobId = 0;
            try
            {
                var result = await this.exportServiceApi.RequestUpdate(
                    questionnaireBrowseItem.Id,
                    format,
                    status,
                    @from?.ToUniversalTime(),
                    to?.ToUniversalTime(),
                    GetPasswordFromSettings(),
                    accessToken,
                    externalStorageType);

                jobId = result?.JobId ?? 0;

                this.auditLog.ExportStared(
                    $@"{questionnaireBrowseItem.Title} v{questionnaireBrowseItem.Version} {status?.ToString() ?? ""}",
                    format);
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return Request.CreateResponse(new
            {
                JobId = jobId
            });
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public async Task<HttpResponseMessage> DeleteDataExportProcess(string id)
        {
            try
            {
                await this.exportServiceApi.DeleteProcess(id);
                return Request.CreateResponse(true);
            }
            catch (Exception)
            {
                return Request.CreateResponse(false);
            }
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public Task<DataExportStatusView> GetExportStatus(Guid id, long version, InterviewStatus? status, DateTime? from = null, DateTime? to = null)
            => this.dataExportStatusReader.GetDataExportStatusForQuestionnaireAsync(new QuestionnaireIdentity(id, version),
                status,
                fromDate: @from?.ToUniversalTime(),
                toDate: to?.ToUniversalTime());

        /// <summary>
        /// Handle CORS preflight request
        /// </summary>
        /// <returns></returns>
        [HttpOptions]
        [AllowAnonymous]
        [Localizable(false)]
        public HttpResponseMessage ExportToExternalStorage()
        {
            var uri = new Uri(externalStoragesSettings.OAuth2.RedirectUri);

            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            // Define and add values to variables: origins, headers, methods (can be global)               
            response.Headers.Add("Access-Control-Allow-Origin", $"{uri.Scheme}://{uri.Host}");
            response.Headers.Add("Access-Control-Allow-Methods", "POST");

            return response;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> ExportToExternalStorage(ExportToExternalStorageModel model)
        {
            var state = this.serializer.Deserialize<ExternalStorageStateModel>(model.State);
            if (state == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, @"Export parameters not found");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(state.QuestionnaireIdentity);
            if (questionnaireBrowseItem == null || questionnaireBrowseItem.IsDeleted)
                throw new HttpException(404, @"Questionnaire not found");

            await RequestExportUpdateAsync(questionnaireBrowseItem,
                state.Format ?? DataExportFormat.Binary,
                state.InterviewStatus,
                state.FromDate?.ToUniversalTime(),
                state.ToDate?.ToUniversalTime(),
                model.Access_token,
                state.Type);

            return ExportToExternalStorage();
        }

        private string GetPasswordFromSettings()
        {
            return this.exportSettings.EncryptionEnforced()
                ? this.exportSettings.GetPassword()
                : null;
        }

        public class ExportToExternalStorageModel
        {
            public string Access_token { get; set; }
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
        }
    }
}
