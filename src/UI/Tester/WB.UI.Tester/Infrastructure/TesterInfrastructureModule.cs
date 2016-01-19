using System.Collections.Generic;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using Sqo;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Tester.CustomServices;
using WB.UI.Tester.Infrastructure.Internals;
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
        private readonly string questionnaireAssembliesFolder;

        public TesterInfrastructureModule(string questionnaireAssembliesFolder = "assemblies")
        {
            this.questionnaireAssembliesFolder = questionnaireAssembliesFolder;
        }

        public override void Load()
        {
            this.Bind<IEventStore>().To<InMemoryEventStore>().InSingletonScope();
            this.Bind<ISnapshotStore>().To<InMemoryEventStore>().InSingletonScope();

            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<InMemoryKeyValueStorage<QuestionnaireDocument>>().InSingletonScope();
            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().To<InMemoryKeyValueStorage<QuestionnaireModel>>().InSingletonScope();

            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            this.Bind<Siaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")));

            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorage<>)).InSingletonScope();

            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();

            this.Bind<IRestServiceSettings>().To<TesterSettings>();
            this.Bind<INetworkService>().To<AndroidNetworkService>();
            this.Bind<IEnumeratorSettings>().To<TesterSettings>();
            this.Bind<IRestServicePointManager>().To<RestServicePointManager>();
            this.Bind<IRestService>().To<RestService>();

            this.Bind<JsonUtilsSettings>().ToSelf().InSingletonScope();

            this.Bind<ISerializer>().ToMethod((ctx) => new NewtonJsonSerializer(new JsonSerializerSettingsFactory()));

            this.Bind<IStringCompressor>().To<JsonCompressor>();

            this.Bind<IDesignerApiService>().To<DesignerApiService>().InSingletonScope();

            this.Bind<IPrincipal>().To<TesterPrincipal>().InSingletonScope();

            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<TesterQuestionnaireAssemblyFileAccessor>().InSingletonScope()
                .WithConstructorArgument("assemblyStorageDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory(this.questionnaireAssembliesFolder));
        }
    }
}