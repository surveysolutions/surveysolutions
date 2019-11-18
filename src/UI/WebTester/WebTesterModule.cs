﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Documents;
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
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
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
using WB.UI.WebTester.Infrastructure.AppDomainSpecific;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.UI.WebTester
{
    public class WebTesterModule : IModule
    {
        private readonly string designerAddress;

        public WebTesterModule(string designerAddress)
        {
            this.designerAddress = designerAddress.TrimEnd('/');
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>("eventBusSettings", new EventBusSettings
            {
                DisabledEventHandlerTypes = Array.Empty<Type>(),
                EventHandlerTypesWithIgnoredExceptions = Array.Empty<Type>(),
                IgnoredAggregateRoots = new List<string>()
            });

            registry.Bind<WebTesterStatefulInterview>();
            registry.Bind<IInterviewFactory, InterviewFactory>();

            registry.BindAsSingleton<IEvictionObservable, IEvictionNotifier, TokenEviction>();

            registry.Bind<IEnumeratorGroupStateCalculationStrategy, EnumeratorGroupGroupStateCalculationStrategy>();
            registry.Bind<ISupervisorGroupStateCalculationStrategy, SupervisorGroupStateCalculationStrategy>();
            registry.BindAsSingleton<IEventSourcedAggregateRootRepository, IAggregateRootCacheFiller, IAggregateRootCacheCleaner, WebTesterAggregateRootRepository>();
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
            registry.BindAsSingleton<ICommandService, WebTesterCommandService>();
            registry.BindInPerLifetimeScope<IWebTesterTranslationService, WebTesterTranslationService>();
            registry.BindInPerLifetimeScope<IWebTesterTranslationStorage , WebTesterTranslationStorage>();
            registry.BindInPerLifetimeScope<IQuestionnaireStorage , WebTesterQuestionnaireStorage>();

            registry.BindAsSingleton<IAppdomainsPerInterviewManager, AppdomainsPerInterviewManager>();
            registry.Bind<IVirtualPathService, VirtualPathService>();
            registry.Bind<ISerializer, NewtonJsonSerializer>();
            registry.BindAsSingleton<IScenarioSerializer, ScenarioSerializer>();

            registry.BindToMethod<IServiceLocator>(() => ServiceLocator.Current);

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
                    BaseAddress = new Uri(designerAddress),
                    Timeout = TimeSpan.FromMinutes(3)
                },
                new RefitSettings
                {                   
                    ContentSerializer = new JsonContentSerializer(new JsonSerializerSettings { 
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
                        SerializationBinder = new OldToNewAssemblyRedirectSerializationBinder()
                    })
                }));

            foreach (var type in HubPipelineModules)
            {
                registry.BindAsSingleton(typeof(IPipelineModule), type);
                registry.BindAsSingleton(type, type);
            }

            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();
            registry.Bind<IWebNavigationService, WebNavigationService>();

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

            registry.BindAsSingleton<IInMemoryEventStore, WebTesterEventStore>();
            registry.BindToMethod<IEventStore>(f => f.Resolve<IInMemoryEventStore>());

            registry.BindAsSingleton<IPlainKeyValueStorage<QuestionnaireDocument>, InMemoryKeyValueStorage<QuestionnaireDocument>>();
            registry.BindAsSingleton(typeof(ICacheStorage<,>), typeof(InMemoryCacheStorage<,>));
            registry.BindAsSingleton(typeof(IPlainStorageAccessor<>), typeof(InMemoryPlainStorageAccessor<>));
            registry.Bind<ITranslationStorage, TranslationStorage>();
            registry.BindAsSingleton<IQuestionnaireImportService, QuestionnaireImportService>();

            registry.Bind<IAudioFileStorage, WebTesterAudioFileStorage>();
            registry.Bind<IImageFileStorage, WebTesterImageFileStorage>();

            // TODO: Find a generic place for each of the dependencies below
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.BindInPerLifetimeScope<IQuestionnaireAssemblyAccessor, WebTesterQuestionnaireAssemblyAccessor>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();
            registry.BindAsSingleton<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();
        }
        
        public static Type[] HubPipelineModules => new[]
        {
            typeof(WebInterviewVersionChecker),
            //typeof(WebInterviewStateManager),
            typeof(WebInterviewConnectionsCounter)
        };

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
             CommandRegistry
               .Setup<StatefulInterview>()
               .InitializesWith<CreateInterview>(command => command.InterviewId, (command, aggregate) => aggregate.CreateInterview(command))
               .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
               .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Latitude, command.Longitude, command.Accuracy, command.Altitude, command.Timestamp))
               .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVectors))
               .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValues))
               .Handles<AnswerYesNoQuestion>(command => command.InterviewId, aggregate => aggregate.AnswerYesNoQuestion)
               .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId, aggregate => aggregate.AnswerNumericIntegerQuestion)
               .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
               .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.PictureFileName))
               .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
               .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedRosterVector))
               .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.SelectedValue))
               .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answers))
               .Handles<AnswerTextQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Answer))
               .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.FileName, command.Length))
               .Handles<RemoveAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector, command.UserId, command.OriginDate))
               .Handles<AnswerGeographyQuestionCommand>(command => command.InterviewId, (command, aggregate) => aggregate.AnswerAreaQuestion(command))
               .Handles<CommentAnswerCommand>(command => command.InterviewId, (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId, command.RosterVector, command.OriginDate, command.Comment))
               .Handles<CompleteInterviewCommand>(command => command.InterviewId, (command, aggregate) => aggregate.Complete(command.UserId, command.Comment, command.OriginDate))
               .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation);


            return Task.CompletedTask;
        }
    }
}
