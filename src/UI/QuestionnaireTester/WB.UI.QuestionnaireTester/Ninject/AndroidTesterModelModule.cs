using Ninject.Modules;
using Sqo;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class AndroidTesterModelModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>().InSingletonScope();
            this.Bind<IStringCompressor>().To<JsonCompressor>().InSingletonScope();
            this.Bind<INetworkService>().To<AndroidNetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();
            this.Bind<IRestServiceSettings>().To<RestServiceSettings>().InSingletonScope();
            this.Bind<IApplicationSettings>().To<ApplicationSettings>().InSingletonScope();
            this.Bind<IPrincipal>().To<Principal>().InSingletonScope();

            this.Bind<IAnswerProgressIndicator>().To<AnswerProgressIndicator>().InSingletonScope();
            
            
            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnareAssemblyTesterFileAccessor>().InSingletonScope();

            
            this.Bind(typeof(IQueryablePlainStorageAccessor<>)).To(typeof(SiaqoDbAccessor<>)).InSingletonScope();
            this.Bind<IDocumentSerializer>().To<StorageSerializer>().InSingletonScope();
        }
    }
}