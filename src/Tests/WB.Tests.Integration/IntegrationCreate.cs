using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;
using WB.Tests.Abc;
using Configuration = NHibernate.Cfg.Configuration;
using Environment = System.Environment;

namespace WB.Tests.Integration
{
    internal static class IntegrationCreate
    {
        public static CodeGenerator CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
        {
            return new CodeGenerator(
                macrosSubstitutionService ?? DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>(),
                new FileSystemIOAccessor(), 
                GetCompilerSettingsStub());
        }

        public static string CompileAssembly(QuestionnaireDocument questionnaireDocument)
        {
            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    CodeGenerator(),
                    CodeGeneratorV2(),
                    new DynamicCompilerSettingsProvider());

            var latestSupportedVersion = DesignerEngineVersionService().LatestSupportedVersion;
            var emitResult = 
                expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument,  
                    latestSupportedVersion, 
                    out var resultAssembly);

            if (!emitResult.Success || string.IsNullOrEmpty(resultAssembly))
                throw new Exception(
                    $"Errors on IInterviewExpressionState generation:{Environment.NewLine}"
                    + string.Join(Environment.NewLine, emitResult.Diagnostics.Select((d, i) => $"{i + 1}. {d.Message}")));
            return resultAssembly;
        }

        public static CodeGeneratorV2 CodeGeneratorV2()
        {
            return new CodeGeneratorV2(CodeGenerationModelsFactory());
        }

        public static CodeGenerationModelsFactory CodeGenerationModelsFactory()
        {
            return new CodeGenerationModelsFactory(
                    DefaultMacrosSubstitutionService(),
                    ServiceLocator.Current.GetInstance<ILookupTableService>(),
                    new QuestionTypeToCSharpTypeMapper());
        }

        public static ExpressionsPlayOrderProvider ExpressionsPlayOrderProvider(
            IExpressionProcessor expressionProcessor = null,
            IMacrosSubstitutionService macrosSubstitutionService = null)
        {
            return new ExpressionsPlayOrderProvider(
                new ExpressionsGraphProvider(
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                macrosSubstitutionService ?? DefaultMacrosSubstitutionService()));
        }

        private static ICompilerSettings GetCompilerSettingsStub()
            => System.Environment.MachineName.ToLower() == "powerglide" // TLK's :)
                ? Mock.Of<ICompilerSettings>(settings
                    => settings.EnableDump == false
                    && settings.DumpFolder == "C:/Projects/Data/Tests/CodeDump")
                : Mock.Of<ICompilerSettings>();

        public static IMacrosSubstitutionService DefaultMacrosSubstitutionService()
        {
            var macrosSubstitutionServiceMock = new Mock<IMacrosSubstitutionService>();
            macrosSubstitutionServiceMock.Setup(
                x => x.InlineMacros(It.IsAny<string>(), It.IsAny<IEnumerable<Macro>>()))
                .Returns((string e, IEnumerable<Macro> macros) =>
                {
                    return e;
                });

            return macrosSubstitutionServiceMock.Object;
        }

        public static Interview Interview(
            Guid? questionnaireId = null,
            IQuestionnaireStorage questionnaireRepository = null, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                expressionProcessorStatePrototypeProvider ?? Mock.Of<IInterviewExpressionStatePrototypeProvider>(),
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder());

            interview.CreateInterview(Create.Command.CreateInterview(
                interviewId: interview.EventSourceId, 
                userId: new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId: questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                version: 1,
                answers:new List<InterviewAnswer>(),
                supervisorId: new Guid("D222D222D222D222D222D222D222D222")));

            return interview;
        }

        public static StatefulInterview PreloadedInterview(
            PreloadedDataDto preloadedData,
            Guid? questionnaireId = null,
            IQuestionnaireStorage questionnaireRepository = null, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new StatefulInterview(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                expressionProcessorStatePrototypeProvider ?? Mock.Of<IInterviewExpressionStatePrototypeProvider>(),
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder());
          
            interview.CreateInterview(Create.Command.CreateInterview(
                interviewId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                questionnaireId: questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                version: 1,
                answers: preloadedData.Answers,
                //answersTime: new DateTime(2012, 12, 20),
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: Create.Entity.InterviewKey()));

            return interview;
        }

        public static StatefulInterview StatefulInterview(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 7);

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireIdentity.QuestionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaireDocument),
                questionnaireIdentity.Version
            );

            return StatefulInterview(
                questionnaireIdentity: questionnaireIdentity,
                questionnaireRepository: questionnaireRepository);
        }

        public static StatefulInterview StatefulInterview(QuestionnaireIdentity questionnaireIdentity,
            IQuestionnaireStorage questionnaireRepository = null, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null,
            List<InterviewAnswer> answersOnPrefilledQuestions = null)
        {
            var interview = new StatefulInterview(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                expressionProcessorStatePrototypeProvider ?? Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues,
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder());

            interview.CreateInterview(Create.Command.CreateInterview(
                interviewId: interview.EventSourceId,
                userId: new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId: questionnaireIdentity?.QuestionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                version: questionnaireIdentity?.Version ?? 1,
                answers: answersOnPrefilledQuestions ?? new List<InterviewAnswer>(),
                //answersTime: new DateTime(2012, 12, 20),
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: Create.Entity.InterviewKey()));

          return interview;
        }

        public static CommittedEvent CommittedEvent(string origin = null, 
            Guid? eventSourceId = null,
            IEvent payload = null,
            Guid? eventIdentifier = null, 
            int eventSequence = 1)
        {
            return new CommittedEvent(
                Guid.Parse("33330000333330000003333300003333"),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                0,
                payload ?? Mock.Of<IEvent>());
        }

        public static CommittedEventStream CommittedEventStream(Guid eventSourceId, IEnumerable<UncommittedEvent> events)
        {
            return new CommittedEventStream(eventSourceId,
                events
                    .Select(x => IntegrationCreate.CommittedEvent(payload: x.Payload,
                        eventSourceId: x.EventSourceId,
                        eventSequence: x.EventSequence)));
        }

        public static SequentialCommandService SequentialCommandService(IEventSourcedAggregateRootRepository repository = null, ILiteEventBus eventBus = null, IAggregateSnapshotter snapshooter = null)
        {
            return new SequentialCommandService(
                repository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus ?? Mock.Of<ILiteEventBus>(),
                snapshooter ?? Mock.Of<IAggregateSnapshotter>(), Mock.Of<IServiceLocator>(),
                Mock.Of<IPlainAggregateRootRepository>(),
                new AggregateLock(),
                Mock.Of<IAggregateRootCacheCleaner>());
        }

        public static Answer Answer(string answer, decimal value, decimal? parentValue = null)
        {
            return new Answer()
            {
                AnswerText = answer,
                AnswerValue = value.ToString(),
                ParentValue = parentValue.HasValue ? parentValue.ToString() : null
            };
        }

        public static FixedRosterTitle FixedTitle(decimal value, string title = null)
        {
            return new FixedRosterTitle(value, title ?? ("Roster " + value));
        }

        public static LookupTableContent LookupTableContent(string[] variableNames, params LookupTableRow[] rows)
        {
            return new LookupTableContent
            {
                VariableNames = variableNames,
                Rows = rows
            };
        }

        public static LookupTableRow LookupTableRow(long rowcode, decimal?[] values)
        {
            return new LookupTableRow
                   {
                       RowCode = rowcode,
                       Variables = values
            };
        }

        public static PostgresReadSideKeyValueStorage<TEntity> PostgresReadSideKeyValueStorage<TEntity>(
            IUnitOfWork sessionProvider = null, UnitOfWorkConnectionSettings postgreConnectionSettings = null)
            where TEntity : class, IReadSideRepositoryEntity
        {
            return new PostgresReadSideKeyValueStorage<TEntity>(
                sessionProvider ?? Mock.Of<IUnitOfWork>(),
                postgreConnectionSettings ?? new UnitOfWorkConnectionSettings(),
                Mock.Of<ILogger>(),
                new EntitySerializer<TEntity>());
        }

        public static ISessionFactory SessionFactory(string connectionString, IEnumerable<Type> painStorageEntityMapTypes, bool executeSchemaUpdate, string schemaName = null)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = connectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });

            cfg.AddDeserializedMapping(GetMappingsFor(painStorageEntityMapTypes), "Plain");
            if (executeSchemaUpdate)
            {
                var update = new SchemaUpdate(cfg);
                update.Execute(true, true);
            }
            if (schemaName != null)
            {
                cfg.SetProperty(NHibernate.Cfg.Environment.DefaultSchema, schemaName);
            }

            return cfg.BuildSessionFactory();
        }

        private static HbmMapping GetMappingsFor(IEnumerable<Type> painStorageEntityMapTypes)
        {
            var mapper = new ModelMapper();
            mapper.AddMappings(painStorageEntityMapTypes);
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo)member.LocalMember;
                if (propertyInfo.PropertyType == typeof(string))
                {
                    customizer.Type(NHibernateUtil.StringClob);
                }
            };
            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                var tableName = type.Name.Pluralize();
                customizer.Table(tableName);
            };

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        public static AggregateRootEvent AggregateRootEvent(IEvent evnt)
        {
            var rnd = new Random();
            return new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), "origin", Guid.NewGuid(), Guid.NewGuid(),
                    rnd.Next(1, 10000000), DateTime.UtcNow, rnd.Next(1, 1000000), evnt));
        }

        public static CumulativeReportStatusChange CumulativeReportStatusChange(Guid? questionnaireId=null, long? questionnaireVersion=null, DateTime? date = null, Guid? interviewId = null, long eventSequence = 1)
        {
            return new CumulativeReportStatusChange(Guid.NewGuid().FormatGuid(), questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1, date??DateTime.Now, InterviewStatus.Completed, 1, interviewId ?? Guid.NewGuid(), eventSequence);
        }

        public static DesignerEngineVersionService DesignerEngineVersionService()
            => new DesignerEngineVersionService();

        public static PostgreReadSideStorage<TEntity> PostgresReadSideRepository<TEntity>(
            IUnitOfWork sessionProvider = null)
            where TEntity : class, IReadSideRepositoryEntity
        {
            return new PostgreReadSideStorage<TEntity>(
                sessionProvider ?? Mock.Of<IUnitOfWork>(),
                Mock.Of<ILogger>());
        }

        public static AnswerNotifier AnswerNotifier(ILiteEventRegistry registry = null)
            => new AnswerNotifier(registry ?? Abc.Create.Service.LiteEventRegistry());

        public static IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationCondition(Identity questionIdentity)
            => new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {
                    questionIdentity,
                    new List<FailedValidationCondition>() {new FailedValidationCondition(0)}
                }
            };

        public static PreloadedDataDto PreloadedDataDto(params PreloadedLevelDto[] levels)
        {
            return new PreloadedDataDto(levels);
        }

        public static PreloadedLevelDto PreloadedLevelDto(RosterVector rosterVector, Dictionary<Guid, AbstractAnswer> answers)
        {
            return new PreloadedLevelDto(rosterVector, answers);
        }
    }
}
