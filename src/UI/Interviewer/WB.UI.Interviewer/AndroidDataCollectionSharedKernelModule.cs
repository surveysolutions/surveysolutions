using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.SurveyManagement
{
    public class AndroidDataCollectionSharedKernelModule : NinjectModule
    {
        public override void Load()
        {
        }
    }
}