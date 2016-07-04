using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IQuestionnaireExportStructureStorage
    {
        QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity id);
    }
}