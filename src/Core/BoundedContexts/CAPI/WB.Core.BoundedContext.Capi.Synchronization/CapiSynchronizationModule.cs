using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using Ninject.Modules;
using WB.Core.BoundedContext.Capi.Synchronization.Synchronization.ChangeLog;
using WB.Core.BoundedContext.Capi.Synchronization.Synchronization.SyncCacher;
using WB.Core.BoundedContext.Capi.Synchronization.Views.InterviewMetaInfo;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.BoundedContext.Capi.Synchronization
{
    public class CapiSynchronizationModule : NinjectModule
    {
        public override void Load()
        {
          /*  var interviewMetaInfoFactory = new InterviewMetaInfoFactory(questionnaireStore);
            var changeLogStore = new FileChangeLogStore(interviewMetaInfoFactory);
            var syncCacher = new FileSyncCacher();
            this.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();

            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);
            this.Bind<ISyncCacher>().ToConstant(syncCacher);

            this.Bind<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>().ToConstant(interviewMetaInfoFactory);*/
        }
    }
}
