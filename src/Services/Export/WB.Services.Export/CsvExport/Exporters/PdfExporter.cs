using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IPdfExporter
    {
        Task ExportAsync(TenantInfo tenant,
            QuestionnaireDocument questionnaire,
            string basePath);
    }

    public class PdfExporter : IPdfExporter
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly ILogger<PdfExporter> logger;

        public PdfExporter(ITenantApi<IHeadquartersApi> tenantApi,
            ILogger<PdfExporter> logger)
        {
            this.tenantApi = tenantApi ?? throw new ArgumentNullException(nameof(tenantApi));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExportAsync(TenantInfo tenant,
            QuestionnaireDocument questionnaire,
            string basePath)
        {
            var targetFolder = Path.Combine(basePath, "Pdf");
            IHeadquartersApi hqApi = tenantApi.For(tenant);

            this.logger.LogDebug("Loading main pdf for {questionnaire}", questionnaire.QuestionnaireId);

            var targetFileName = Path.ChangeExtension(questionnaire.VariableName, ".pdf");
            try
            {
                var mainPdf = await hqApi.GetPdfAsync(questionnaire.QuestionnaireId, null);
                var mainFilePath = Path.Combine(targetFolder, targetFileName);
                Directory.CreateDirectory(targetFolder);
                using (var mainStream = File.OpenWrite(mainFilePath))
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
                try
                {
                    this.logger.LogDebug("Loading pdf for {questionnaire} {translationId}", questionnaire.QuestionnaireId, translation.Id);
                    var translatedPdf = await hqApi.GetPdfAsync(questionnaire.QuestionnaireId, translation.Id);
                    var translatedFilePath = Path.Combine(targetFolder, translation.Name + " " + targetFileName);

                    using (var translatedStream = File.OpenWrite(translatedFilePath))
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
