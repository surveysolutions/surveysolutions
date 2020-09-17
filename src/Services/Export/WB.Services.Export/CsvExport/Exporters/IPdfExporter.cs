using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IPdfExporter
    {
        Task ExportAsync(TenantInfo tenant,
            QuestionnaireDocument questionnaire,
            string basePath, CancellationToken cancellationToken);
    }
}
