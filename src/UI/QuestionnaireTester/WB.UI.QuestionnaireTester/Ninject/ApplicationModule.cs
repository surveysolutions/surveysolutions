using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.UI.QuestionnaireTester.Views.Adapters;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<DesignerApiService>().ToSelf().InSingletonScope();

            var evenStore = new InMemoryEventStore();
            var snapshotStore = new InMemoryEventStore();


            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);

            this.Bind<IQuestionEditorViewAdapter>().To<QuestionEditorViewAdapter>().InSingletonScope();
        }
    }
}