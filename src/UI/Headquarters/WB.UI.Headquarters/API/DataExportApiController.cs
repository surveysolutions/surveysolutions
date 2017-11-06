﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
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
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
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
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.ddiMetadataAccessor = ddiMetadataAccessor;
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage Paradata(Guid id, long version)
            => CreateHttpResponseMessageWithFileContent(
                this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                    new QuestionnaireIdentity(id, version), DataExportFormat.Paradata));

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage AllData(Guid id, long version, DataExportFormat format, InterviewStatus? status = null)
        {
            return
                CreateHttpResponseMessageWithFileContent(
                    this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                        new QuestionnaireIdentity(id, version), format, status));
        }

        [HttpGet]
        [ObserverNotAllowedApi]
        public HttpResponseMessage DDIMetadata(Guid id, long version)
        {
            return
                CreateHttpResponseMessageWithFileContent(
                    this.ddiMetadataAccessor.GetFilePathToDDIMetadata(new QuestionnaireIdentity(id,
                        version)));
        }

        [HttpPost]
        [ObserverNotAllowedApi]
        public HttpResponseMessage RequestUpdate(Guid questionnaireId, long questionnaireVersion,
            DataExportFormat format,
            InterviewStatus? status)
        {
            try
            {
                this.dataExportProcessesService.AddDataExport(new QuestionnaireIdentity(questionnaireId, questionnaireVersion), format, status);
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

        public DataExportStatusView ExportedDataReferencesForQuestionnaire(Guid questionnaireId,
            long questionnaireVersion,
            InterviewStatus? status)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            return this.dataExportStatusReader.GetDataExportStatusForQuestionnaire(questionnaireIdentity, status);
        }

        private HttpResponseMessage CreateHttpResponseMessageWithFileContent(string filePath)
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