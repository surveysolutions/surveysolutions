using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using Sqo;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Json;
using WB.Core.Infrastructure.Android.Implementation.Services.Log;
using WB.Core.Infrastructure.Android.Implementation.Services.Network;
using WB.Core.Infrastructure.Android.Implementation.Services.Rest;
using WB.Core.Infrastructure.Android.Implementation.Services.Storage;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Shared.Enumerator;

namespace WB.UI.Tester.Ninject
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

            this.Bind(typeof(Core.BoundedContexts.Tester.Infrastructure.IPlainStorageAccessor<>)).To(typeof(SiaqodbPlainStorageAccessor<>)).InSingletonScope();

            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();

            this.Bind<INetworkService>().To<NetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();

            this.Bind<NewtonJsonSerializer>().ToSelf().InSingletonScope();
        }
    }
}