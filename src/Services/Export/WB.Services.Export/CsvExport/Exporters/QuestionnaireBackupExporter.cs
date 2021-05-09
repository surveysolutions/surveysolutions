using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IQuestionnaireBackupExporter
    {
        Task ExportAsync(TenantInfo tenant, QuestionnaireDocument questionnaire, string basePath, CancellationToken cancellationToken);
    }

    public class QuestionnaireBackupExporter : IQuestionnaireBackupExporter
    {
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger<QuestionnaireBackupExporter> logger;

        public QuestionnaireBackupExporter(IFileSystemAccessor fileSystemAccessor, ILogger<QuestionnaireBackupExporter> logger, ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.fileSystemAccessor = fileSystemAccessor ?? throw new ArgumentNullException(nameof(fileSystemAccessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tenantApi = tenantApi ?? throw new ArgumentNullException(nameof(tenantApi));
        }

        public async Task ExportAsync(TenantInfo tenant, QuestionnaireDocument questionnaire, string basePath, 
            CancellationToken cancellationToken)
        {
            try
            {
                var targetFolder = Path.Combine(basePath, "Questionnaire");
                Directory.CreateDirectory(targetFolder);

                IHeadquartersApi hqApi = tenantApi.For(tenant);
                var backup = await hqApi.GetBackupAsync(questionnaire.QuestionnaireId);
                if (cancellationToken.IsCancellationRequested) return;

                var backupFilePath = Path.Combine(targetFolder, "content.zip");

                await using (var mainStream = this.fileSystemAccessor.OpenOrCreateFile(backupFilePath, false))
                {
                    await backup.CopyToAsync(mainStream);
                }

                this.logger.LogInformation("Exported questionnaire backup to {path}", backupFilePath);
            }
            catch (Refit.ApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    this.logger.LogInformation("Backup file is missing for {id}", questionnaire.QuestionnaireId);
                }
                else
                {
                    this.logger.LogError(e, "Error while loading of backup file {id}", questionnaire.QuestionnaireId);
                }
            }
        }
    }
}
