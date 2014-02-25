using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Factories
{
    public interface IQuestionnaireCacheInitializerFactory
    {
        IQuestionnaireCacheInitializer CreateQuestionnaireCacheInitializer(QuestionnaireDocument document);
    }
}