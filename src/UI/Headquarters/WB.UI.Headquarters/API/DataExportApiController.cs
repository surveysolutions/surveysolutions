using System;
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
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Filters;
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
        private readonly IAuditLog auditLog;
        private readonly ISerializer serializer;

        public DataExportApiController(
            IFileSystemAccessor fileSystemAccessor,
            IDataExportStatusReader dataExportStatusReader,
            ISerializer serializer,
            IExportSettings exportSettings,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, 
            IExportFileNameService exportFileNameService,
            IExportServiceApi exportServiceApi,
            IAuditLog auditLog)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportStatusReader = dataExportStatusReader;
            this.exportSettings = exportSettings;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.exportFileNameService = exportFileNameService;
            this.exportServiceApi = exportServiceApi;
            this.auditLog = auditLog;
            this.serializer = serializer;
        }
        
        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<HttpResponseMessage> AllData(Guid id, long version, DataExportFormat format, 
            InterviewStatus? status = null, 
            DateTime? from = null, 
            DateTime? to = null)
        {
            var result = await this.dataExportStatusReader.GetDataArchive(new QuestionnaireIdentity(id, version), format, status, from , to);
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
        public async Task<HttpResponseMessage> RequestUpdate(Guid id, long version,
            DataExportFormat format, InterviewStatus? status, DateTime? from = null, DateTime? to = null)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                throw new HttpException(404, @"Questionnaire not found");

            return await RequestExportUpdateAsync(questionnaireBrowseItem, format, status, @from, to);
        }

        private async Task<HttpResponseMessage> RequestExportUpdateAsync(
            QuestionnaireBrowseItem questionnaireBrowseItem, DataExportFormat format, InterviewStatus? status,
            DateTime? @from,
            DateTime? to, string accessToken = null, ExternalStorageType? externalStorageType = null)
        {
            try
            {
                await this.exportServiceApi.RequestUpdate(questionnaireBrowseItem.Id,
                    format, status,
                    @from?.ToUniversalTime(),
                    to?.ToUniversalTime(), GetPasswordFromSettings(), accessToken, externalStorageType);

                this.auditLog.ExportStared(
                    $@"{questionnaireBrowseItem.Title} v{questionnaireBrowseItem.Version} {status?.ToString() ?? ""}",
                    format);
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return Request.CreateResponse(true);
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

        [HttpPost]
        [AllowAnonymous]
        public async Task ExportToExternalStorage(ExportToExternalStorageModel model)
        {
            var state = this.serializer.Deserialize<ExternalStorageStateModel>(model.State);
            if(state == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, @"Export parameters not found");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(state.QuestionnaireIdentity);
            if (questionnaireBrowseItem == null)
                throw new HttpException(404, @"Questionnaire not found");

            await RequestExportUpdateAsync(questionnaireBrowseItem, DataExportFormat.Binary, 
                null,
                state.FromDate?.ToUniversalTime(),
                state.ToDate?.ToUniversalTime(),
                model.Access_token, state.Type);
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

        }
    }
}
