using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unidecode.NET;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class PdfExporter : IPdfExporter
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger<PdfExporter> logger;

        private string translationDefaultName = "Original";

        public PdfExporter(ITenantApi<IHeadquartersApi> tenantApi,
            IFileSystemAccessor fileSystemAccessor,
            ILogger<PdfExporter> logger)
        {
            this.tenantApi = tenantApi ?? throw new ArgumentNullException(nameof(tenantApi));
            this.fileSystemAccessor = fileSystemAccessor ?? throw new ArgumentNullException(nameof(fileSystemAccessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExportAsync(TenantInfo tenant,
            QuestionnaireDocument questionnaire,
            string basePath, CancellationToken cancellationToken)
        {
            var targetFolder = Path.Combine(basePath, "Questionnaire");
            Directory.CreateDirectory(targetFolder);
            
            targetFolder = Path.Combine(targetFolder, "Preview");
            IHeadquartersApi hqApi = tenantApi.For(tenant);

            this.logger.LogDebug("Loading main pdf for {questionnaire}", questionnaire.QuestionnaireId);

            var targetFileName = Path.ChangeExtension(questionnaire.VariableName, ".pdf");
            targetFileName = fileSystemAccessor.MakeValidFileName(targetFileName);
            try
            {
                var mainPdf = await hqApi.GetPdfAsync(questionnaire.QuestionnaireId);

                if (mainPdf.Headers.ContentType?.MediaType != null &&  mainPdf.Headers.ContentType.MediaType.Contains("html"))
                    targetFileName = Path.ChangeExtension(targetFileName, ".html");
                 
                if (cancellationToken.IsCancellationRequested) return;
                
                var defaultTranslationName = string.IsNullOrEmpty(questionnaire.DefaultLanguageName)
                    ? translationDefaultName
                    : questionnaire.DefaultLanguageName.Unidecode();
                var mainFilePath = Path.Combine(targetFolder, $"{defaultTranslationName} {targetFileName}");
                Directory.CreateDirectory(targetFolder);
                using (var mainStream = this.fileSystemAccessor.OpenOrCreateFile(mainFilePath, false))
                {
                    await mainPdf.CopyToAsync(mainStream);
                }

                this.logger.LogInformation("Exported questionnaire pdf to {path}", mainFilePath);
            }
            catch (Refit.ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogInformation("PDF file missing for {id}", questionnaire.QuestionnaireId);
                }
                else
                {
                    this.logger.LogError(e, "Error while loading of PDF file {id}", questionnaire.QuestionnaireId);
                }
            }
            
            foreach (var translation in questionnaire.Translations)
            {
                if (cancellationToken.IsCancellationRequested) return;
                try
                {
                    this.logger.LogDebug("Loading pdf for {questionnaire} {translationId}", questionnaire.QuestionnaireId, translation.Id);
                    var translatedPdf = await hqApi.GetPdfAsync(questionnaire.QuestionnaireId, translation.Id);
                    var fileName = this.fileSystemAccessor.MakeValidFileName(translation.Name.Unidecode() + " " + targetFileName);
                    
                    if (translatedPdf.Headers.ContentType?.MediaType != null && translatedPdf.Headers.ContentType.MediaType.Contains("html"))
                        targetFileName = Path.ChangeExtension(targetFileName, ".html");
                    
                    var translatedFilePath = Path.Combine(targetFolder, fileName);

                    for (int i = 1; fileSystemAccessor.IsFileExists(translatedFilePath); i++)
                    {
                        translatedFilePath = Path.Combine(targetFolder,
                            $"{translation.Name.Unidecode()} ({i}) {targetFileName}");
                    }

                    using (var translatedStream = this.fileSystemAccessor.OpenOrCreateFile(translatedFilePath, false))
                    {
                        await translatedPdf.CopyToAsync(translatedStream);
                    }

                    this.logger.LogInformation("Exported questionnaire pdf to {path}", translatedFilePath);
                }
                catch (Refit.ApiException e)
                {
                    if (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        this.logger.LogInformation("PDF file missing for {id} translation {translationId}", questionnaire.QuestionnaireId, translation.Id);
                    }
                    else
                    {
                        this.logger.LogError(e, "Error while loading of PDF file {id} translation {translationId}", questionnaire.QuestionnaireId, translation.Id);
                    }
                }
            }
        }
    }
}
