using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    internal interface IPreloadedDataServiceFactory
    {
        IPreloadedDataService CreatePreloadedDataService(QuestionnaireExportStructure exportStructure,
            QuestionnaireRosterStructure questionnaireRosterStructure, QuestionnaireDocument questionnaireDocument);
    }
}
