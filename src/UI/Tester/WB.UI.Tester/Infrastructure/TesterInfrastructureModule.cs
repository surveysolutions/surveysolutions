using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using Sqo;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Tester.Infrastructure.Internals;
using WB.UI.Tester.Infrastructure.Internals.Json;
using WB.UI.Tester.Infrastructure.Internals.Log;
using WB.UI.Tester.Infrastructure.Internals.Rest;
using WB.UI.Tester.Infrastructure.Internals.Security;
using WB.UI.Tester.Infrastructure.Internals.Settings;
using WB.UI.Tester.Infrastructure.Internals.Storage;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Tester.Infrastructure
{
    public class TesterInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IEventStore>().To<InMemoryEventStore>().InSingletonScope();
            this.Bind<ISnapshotStore>().To<InMemoryEventStore>().InSingletonScope();

            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<InMemoryKeyValueStorage<QuestionnaireDocument>>().InSingletonScope();
            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().To<InMemoryKeyValueStorage<QuestionnaireModel>>().InSingletonScope();

            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")));

            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorage<>)).InSingletonScope();

            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();

            this.Bind<ITesterNetworkService>().To<TesterNetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();

            this.Bind<NewtonJsonSerializer>().ToSelf().InSingletonScope();

            this.Bind<IDesignerApiService>().To<DesignerApiService>().InSingletonScope();
            this.Bind<IFriendlyErrorMessageService>().To<FriendlyErrorMessageService>().InSingletonScope();

            this.Bind<ITesterSettings>().To<TesterSettings>();
            this.Bind<IEnumeratorSettings>().To<TesterSettings>();

            this.Bind<IPrincipal>().To<TesterPrincipal>().InSingletonScope();
        }
    }
}