using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using Ninject.Modules;
using WB.Core.Synchronization.Implementation.ImportManager;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncProvider;
using WB.Core.Synchronization.SyncStorage;
using WB.Core.Synchronization.Views;

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
            this.Bind<IChunkWriter>().To<ReadSideChunkWriter>().InSingletonScope();
            this.Bind<IChunkReader>().To<ReadSideChunkReader>();
            this.Bind<IIncomePackagesRepository>().To<IncomePackagesRepository>().InSingletonScope().WithConstructorArgument("folderPath", currentFolderPath);
            this.Bind<IViewFactory<IncomeSyncPackagesInputModel, IncomeSyncPackagesView>>()
                .To<IncomeSyncPackagesViewFactory>();
        }
    }
}
