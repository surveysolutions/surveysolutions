using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class DataExportApiController : ApiController
    {
        private readonly IFilebasedExportedDataAccessor exportedFilesAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly IDataExportStatusReader dataExportStatusReader;
        private readonly IDataExportProcessesService dataExportProcessesService;

        private readonly IDdiMetadataAccessor ddiMetadataAccessor;
        private readonly IExternalFileStorage externalFileStorage;

        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        public DataExportApiController(
            IFileSystemAccessor fileSystemAccessor,
            IDataExportStatusReader dataExportStatusReader,
            IDataExportProcessesService dataExportProcessesService,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IDdiMetadataAccessor ddiMetadataAccessor,
            IExternalFileStorage externalFileStorage,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportStatusReader = dataExportStatusReader;
            this.dataExportProcessesService = dataExportProcessesService;
            this.exportedFilesAccessor = filebasedExportedDataAccessor;
            this.ddiMetadataAccessor = ddiMetadataAccessor;
            this.externalFileStorage = externalFileStorage;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
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
        public HttpResponseMessage AllData(Guid id, long version, DataExportFormat format, InterviewStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            var filenameFullPath = this.exportedFilesAccessor.GetArchiveFilePathForExportedData(
                new QuestionnaireIdentity(id, version), format, status, @from?.ToUniversalTime(),
                to?.ToUniversalTime());

            if (format == DataExportFormat.Binary && externalFileStorage.IsEnabled())
            {
                var filename = this.fileSystemAccessor.GetFileName(filenameFullPath);
                if (this.externalFileStorage.IsExist($@"export/{filename}"))
                {
                    var directLink = this.externalFileStorage.GetDirectLink($@"export/{filename}", TimeSpan.FromMinutes(30));

                    var response = Request.CreateResponse(HttpStatusCode.Moved);
                    response.Headers.Location = new Uri(directLink);
                    return response;
                }
            }

            return CreateFile(filenameFullPath);
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage DDIMetadata(Guid id, long version)
            => CreateFile(this.ddiMetadataAccessor.GetFilePathToDDIMetadata(new QuestionnaireIdentity(id, version)));

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
                this.dataExportProcessesService.AddDataExport(new DataExportProcessDetails(format, questionnaireIdentity, questionnaireBrowseItem.Title)
                {
                    FromDate = from?.ToUniversalTime(),
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
        public DataExportStatusView GetExportStatus(Guid id, long version, InterviewStatus? status, DateTime? from = null, DateTime? to = null)
            => this.dataExportStatusReader.GetDataExportStatusForQuestionnaire(new QuestionnaireIdentity(id, version), status, from?.ToUniversalTime(), to?.ToUniversalTime());

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
    }
}