using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
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

        public DataExportApiController(
            IFileSystemAccessor fileSystemAccessor,
            IDataExportStatusReader dataExportStatusReader,
            IDataExportProcessesService dataExportProcessesService,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            IDdiMetadataAccessor ddiMetadataAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportStatusReader = dataExportStatusReader;
            this.dataExportProcessesService = dataExportProcessesService;
            this.exportedFilesAccessor = filebasedExportedDataAccessor;
            this.ddiMetadataAccessor = ddiMetadataAccessor;
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage Paradata(Guid id, long version, string from = null, string to = null)
            => CreateFile(this.exportedFilesAccessor.GetArchiveFilePathForExportedData(
                    new QuestionnaireIdentity(id, version), DataExportFormat.Paradata, null, from.TryParseDate(), to.TryParseDate()));

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage AllData(Guid id, long version, DataExportFormat format, InterviewStatus? status = null, string from = null, string to = null)
            => CreateFile(this.exportedFilesAccessor.GetArchiveFilePathForExportedData(
                    new QuestionnaireIdentity(id, version), format, status, from.TryParseDate(), to.TryParseDate()));

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage DDIMetadata(Guid id, long version)
            => CreateFile(this.ddiMetadataAccessor.GetFilePathToDDIMetadata(new QuestionnaireIdentity(id, version)));

        [HttpPost]
        [ObserverNotAllowedApi]
        public HttpResponseMessage RequestUpdate(Guid id, long version,
            DataExportFormat format, InterviewStatus? status, string from = null, string to = null)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);
            try
            {
                DateTime? fromDate = from.TryParseDate();
                DateTime? toDate = to.TryParseDate();

                this.dataExportProcessesService.AddDataExport(new DataExportProcessDetails(format, questionnaireIdentity, null)
                {
                    FromDate = fromDate.HasValue ? new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 0, 0, 1).ToUniversalTime() : (DateTime?)null,
                    ToDate = toDate.HasValue ? new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 23, 59, 59).ToUniversalTime() : (DateTime?) null,
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
        public DataExportStatusView GetExportStatus(Guid id, long version, InterviewStatus? status, string from = null, string to = null)
            => this.dataExportStatusReader.GetDataExportStatusForQuestionnaire(new QuestionnaireIdentity(id, version),
                status, from.TryParseDate(), to.TryParseDate());

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