using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IStatefulInterviewRepository, StatefulInterviewRepository>();
        }
    }
}