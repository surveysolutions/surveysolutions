using System.Runtime.CompilerServices;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IStatefulInterviewRepository, StatefulInterviewRepository>();
            registry.Bind<StatefulInterview>();
        }

        public void Init(IServiceLocator serviceLocator)
        {
            
        }
    }
}
