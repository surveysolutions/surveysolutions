using Ninject.Modules;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DataCollectionSharedKernelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IHistoricalQuestionnaireRepository>().To<HistoricalQuestionnaireRepository>();
        }
    }
}