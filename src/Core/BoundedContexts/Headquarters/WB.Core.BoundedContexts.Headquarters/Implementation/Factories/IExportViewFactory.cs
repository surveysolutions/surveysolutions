using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public interface IExportViewFactory
    {
        QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireIdentity id);
    }
}
