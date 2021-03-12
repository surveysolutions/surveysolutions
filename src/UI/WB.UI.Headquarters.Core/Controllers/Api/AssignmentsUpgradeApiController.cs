using System;
using System.Collections;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [Route("api/{controller}/{action}/{id?}")]
    [ResponseCache(NoStore = true)]
    public class AssignmentsUpgradeApiController : ControllerBase
    {
        private readonly IAssignmentsUpgradeService upgradeService;
        private readonly IQuestionnaireBrowseViewFactory browseViewFactory;
        private readonly IFileSystemAccessor fileNameService;

        public AssignmentsUpgradeApiController(IAssignmentsUpgradeService upgradeService,
            IQuestionnaireBrowseViewFactory browseViewFactory,
            IFileSystemAccessor fileNameService)
        {
            this.upgradeService = upgradeService;
            this.browseViewFactory = browseViewFactory;
            this.fileNameService = fileNameService;
        }

        [HttpGet]
        public IActionResult Status(string id)
        {
            AssignmentUpgradeProgressDetails assignmentUpgradeProgressDetails = this.upgradeService.Status(Guid.Parse(id));
            if (assignmentUpgradeProgressDetails != null)
            {
                var questionnaireTo = this.browseViewFactory.GetById(assignmentUpgradeProgressDetails.MigrateTo);
                var questionnaireFrom = this.browseViewFactory.GetById(assignmentUpgradeProgressDetails.MigrateFrom);

                return Ok(new
                {
                    progressDetails = assignmentUpgradeProgressDetails,
                    migrateTo = new
                    {
                        questionnaireTo.Title,
                        questionnaireTo.Version
                    },
                    migrateFrom = new
                    {
                        questionnaireFrom.Title,
                        questionnaireFrom.Version
                    },
                });
            }
            return NotFound();

        }

        [HttpPost]
        public IActionResult Stop(string id)
        {
            this.upgradeService.StopProcess(Guid.Parse(id));
            return Ok();
        }

        [HttpGet]
        public IActionResult ExportErrors(string id)
        {
            AssignmentUpgradeProgressDetails assignmentUpgradeProgressDetails = this.upgradeService.Status(Guid.Parse(id));

            if (assignmentUpgradeProgressDetails == null)
            {
                return NotFound();
            }

            using MemoryStream resultStream = new MemoryStream();
            using var streamWriter = new StreamWriter(resultStream);
            using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture) {Delimiter = "\t"});

            csvWriter.WriteHeader<AssignmentUpgradeError>();
            csvWriter.NextRecord();
            csvWriter.WriteRecords((IEnumerable) assignmentUpgradeProgressDetails.GetAssignmentUpgradeErrors());
            csvWriter.Flush();
            streamWriter.Flush();

            resultStream.Seek(0, SeekOrigin.Begin);
            var fileContents = Compress(resultStream);
            var fileName = this.GetOutputFileName(assignmentUpgradeProgressDetails.MigrateTo);
            return File(fileContents, "application/octet-stream", fileName, null, null, false);
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
