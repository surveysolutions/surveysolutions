using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Export
{
    public interface ITabularFormatExportService
    {
        void GenerateDescriptionFile(QuestionnaireIdentity questionnaireIdentity, string basePath, string dataFilesExtension);

        void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath);
    }
}
