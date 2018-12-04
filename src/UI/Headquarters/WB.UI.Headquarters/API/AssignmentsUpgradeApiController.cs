using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using CsvHelper;
using CsvHelper.Configuration;
using Ionic.Zip;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [AuthorizeOr403(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    public class AssignmentsUpgradeApiController : BaseApiController
    {
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IQuestionnaireBrowseViewFactory browseViewFactory;
        private readonly IFileSystemAccessor fileNameService;

        public AssignmentsUpgradeApiController(ICommandService commandService, 
            ILogger logger, 
            IAssignmentsUpgradeService upgradeService,
            IQuestionnaireBrowseViewFactory browseViewFactory,
            IFileSystemAccessor fileNameService) : base(commandService, logger)
        {
            this.upgradeService = upgradeService;
            this.browseViewFactory = browseViewFactory;
            this.fileNameService = fileNameService;
        }

        [CamelCase] 
        [HttpGet]
        public HttpResponseMessage Status(string id)
        {
            AssignmentUpgradeProgressDetails assignmentUpgradeProgressDetails = this.upgradeService.Status(Guid.Parse(id));
            if (assignmentUpgradeProgressDetails != null)
            {
                var questionnaireTo = this.browseViewFactory.GetById(assignmentUpgradeProgressDetails.MigrateTo);
                var questionnaireFrom = this.browseViewFactory.GetById(assignmentUpgradeProgressDetails.MigrateFrom);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    progressDetails = assignmentUpgradeProgressDetails,
                    migrateToTitle = string.Format(Pages.QuestionnaireNameFormat, questionnaireTo.Title,
                        questionnaireTo.Version),
                    migrateFromTitle = string.Format(Pages.QuestionnaireNameFormat, questionnaireFrom.Title,
                        questionnaireFrom.Version)
                });
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not found");
        }

        [HttpPost]
        public HttpResponseMessage Stop(string id)
        {
            this.upgradeService.StopProcess(Guid.Parse(id));
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        public HttpResponseMessage ExportErrors(string id)
        {
            AssignmentUpgradeProgressDetails assignmentUpgradeProgressDetails = this.upgradeService.Status(Guid.Parse(id));

            if (assignmentUpgradeProgressDetails == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            using (MemoryStream resultStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(resultStream))
            using (var csvWriter = new CsvWriter(streamWriter, new Configuration{Delimiter = "\t"}))
            {
                csvWriter.WriteHeader<AssignmentUpgradeError>();
                csvWriter.NextRecord();
                csvWriter.WriteRecords(assignmentUpgradeProgressDetails.AssignmentsMigratedWithError);
                csvWriter.Flush();
                streamWriter.Flush();

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                resultStream.Seek(0, SeekOrigin.Begin);
                response.Content = new ByteArrayContent(Compress(resultStream));

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = this.GetOutputFileName(assignmentUpgradeProgressDetails.MigrateTo)
                };

                return response;
            }
        }

        private string GetOutputFileName(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireBrowseItem = this.browseViewFactory.GetById(questionnaireIdentity);

            var fileName = this.fileNameService.MakeValidFileName(
                $"Web {questionnaireBrowseItem.Title} (ver. {questionnaireBrowseItem.Version}).zip");
            return fileName;
        }

        private static byte[] Compress(Stream uncompressedDataStream)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddEntry("assignments.tab", uncompressedDataStream);
                    zip.Save(memoryStream);
                }

                compressedBytes = memoryStream.ToArray();
            }
            return compressedBytes;
        }
    }
}
