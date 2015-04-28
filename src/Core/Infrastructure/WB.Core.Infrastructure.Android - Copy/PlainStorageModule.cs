using Ninject;
using Ninject.Modules;
using Sqo;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Storage;

namespace WB.Core.Infrastructure.Android
{
    public class PlainStorageModule : NinjectModule
    {
        private readonly PlainStorageSettings settings;

        public PlainStorageModule(PlainStorageSettings settings)
        {
            this.settings = settings;
        }

        public override void Load()
        {
            this.Bind<IDocumentSerializer>().To<PlainStorageSerializer>().InSingletonScope();
            SiaqodbConfigurator.SetDocumentSerializer(this.Kernel.Get<IDocumentSerializer>());

            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(this.settings.StorageFolderPath));

            this.Bind(typeof(IPlainStorageAccessor<>)).To(typeof(PlainStorageAccessor<>)).InSingletonScope();
        }
    }
}