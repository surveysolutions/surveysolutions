using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface ITabularFormatExportService
    {
        void CreateTemplateFilesForAdvancedPreloading(QuestionnaireIdentity questionnaireIdentity, string basePath);
    }
}
