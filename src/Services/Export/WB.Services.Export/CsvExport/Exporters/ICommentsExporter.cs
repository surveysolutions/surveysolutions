using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface ICommentsExporter
    {
        Task ExportAsync(QuestionnaireExportStructure questionnaireExportStructure,
            List<Guid> interviewIdsToExport,
            string basePath,
            TenantInfo tenant,
            IProgress<int> progress,
            CancellationToken cancellationToken);

        void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string folderPath);
        string CommentsFileName { get; }
        DoExportFileHeader[] CommentsFileColumns { get; }
    }
}
