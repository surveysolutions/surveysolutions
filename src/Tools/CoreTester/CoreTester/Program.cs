using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using CommandLine;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Ninject;
using Ninject.Modules;
using NLog;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.JsonConversion;
using WB.Enumerator.Native.Questionnaire;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;


namespace CoreTester
{
    class Program
    {
        [Verb("run", HelpText = "Test core.")]
        protected class CoreTestOptions
        {
            [Option('c', "connection", Required = true, HelpText = "Connection string to DB")]
            public string ConnectionString { get; set; }
        }

        [Verb("dump", HelpText = "Dump debug information in folder")]
        protected class DumpDebugInformationOptions
        {
            [Option('c', "connection", Required = true, HelpText = "Connection string to DB")]
            public string ConnectionString { get; set; }
        }

        static int Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Application started");

            return Parser.Default
                .ParseArguments<CoreTestOptions, DumpDebugInformationOptions>(args)
                .MapResult(
                (CoreTestOptions o) => RunCoreTestOptions(o), 
                (DumpDebugInformationOptions o) => RunDumpDebugInformationOptions(o), errs =>
                {
                    foreach (var error in errs)
                    {
                        Console.WriteLine(error);    
                    }
                
                    return 1;
                });
        }

        private static int RunDumpDebugInformationOptions(DumpDebugInformationOptions opts)
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"started at {DateTime.Now}");
            DbConnectionStringBuilder db = new DbConnectionStringBuilder {ConnectionString = opts.ConnectionString};
            var serverName = db["Database"].ToString();
            Console.WriteLine(serverName);
            Console.WriteLine();

            IKernel container = NinjectConfig.CreateKernel(opts.ConnectionString.Trim('"'));

            DebugInformationDumper dumper = container.Get<DebugInformationDumper>();

            return dumper.Run(serverName);
        }

        private static int RunCoreTestOptions(CoreTestOptions opts)
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"started at {DateTime.Now}");
            DbConnectionStringBuilder db = new DbConnectionStringBuilder {ConnectionString = opts.ConnectionString};
            var serverName = db["User Id"].ToString();
            Console.WriteLine(serverName);
            Console.WriteLine();

            IKernel container = NinjectConfig.CreateKernel(opts.ConnectionString.Trim('"'));

            CoreTestRunner coreTestRunner = container.Get<CoreTestRunner>();

            DebugInformationDumper dumper = container.Get<DebugInformationDumper>();

            var result = coreTestRunner.Run(serverName);
            dumper.Run(serverName);

            return result;
        }
    }

    public class NinjectConfig
    {
        public static IKernel CreateKernel(string connectionString)
        {
            var cacheSettings = new ReadSideCacheSettings(1024, 512);
            var mappingAssemblies = new List<Assembly> {typeof(HeadquartersBoundedContextModule).Assembly};

            var eventStoreSettings = new PostgreConnectionSettings
            {
                ConnectionString = connectionString,
                SchemaName = "events"
            };

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings
            {
                ConnectionString = connectionString,
                SchemaName = "plainstore",
                DbUpgradeSettings =
                    new DbUpgradeSettings(typeof(CoreTestRunner).Assembly, typeof(CoreTestRunner).Namespace),
                MappingAssemblies = new List<Assembly>
                {
                    typeof(HeadquartersBoundedContextModule).Assembly
                }
            };

            var kernel = new StandardKernel(
                new NinjectSettings {InjectNonPublic = true},
                new NcqrsModule().AsNinject(),
                new NLogLoggingModule().AsNinject(),
                new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new DataCollectionSharedKernelModule().AsNinject(),
                new PostgresKeyValueModule(cacheSettings).AsNinject(),
                new PostgresReadSideModule(
                    connectionString,
                    PostgresReadSideModule.ReadSideSchemaName,
                    new DbUpgradeSettings(typeof(CoreTestRunner).Assembly, typeof(CoreTestRunner).Namespace),
                    cacheSettings,
                    mappingAssemblies,
                    runInitAndMigrations: false
                    ).AsNinject(),
                new PostgresPlainStorageModule(postgresPlainStorageSettings).AsNinject(),
                new CoreTesterdule(eventStoreSettings).AsNinject());
            return kernel;
        }
    }

    internal class ServiceLocationModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NativeNinjectServiceLocatorAdapter(this.Kernel));

            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);
        }
    }

    public class CoreTesterdule : IModule
    {
        private readonly PostgreConnectionSettings eventStoreSettings;

        public CoreTesterdule(PostgreConnectionSettings eventStoreSettings)
        {
            this.eventStoreSettings = eventStoreSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IEventTypeResolver>(() =>
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(HeadquartersBoundedContextModule).Assembly));

            registry.BindAsSingletonWithConstructorArgument<ILiteEventBus, NcqrCompatibleEventDispatcher>(
                "eventBusSettings", new EventBusSettings
                {
                    DisabledEventHandlerTypes = Array.Empty<Type>(),
                    EventHandlerTypesWithIgnoredExceptions = Array.Empty<Type>(),
                    IgnoredAggregateRoots = new HashSet<string>()
                });
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewNotificationService>();
            registry.BindAsSingleton<IEventBus, InProcessEventBus>();

            registry
                .BindAsSingleton<IEventSourcedAggregateRootRepository, IAggregateRootCacheCleaner,
                    EventSourcedAggregateRootRepositoryWithWebCache>();

            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();

            registry.BindToConstant(() => JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    AssembliesToInclude =
                    {
                        typeof(WebInterviewModule).Assembly,
                        typeof(CategoricalOption).Assembly
                    }
                }
            }));

            registry.BindAsSingletonWithConstructorArgument<IStreamableEventStore, PostgresEventStore>(
                "connectionSettings", this.eventStoreSettings);
            registry.BindToMethod<IEventStore>(context => context.Get<IStreamableEventStore>());

            // TODO: Find a generic place for each of the dependencies below
            registry.Bind<IInterviewExpressionStatePrototypeProvider, InterviewExpressionStatePrototypeProvider>();
            registry.Bind<ITranslationManagementService, TranslationManagementService>();
            registry.Bind<ISubstitutionTextFactory, SubstitutionTextFactory>();
            registry.Bind<ISubstitutionService, SubstitutionService>();
            registry.Bind<IInterviewTreeBuilder, InterviewTreeBuilder>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IQuestionOptionsRepository, QuestionnaireQuestionOptionsRepository>();
            registry.BindAsSingleton<IInterviewExpressionStateUpgrader, InterviewExpressionStateUpgrader>();
            registry.Bind<IVariableToUIStringService, VariableToUIStringService>();

            registry.Unbind<ICommandService>();
            registry.BindAsSingleton<ICommandService, CustomCommandService>();

            registry.Bind<IAssemblyService, AssemblyService>();
            registry.BindAsSingleton<IQuestionnaireAssemblyAccessor, QuestionnaireAssemblyAccessor>();

            registry.BindToMethod<IInterviewAnswerSerializer>(() => new NewtonInterviewAnswerJsonSerializer());

            registry.Bind<IQuestionnaireBrowseViewFactory, QuestionnaireBrowseViewFactory>();

            registry.BindAsSingleton<IQuestionnaireStorage, UpdatedQuestionnaireStorage>();
            registry.Bind<CoreTestRunner>();
            registry.Bind<DebugInformationDumper>();

            registry.Bind<ISerializer, NewtonJsonSerializer>();
             
            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<CreateInterview>(command => command.InterviewId,
                    (command, aggregate) => aggregate.CreateInterview(command))
                .Handles<AnswerDateTimeQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerDateTimeQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerGeoLocationQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerGeoLocationQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Latitude, command.Longitude, command.Accuracy,
                        command.Altitude, command.Timestamp))
                .Handles<AnswerMultipleOptionsLinkedQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerMultipleOptionsLinkedQuestion(command.UserId,
                        command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVectors))
                .Handles<AnswerMultipleOptionsQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerMultipleOptionsQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.SelectedValues))
                .Handles<AnswerYesNoQuestion>(command => command.InterviewId,
                    aggregate => aggregate.AnswerYesNoQuestion)
                .Handles<AnswerNumericIntegerQuestionCommand>(command => command.InterviewId,
                    aggregate => aggregate.AnswerNumericIntegerQuestion)
                .Handles<AnswerNumericRealQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerNumericRealQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerPictureQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerPictureQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.PictureFileName))
                .Handles<AnswerQRBarcodeQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerQRBarcodeQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerSingleOptionLinkedQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerSingleOptionLinkedQuestion(command.UserId,
                        command.QuestionId, command.RosterVector, command.AnswerTime, command.SelectedRosterVector))
                .Handles<AnswerSingleOptionQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerSingleOptionQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.SelectedValue))
                .Handles<AnswerTextListQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerTextListQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answers))
                .Handles<AnswerTextQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerTextQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.Answer))
                .Handles<AnswerAudioQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerAudioQuestion(command.UserId, command.QuestionId,
                        command.RosterVector, command.AnswerTime, command.FileName, command.Length))
                .Handles<RemoveAnswerCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.RemoveAnswer(command.QuestionId, command.RosterVector,
                        command.UserId, command.RemoveTime))
                .Handles<AnswerAreaQuestionCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.AnswerAreaQuestion(command))
                .Handles<CommentAnswerCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.CommentAnswer(command.UserId, command.QuestionId,
                        command.RosterVector, command.CommentTime, command.Comment))
                .Handles<CompleteInterviewCommand>(command => command.InterviewId,
                    (command, aggregate) => aggregate.Complete(command.UserId, command.Comment, command.CompleteTime))
                .Handles<SwitchTranslation>(command => command.InterviewId, aggregate => aggregate.SwitchTranslation);

            //Designer
            registry.Bind<IExpressionProcessorGenerator, QuestionnaireExpressionProcessorGenerator>();
            registry.Bind<IExpressionsGraphProvider, ExpressionsGraphProvider>();
            registry.Bind<IExpressionsPlayOrderProvider, ExpressionsPlayOrderProvider>();
            registry.Bind<IMacrosSubstitutionService, MacrosSubstitutionService>();

            registry.BindAsSingleton<IExpressionProcessor, RoslynExpressionProcessor>();
        }
    }

    public static class ModuleExtensions
    {
        public static NinjectModule AsNinject<TModule>(this TModule module)
            where TModule : IModule
        {
            return new NinjectModuleAdapter<TModule>(module);
        }
    }
}