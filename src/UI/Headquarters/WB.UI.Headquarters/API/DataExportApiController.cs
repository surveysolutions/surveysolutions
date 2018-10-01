using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class DataExportApiController : ApiController
    {
        private readonly IFilebasedExportedDataAccessor exportedFilesAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly IDataExportStatusReader dataExportStatusReader;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IDataExportFileAccessor exportFileAccessor;

        private readonly IExportFileNameService exportFileNameService;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IExportSettings exportSettings;
        private readonly InterviewDataExportSettings interviewExportSettings;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private readonly IConfigurationManager configurationManager;


        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly ISerializer serializer;

        public DataExportApiController(
            IFileSystemAccessor fileSystemAccessor,
            IDataExportStatusReader dataExportStatusReader,
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor exportFileAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            ISerializer serializer,
            IExternalFileStorage externalFileStorage,
            IExportSettings exportSettings,
            InterviewDataExportSettings interviewExportSettings,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IConfigurationManager configurationManager,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, 
            IExportFileNameService exportFileNameService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportStatusReader = dataExportStatusReader;
            this.dataExportProcessesService = dataExportProcessesService;
            this.exportFileAccessor = exportFileAccessor;
            this.exportedFilesAccessor = filebasedExportedDataAccessor;
            this.externalFileStorage = externalFileStorage;
            this.exportSettings = exportSettings;
            this.interviewExportSettings = interviewExportSettings;
            this.exportServiceSettings = exportServiceSettings;
            this.configurationManager = configurationManager;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.exportFileNameService = exportFileNameService;
            this.serializer = serializer;
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage Paradata(Guid id, long version, DateTime? from = null, DateTime? to = null)
        {
            return CreateFile(this.exportedFilesAccessor.GetArchiveFilePathForExportedData(
                new QuestionnaireIdentity(id, version), DataExportFormat.Paradata, null, @from?.ToUniversalTime(),
                to?.ToUniversalTime()));
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        [ApiNoCache]
        public async Task<HttpResponseMessage> AllData(Guid id, long version, DataExportFormat format, 
            InterviewStatus? status = null, 
            DateTime? from = null, 
            DateTime? to = null)
        {
            var result = await this.dataExportStatusReader.GetDataArchive(
                configurationManager.AppSettings["BaseUrl"],
                this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key,
                new QuestionnaireIdentity(id, version), format, status, from , to);

            if (result.Redirect != null)
            {
                var response = Request.CreateResponse(HttpStatusCode.Redirect);
                response.Headers.Location = result.Redirect;
                return response;
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(result.Data);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(@"application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = result.FileName
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
            var serviceApi = Refit.RestService.For<IExportServiceApi>(interviewExportSettings.ExportServiceUrl);
            var result = await serviceApi.GetDdiArchive(questionnaireIdentity.ToString(),
                this.exportSettings.GetPassword(),
                this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key,
                this.configurationManager.AppSettings["BaseUrl"]);

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
        public HttpResponseMessage RequestUpdate(Guid id, long version,
            DataExportFormat format, InterviewStatus? status, DateTime? from = null, DateTime? to = null)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                throw new HttpException(404, @"Questionnaire not found");

            try
            {
                this.dataExportProcessesService.AddDataExportAsync(configurationManager.AppSettings["BaseUrl"],
                    this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key,
                    new DataExportProcessDetails(format, questionnaireIdentity, questionnaireBrowseItem.Title)
                {
                    FromDate = @from?.ToUniversalTime(),
                    ToDate = to?.ToUniversalTime(),
                    InterviewStatus = status
                });
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return Request.CreateResponse(true);
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public HttpResponseMessage DeleteDataExportProcess(string id)
        {
            try
            {
                this.dataExportProcessesService.DeleteDataExport(id);
            }
            catch (Exception)
            {
                return Request.CreateResponse(false);
            }

            return Request.CreateResponse(true);
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public Task<DataExportStatusView> GetExportStatus(Guid id, long version, InterviewStatus? status, DateTime? from = null, DateTime? to = null)
            => this.dataExportStatusReader.GetDataExportStatusForQuestionnaireAsync(
                configurationManager.AppSettings["BaseUrl"],
                this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key, 
                new QuestionnaireIdentity(id, version), 
                status, 
                fromDate: @from?.ToUniversalTime(), 
                toDate: to?.ToUniversalTime());

        private HttpResponseMessage CreateFile(string filePath)
        {
            if (!fileSystemAccessor.IsFileExists(filePath))
                throw new HttpException(404, @"file is absent");

            Stream exportZipStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = new ProgressiveDownload(this.Request).ResultMessage(exportZipStream, @"application/zip");
            
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileNameStar = fileSystemAccessor.GetFileName(filePath)
            };

            return result;
        }

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

            await this.dataExportProcessesService.AddDataExportAsync(configurationManager.AppSettings["BaseUrl"],
                this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey).Key, 
                new DataExportProcessDetails(DataExportFormat.Binary, state.QuestionnaireIdentity, questionnaireBrowseItem.Title)
            {
                AccessToken = model.Access_token,
                InterviewStatus = state.InterviewStatus,
                FromDate = state.FromDate,
                ToDate = state.ToDate,
                StorageType = state.Type
            });
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
