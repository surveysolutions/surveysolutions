using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.UI.Headquarters.API
{
    [ApiBasicAuth(new[] { UserRoles.ApiUser }, TreatPasswordAsPlain = true)]
    [RoutePrefix(@"api/v1/export")]
    public class ExportController : ApiController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDdiMetadataAccessor ddiMetadataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IDataExportStatusReader dataExportStatusReader;

        public ExportController(IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IDataExportProcessesService dataExportProcessesService,
            IFileSystemAccessor fileSystemAccessor,
            IDdiMetadataAccessor ddiMetadataAccessor,
            IParaDataAccessor paraDataAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IDataExportStatusReader dataExportStatusReader)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.dataExportProcessesService = dataExportProcessesService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.ddiMetadataAccessor = ddiMetadataAccessor;
            this.paraDataAccessor = paraDataAccessor;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.dataExportStatusReader = dataExportStatusReader;
        }

        [HttpGet]
        [Route(@"{exportType}/{id}")]
        public IHttpActionResult Get(string id, string exportType)
        {
            QuestionnaireIdentity questionnaireIdentity;
            if (!QuestionnaireIdentity.TryParse(id, out questionnaireIdentity))
                return this.Content(HttpStatusCode.NotFound, @"Invalid questionnaire identity");

            DataExportFormat dataExportType;
            if (!Enum.TryParse(exportType, true, out dataExportType))
                return this.Content(HttpStatusCode.NotFound, @"Unknown export type");

            string exportedFilePath;
            switch (dataExportType)
            {
                case DataExportFormat.DDI:
                    exportedFilePath = this.ddiMetadataAccessor.GetFilePathToDDIMetadata(questionnaireIdentity);
                    break;
                case DataExportFormat.Paradata:
                    exportedFilePath = this.paraDataAccessor.GetPathToParaDataArchiveByQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
                    break;
                default:
                    exportedFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(questionnaireIdentity, dataExportType);
                    break;
            }

            if(!this.fileSystemAccessor.IsFileExists(exportedFilePath))
                return this.NotFound();

            var exportedFileName = this.fileSystemAccessor.GetFileName(exportedFilePath);

            return new ProgressiveDownloadResult(this.Request, exportedFilePath, exportedFileName, @"application/zip");
        }
        
        [HttpPost]
        [Route(@"{exportType}/{id?}/start")]
        public IHttpActionResult StartProcess(string id, string exportType)
        {
            DataExportFormat dataExportType;
            if (!Enum.TryParse(exportType, true, out dataExportType))
                return this.Content(HttpStatusCode.NotFound, @"Unknown export type");

            switch (dataExportType)
            {
                case DataExportFormat.Paradata:
                    this.dataExportProcessesService.AddParaDataExport(dataExportType);
                    break;
                case DataExportFormat.DDI:
                    return this.BadRequest(@"Not supported export type");
                default:
                    QuestionnaireIdentity questionnaireIdentity;
                    if (!QuestionnaireIdentity.TryParse(id, out questionnaireIdentity))
                        return this.Content(HttpStatusCode.NotFound, @"Invalid questionnaire identity");

                    var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
                    if (questionnaireBrowseItem == null)
                        return this.Content(HttpStatusCode.NotFound, @"Questionnaire not found");

                    this.dataExportProcessesService.AddDataExport(questionnaireIdentity, dataExportType);
                    break;
            }

            return this.Ok();
        }
        
        [HttpPost]
        [Route(@"{exportType}/{id}/cancel")]
        public IHttpActionResult CancelProcess(string id, string exportType)
        {
            QuestionnaireIdentity questionnaireIdentity;
            if (!QuestionnaireIdentity.TryParse(id, out questionnaireIdentity))
                return this.Content(HttpStatusCode.NotFound, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return this.Content(HttpStatusCode.NotFound, @"Questionnaire not found");

            DataExportFormat exportFormat;
            if (!Enum.TryParse(exportType, true, out exportFormat))
                return this.Content(HttpStatusCode.NotFound, @"Unknown export type");

            var dataExportType = exportFormat == DataExportFormat.Paradata
                ? DataExportType.ParaData
                : DataExportType.Data;

            this.dataExportProcessesService.DeleteProcess(questionnaireIdentity, exportFormat, dataExportType);

            return this.Ok();
        }

        [HttpGet]
        [Route(@"{exportType}/{id}/details")]
        public IHttpActionResult ProcessDetails(string id, string exportType)
        {
            QuestionnaireIdentity questionnaireIdentity;
            if (!QuestionnaireIdentity.TryParse(id, out questionnaireIdentity))
                return this.Content(HttpStatusCode.NotFound, @"Invalid questionnaire identity");

            var questionnaireBrowseItem = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaireBrowseItem == null)
                return this.Content(HttpStatusCode.NotFound, @"Questionnaire not found");

            DataExportFormat exportFormat;
            if (!Enum.TryParse(exportType, true, out exportFormat))
                return this.Content(HttpStatusCode.NotFound, @"Unknown export type");

            var dataExportType = exportFormat == DataExportFormat.Paradata
                ? DataExportType.ParaData
                : DataExportType.Data;

            var allExportStatuses = this.dataExportStatusReader.GetDataExportStatusForQuestionnaire(questionnaireIdentity);

            var exportStatusByExportType = allExportStatuses?.DataExports?.FirstOrDefault(x =>
                x.DataExportFormat == exportFormat &&
                x.DataExportType == dataExportType);

            if (exportStatusByExportType == null)
                return this.NotFound();

            var runningExportStatus = allExportStatuses.RunningDataExportProcesses.FirstOrDefault(x =>
                (x.QuestionnaireIdentity == null || x.QuestionnaireIdentity.Equals(questionnaireIdentity)) &&
                x.Format == exportFormat &&
                x.Type == dataExportType);

            return this.Ok(new ExportDetails
            {
                HasExportedFile = exportStatusByExportType.HasDataToExport,
                LastUpdateDate = exportStatusByExportType.LastUpdateDate,
                ExportStatus = exportStatusByExportType.StatusOfLatestExportProcess.ToString(),
                RunningProcess = runningExportStatus == null ? null : new RunningProcess
                {
                    StartDate = runningExportStatus.BeginDate,
                    ProgressInPercents = runningExportStatus.Progress
                }
            });
        }


        public class ExportDetails
        {
            public bool HasExportedFile { get; set; }
            public DateTime? LastUpdateDate { get; set; }
            public string ExportStatus { get; set; }
            public RunningProcess RunningProcess { get; set; }
        }

        public class RunningProcess
        {
            public DateTime StartDate { get; set; }
            public int ProgressInPercents { get; set; }
        }
    }

    
}
