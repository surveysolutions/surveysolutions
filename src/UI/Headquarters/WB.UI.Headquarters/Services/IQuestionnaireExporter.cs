using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Services
{
    public interface IQuestionnaireExporter
    {
        File CreateZipExportFile(QuestionnaireIdentity questionnaireIdentity);
    }
}
