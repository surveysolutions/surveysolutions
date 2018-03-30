using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Tester
{
    public class TesterBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IEventStore, InMemoryEventStore>();
            registry.BindAsSingleton<ISnapshotStore, InMemoryEventStore>();
            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, InMemoryKeyValueStorage<QuestionnaireDocument>>();
            registry.BindAsSingleton<IPlainStorage<OptionView>, InMemoryPlainStorage<OptionView>>();
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();
        }

        public void Init(IServiceLocator serviceLocator)
        {
            
        }
    }
}
