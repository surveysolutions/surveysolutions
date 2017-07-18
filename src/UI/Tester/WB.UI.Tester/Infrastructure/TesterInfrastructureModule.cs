using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
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
    public class TesterInfrastructureModule : NinjectModule
    {
        private readonly string basePath;
        private readonly string questionnaireAssembliesFolder;

        public TesterInfrastructureModule(string basePath, string questionnaireAssembliesFolder = "assemblies")
        {
            this.basePath = basePath;
            this.questionnaireAssembliesFolder = questionnaireAssembliesFolder;
        }

        public override void Load()
        {
            this.Bind<IEventStore>().To<InMemoryEventStore>().InSingletonScope();
            this.Bind<ISnapshotStore>().To<InMemoryEventStore>().InSingletonScope();

            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<InMemoryKeyValueStorage<QuestionnaireDocument>>().InSingletonScope();

            this.Bind<SqliteSettings>().ToConstant(
                new SqliteSettings
                {
                    PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data")
                });
            this.Bind(typeof(IPlainStorage<>)).To(typeof(SqlitePlainStorage<>)).InSingletonScope();

            this.Unbind<IPlainStorage<OptionView>>();
            this.Bind<IPlainStorage<OptionView>>().To<InMemoryPlainStorage<OptionView>>().InSingletonScope();

            this.Bind<ILoggerProvider>().To<XamarinInsightsLoggerProvider>();
            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();

            this.Bind<IRestServiceSettings>().To<TesterSettings>();
            this.Bind<INetworkService>().To<AndroidNetworkService>();
            this.Bind<IEnumeratorSettings>().To<TesterSettings>();
            this.Bind<IRestServicePointManager>().To<RestServicePointManager>();
            this.Bind<IHttpClientFactory>().To<ModernHttpClientFactory>();
            this.Bind<IRestService>().To<RestService>();

            this.Bind<ISerializer>().ToMethod((ctx) => new PortableJsonSerializer());
            this.Bind<IInterviewAnswerSerializer>().ToMethod((ctx) => new PortableInterviewAnswerJsonSerializer());
            this.Bind<IJsonAllTypesSerializer>().ToMethod((ctx) => new PortableJsonAllTypesSerializer());

            this.Bind<IStringCompressor>().To<JsonCompressor>();

            this.Bind<IDesignerApiService>().To<DesignerApiService>().InSingletonScope();

            this.Bind<IPrincipal>().To<TesterPrincipal>().InSingletonScope();

            this.Bind<IQuestionnaireAssemblyAccessor>().To<TesterQuestionnaireAssemblyAccessor>().InSingletonScope()
                .WithConstructorArgument("assemblyStorageDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory(this.questionnaireAssembliesFolder));

            this.Bind<IAudioFileStorage>().To<TesterAudioFileStorage>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", basePath);
            this.Bind<IImageFileStorage>().To<TesterImageFileStorage>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", basePath);
            this.Bind<IQuestionnaireTranslator>().To<QuestionnaireTranslator>();
            this.Bind<IQuestionnaireStorage>().To<QuestionnaireStorage>().InSingletonScope();
        }
    }
}