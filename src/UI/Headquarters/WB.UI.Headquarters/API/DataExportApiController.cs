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
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.UI.Headquarters.API
{
    public class DataExportApiController : ApiController
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel> exportedDataReferenceViewFactory;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IDataExportQueue dataExportQueue;

        public DataExportApiController( 
            IFileSystemAccessor fileSystemAccessor, 
            IViewFactory<ExportedDataReferenceInputModel, ExportedDataReferencesViewModel> exportedDataReferenceViewFactory, 
            IDataExportQueue dataExportQueue, IParaDataAccessor paraDataAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.exportedDataReferenceViewFactory = exportedDataReferenceViewFactory;
            this.dataExportQueue = dataExportQueue;
            this.paraDataAccessor = paraDataAccessor;
        }

        [HttpGet]
        public HttpResponseMessage Paradata(Guid id, long version)
        {
            var path = this.paraDataAccessor.GetPathToParaDataByQuestionnaire(id, version);
            if (!fileSystemAccessor.IsFileExists(path))
                throw new HttpException(404, "para data is absent");

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);

            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileSystemAccessor.GetFileName(path);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            return result;
        }

        [HttpPost]
        public HttpResponseMessage RequestUpdateOfParadata()
        {
            try
            {
                this.dataExportQueue.EnQueueParaDataExportProcess(DataExportFormat.TabularData);
            }
            catch (Exception)
            {
            }
            
            return Request.CreateResponse(true);
        }

        [HttpPost]
        public HttpResponseMessage DeleteDataExportProcess(string id)
        {
            try
            {
                this.dataExportQueue.DeleteDataExportProcess(id);
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
    }
}