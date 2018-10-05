using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Documents;
using Microsoft.AspNet.SignalR.Hubs;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Refit;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.JsonConversion;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Storage;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Hub;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.UI.WebTester
{
    public class WebTesterModule : IModule
    {
        private static string DesignerAddress()
        {
            var baseAddress = ConfigurationSource.Configuration["DesignerAddress"];
            return $"{baseAddress.TrimEnd('/')}";
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>("eventBusSettings", new EventBusSettings
            {
                DisabledEventHandlerTypes = Array.Empty<Type>(),
                EventHandlerTypesWithIgnoredExceptions = Array.Empty<Type>(),
                IgnoredAggregateRoots = new HashSet<string>()
            });

            registry.Bind<WebTesterStatefulInterview>();
            registry.Bind<IInterviewFactory, InterviewFactory>();

            registry.BindAsSingleton<IEvictionObservable, IEvictionNotifier, TokenEviction>();

            registry.Bind<IEnumeratorGroupStateCalculationStrategy, EnumeratorGroupGroupStateCalculationStrategy>();
            registry.Bind<ISupervisorGroupStateCalculationStrategy, SupervisorGroupStateCalculationStrategy>();
            registry.BindAsSingleton<IEventSourcedAggregateRootRepository, IAggregateRootCacheFiller, IAggregateRootCacheCleaner, WebTesterAggregateRootRepository>();
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
            registry.BindAsSingleton<ICommandService, WebTesterCommandService>();
            registry.BindAsSingleton<IEventBus, InProcessEventBus>();

            //var binPath = Path.GetFullPath(Path.Combine(HttpRuntime.CodegenDir, ".." + Path.DirectorySeparatorChar + ".."));
            var binPath = System.Web.Hosting.HostingEnvironment.MapPath("~/bin");
            registry.BindAsSingletonWithConstructorArgument<IAppdomainsPerInterviewManager, AppdomainsPerInterviewManager>("binFolderPath", binPath);
            registry.Bind<IImageProcessingService, ImageProcessingService>();
            
            registry.BindToMethod<IServiceLocator>(() => ServiceLocator.Current);

            #if DEBUG

            #endif

            registry.BindToMethod(() => Refit.RestService.For<IDesignerWebTesterApi>(
                new HttpClient(
                    #if DEBUG
                        new HttpClientHandler
                        {
                            ClientCertificateOptions = ClientCertificateOption.Manual,
                            ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                        }
                    #endif
                    )
                {
                    MaxResponseContentBufferSize = 2_000_000_000,
                    BaseAddress = new Uri(DesignerAddress()),
                    Timeout = TimeSpan.FromMinutes(3)
                },
                new RefitSettings
                {
                   
                    JsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
                        Binder = new OldToNewAssemblyRedirectSerializationBinder()
                    }
                }));

            foreach (var type in HubPipelineModules)
            {
                registry.BindAsSingleton(typeof(IHubPipelineModule), type);
                registry.BindAsSingleton(type, type);
            }

            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();

            registry.BindToMethodInSingletonScope(context => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            }).CreateMapper());


            registry.BindToConstant(() => JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    AssembliesToInclude =
                    {
                        typeof(Startup).Assembly,
                        typeof(WebInterviewModule).Assembly,
                        typeof(WebInterviewHub).Assembly,
                        typeof(CategoricalOption).Assembly
                    }
                }
            }));

            registry.BindAsSingleton<IAudioProcessingService, AudioProcessingService>();

            registry.RegisterDenormalizer<InterviewLifecycleEventHandler>();

            registry.BindAsSingleton<IEventStore, ISnapshotStore, InMemoryEventStore>();
            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, InMemoryKeyValueStorage<QuestionnaireDocument>>();
            registry.BindAsSingleton(typeof(ICacheStorage<,>), typeof(InMemoryCacheStorage<,>));
            registry.BindAsSingleton(typeof(IPlainStorageAccessor<>), typeof(InMemoryPlainStorageAccessor<>));
            registry.BindAsSingleton<IQuestionnaireStorage, QuestionnaireStorage>();
            registry.Bind<ITranslationStorage, TranslationStorage>();
            registry.BindAsSingleton<IQuestionnaireImportService, QuestionnaireImportService>();

            registry.Bind<IAudioFileStorage, WebTesterAudioFileStorage>();
            registry.Bind<IImageFileStorage, WebTesterImageFileStorage>();

            registry.Bind<EvictionService>();

            // TODO: Find a generic place for each of the dependencies below
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionnaireAssemblyAccessor, WebTesterQuestionnaireAssemblyAccessor>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();
            registry.BindAsSingleton<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();
        }
        
        public static Type[] HubPipelineModules => new[]
        {
            typeof(SignalrErrorHandler),
            typeof(WebInterviewStateManager),
            typeof(WebInterviewConnectionsCounter)
        };

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
