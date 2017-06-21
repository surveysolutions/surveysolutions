using System.Runtime.CompilerServices;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

[assembly: InternalsVisibleTo("WB.Tests.Unit")]
[assembly: InternalsVisibleTo("WB.Tests.Integration")]
[assembly: InternalsVisibleTo("PerformanceTest")]
[assembly: InternalsVisibleTo("WB.Tests.Abc")]

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