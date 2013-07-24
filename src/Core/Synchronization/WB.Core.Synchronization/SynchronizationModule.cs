using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncProvider;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.Synchronization
{
    public class SynchronizationModule : NinjectModule
    {
        private readonly string currentFolderPath;

        public SynchronizationModule(string currentFolderPath)
        {
            this.currentFolderPath = currentFolderPath;
        }

        public override void Load()
        {
            this.Bind<ISyncManager>().To<SyncManager>();
            this.Bind<ISyncProvider>().To<WB.Core.Synchronization.SyncProvider.SyncProvider>();
            this.Bind<IBackupManager>().To<DefaultBackupManager>();


            this.Bind<ISynchronizationDataStorage>().To<SimpleSynchronizationDataStorage>().InSingletonScope();
            this.Bind<IChunkStorage>().To<ReadSideChunkStorage>().InSingletonScope();
            var incomeStorage=new IncomePackagesRepository(currentFolderPath);
            this.Bind<IIncomePackagesRepository>().ToConstant(incomeStorage);
        }
    }
}
