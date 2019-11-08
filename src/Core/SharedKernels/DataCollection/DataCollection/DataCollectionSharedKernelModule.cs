using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Scenarios;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : IModule, IAppModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IStatefulInterviewRepository, StatefulInterviewRepository>();
            registry.Bind<IScenarioService, ScenarioService>();
            registry.Bind<StatefulInterview>(true);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status) => Task.CompletedTask;

        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IStatefulInterviewRepository, StatefulInterviewRepository>();
            registry.Bind<IScenarioService, ScenarioService>();
            registry.BindToMethod((s) =>
            {
                var interview = s.GetRequiredService<StatefulInterview>();
                interview.ServiceLocatorInstance = s.GetService<IServiceLocator>();

                return interview;
            });
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status) => Task.CompletedTask;
    }
}
