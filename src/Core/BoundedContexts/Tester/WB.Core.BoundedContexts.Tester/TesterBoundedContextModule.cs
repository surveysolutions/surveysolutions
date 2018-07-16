using System.Threading.Tasks;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Tester
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TesterBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IEventStore, InMemoryEventStore>();
            registry.BindAsSingleton<ISnapshotStore, InMemoryEventStore>();
            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, InMemoryKeyValueStorage<QuestionnaireDocument>>();
            registry.BindAsSingleton<IPlainStorage<OptionView>, InMemoryPlainStorage<OptionView>>();
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();
            registry.BindAsSingleton<IExecutedCommandsStorage, ExecutedCommandsStorage>();

            registry.BindAsSingleton<ICommandService, TesterCommandService>();

        }

        public Task Init(IServiceLocator serviceLocator)
        {
            return Task.CompletedTask;
        }
    }
}
