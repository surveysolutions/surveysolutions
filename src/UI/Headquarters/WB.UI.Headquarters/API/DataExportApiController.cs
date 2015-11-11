using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Microsoft.Practices.ServiceLocation;
using Quartz;
using Quartz.Impl.Matchers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.UI.Headquarters.API
{
    public class DataExportApiController : ApiController
    {
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;

        private readonly IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel> exportedDataReferenceViewFactory;
        private readonly IDataExportProcessesService dataExportProcessesService;

        public DataExportApiController( 
            IFileSystemAccessor fileSystemAccessor, 
            IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel> exportedDataReferenceViewFactory, 
            IDataExportProcessesService dataExportProcessesService, IParaDataAccessor paraDataAccessor, IFilebasedExportedDataAccessor filebasedExportedDataAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportedDataReferenceViewFactory = exportedDataReferenceViewFactory;
            this.dataExportProcessesService = dataExportProcessesService;
            this.paraDataAccessor = paraDataAccessor;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        [HttpGet]
        public HttpResponseMessage Paradata(Guid id, long version)
        {
            return CreateHttpResponseMessageWithFileContent(this.paraDataAccessor.GetPathToParaDataByQuestionnaire(id, version));
        }

        [HttpGet]
        public HttpResponseMessage AllData(Guid id, long version, DataExportFormat format)
        {
            return CreateHttpResponseMessageWithFileContent(this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(new QuestionnaireIdentity(id,version), format));
        }

        [HttpGet]
        public HttpResponseMessage ApprovedData(Guid id, long version, DataExportFormat format)
        {
            return CreateHttpResponseMessageWithFileContent(this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(new QuestionnaireIdentity(id, version), format));
        }

        [HttpGet]
        public HttpResponseMessage DDIMetadata(Guid id, long version)
        {
            return CreateHttpResponseMessageWithFileContent(this.filebasedExportedDataAccessor.GetFilePathToExportedDDIMetadata(new QuestionnaireIdentity(id, version)));
        }

        [HttpPost]
        public HttpResponseMessage RequestUpdateOfParadata()
        {
            try
            {
                this.dataExportProcessesService.AddParaDataExportProcess(DataExportFormat.Tabular);
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
            
            return Request.CreateResponse(true);
        }

        [HttpPost]
        public HttpResponseMessage RequestUpdate(Guid questionnaireId, long questionnaireVersion, DataExportFormat format)
        {
            try
            {
                this.dataExportProcessesService.AddDataExportProcess(questionnaireId, questionnaireVersion, format);
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return Request.CreateResponse(true);
        }

        [HttpPost]
        public HttpResponseMessage RequestUpdateOfApproved(Guid questionnaireId, long questionnaireVersion, DataExportFormat format)
        {
            try
            {
                this.dataExportProcessesService.AddApprovedDataExportProcess(questionnaireId, questionnaireVersion, format);
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return Request.CreateResponse(true);
        }

        [HttpPost]
        public HttpResponseMessage DeleteDataExportProcess(string id)
        {
            try
            {
                this.dataExportProcessesService.DeleteDataExportProcess(id);
            }
            catch (Exception)
            {
                return Request.CreateResponse(false);
            }

            return Request.CreateResponse(true);
        }

        public ExportedDataReferencesViewModel ExportedDataReferencesForQuestionnaire(ExportedDataReferenceInputModel request)
        {
            return exportedDataReferenceViewFactory.Load(request);
        }

        private HttpResponseMessage CreateHttpResponseMessageWithFileContent(string filePath)
        {
            if (!fileSystemAccessor.IsFileExists(filePath))
                throw new HttpException(404, "file is absent");

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(filePath, FileMode.Open);

            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileSystemAccessor.GetFileName(filePath);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            return result;
        }
    }
}