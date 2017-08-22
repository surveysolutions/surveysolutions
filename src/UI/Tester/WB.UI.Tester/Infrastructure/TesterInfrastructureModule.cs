using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Tester.Infrastructure.Internals;
using WB.UI.Tester.Infrastructure.Internals.Log;
using WB.UI.Tester.Infrastructure.Internals.Rest;
using WB.UI.Tester.Infrastructure.Internals.Security;
using WB.UI.Tester.Infrastructure.Internals.Settings;
using WB.UI.Tester.Infrastructure.Internals.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Tester.Infrastructure
{
    public class TesterInfrastructureModule : IModule
    {
        private readonly string basePath;
        private readonly string questionnaireAssembliesFolder;

        public TesterInfrastructureModule(string basePath, string questionnaireAssembliesFolder = "assemblies")
        {
            this.basePath = basePath;
            this.questionnaireAssembliesFolder = questionnaireAssembliesFolder;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IEventStore, InMemoryEventStore>();
            registry.BindAsSingleton<ISnapshotStore, InMemoryEventStore>();
            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, InMemoryKeyValueStorage<QuestionnaireDocument>>();
            registry.BindToConstant(() => new SqliteSettings
            {
                PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data")
            });

            registry.BindAsSingleton(typeof(IPlainStorage<>), typeof(SqlitePlainStorage<>)); // TODO Move to generic module between IN, T

            // this.Unbind<IPlainStorage<OptionView>>(); // TODO check wtf?
            registry.BindAsSingleton<IPlainStorage<OptionView>, InMemoryPlainStorage<OptionView>>();

            registry.Bind<ILoggerProvider, XamarinInsightsLoggerProvider>();
            registry.BindAsSingleton<ILogger, XamarinInsightsLogger>();
            registry.Bind<IRestServiceSettings, TesterSettings>();
            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IEnumeratorSettings, TesterSettings>();
            registry.Bind<IRestServicePointManager, RestServicePointManager>();
            registry.Bind<IHttpClientFactory, ModernHttpClientFactory>();
            registry.Bind<IRestService, RestService>();
            registry.Bind<ISerializer, PortableJsonSerializer>();
            registry.Bind<IInterviewAnswerSerializer,PortableInterviewAnswerJsonSerializer>();
            registry.Bind<IJsonAllTypesSerializer, PortableJsonAllTypesSerializer>();

            registry.Bind<IStringCompressor, JsonCompressor>();
            registry.BindAsSingleton<IDesignerApiService, DesignerApiService>();
            registry.BindAsSingleton<IPrincipal, TesterPrincipal>();

            registry.BindAsSingletonWithConstructorArgument<IQuestionnaireAssemblyAccessor, TesterQuestionnaireAssemblyAccessor>(
                "assemblyStorageDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory(this.questionnaireAssembliesFolder));

            registry.BindAsSingletonWithConstructorArgument<IAudioFileStorage, TesterAudioFileStorage>("rootDirectoryPath", basePath);
            registry.BindAsSingletonWithConstructorArgument<IImageFileStorage, TesterImageFileStorage>("rootDirectoryPath", basePath);
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();
        }
    }
}