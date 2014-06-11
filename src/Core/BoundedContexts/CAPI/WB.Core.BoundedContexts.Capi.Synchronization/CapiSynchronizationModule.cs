using Ninject.Modules;

namespace WB.Core.BoundedContexts.Capi.Synchronization
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
