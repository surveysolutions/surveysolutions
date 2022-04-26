using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WB.Services.Export.Assignment;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.CsvExport.Implementation;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.Ddi;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Events.Interview.Dtos;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.Services;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.User;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;
using WB.Services.Infrastructure.Tenant;
using AnsweredYesNoOption = WB.Services.Export.Interview.Entities.AnsweredYesNoOption;

namespace WB.Services.Export.Tests
{
    public static class Create
    {
        public static InterviewDiagnosticsInfo InterviewDiagnosticsInfo(
            Guid? interviewId = null,
            string interviewKey = null,
            InterviewStatus status = InterviewStatus.InterviewerAssigned,
            Guid? responsibleId = null,
            string responsibleName = null,
            int numberOfInterviewers = 0,
            int numberRejectionsBySupervisor = 0,
            int numberRejectionsByHq = 0,
            int numberValidQuestions = 0,
            int numberInvalidEntities = 0,
            int numberUnansweredQuestions = 0,
            int numberCommentedQuestions = 0,
            long? interviewDuration = null, 
            int? notAnsweredCount = null)
            => new InterviewDiagnosticsInfo
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                InterviewKey = interviewKey,
                Status = status,
                ResponsibleId = responsibleId ?? Guid.NewGuid(),
                ResponsibleName = responsibleName,
                NumberOfInterviewers = numberOfInterviewers,
                NumberRejectionsBySupervisor = numberRejectionsBySupervisor,
                NumberRejectionsByHq = numberRejectionsByHq,
                NumberValidQuestions = numberValidQuestions,
                NumberInvalidEntities = numberInvalidEntities,
                NumberUnansweredQuestions = numberUnansweredQuestions,
                NumberCommentedQuestions = numberCommentedQuestions,
                InterviewDuration = interviewDuration,
                NotAnsweredCount = notAnsweredCount
            };

        public static DiagnosticsExporter DiagnosticsExporter(ICsvWriter csvWriter = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITenantApi<IHeadquartersApi> headquartersApi = null)
        {
            return new DiagnosticsExporter(Mock.Of<IOptions<ExportServiceSettings>>(x => x.Value == new ExportServiceSettings()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                headquartersApi ?? HeadquartersApi());
        }

        public static ITenantApi<IHeadquartersApi> TenantHeadquartersApi(IHeadquartersApi api)
        {
            return Mock.Of<ITenantApi<IHeadquartersApi>>(x => x.For(It.IsAny<TenantInfo>()) == api);
        }

        public static ITenantApi<IHeadquartersApi> HeadquartersApi()
        {
            return Mock.Of<ITenantApi<IHeadquartersApi>>();
        }

        public static TenantInfo Tenant(string baseUrl = null, string id = null, string name = null)
        {
            return new TenantInfo(baseUrl, id, name);
        }

        public static QuestionnaireExportStructure QuestionnaireExportStructure(string questionnaireId = "")
        {
            return new QuestionnaireExportStructure
            (
                questionnaireId
            );
        }

        public static HeaderStructureForLevel HeaderStructureForLevel()
        {
            return new HeaderStructureForLevel( new ValueVector<Guid>(), "", "" );
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, long version = 5, string variableName = null, params IQuestionnaireEntity[] children)
        {
            var questionnaireDocument = new QuestionnaireDocument(children.ToList())
            {
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variableName,
            };
            questionnaireDocument.QuestionnaireId = new QuestionnaireId(questionnaireDocument.PublicKey.FormatGuid() + "$" + version);

            return questionnaireDocument;
        }

        public static TextQuestion TextQuestion(Guid? id = null,
            string questionText = null,
            string variable = null,
            string variableLabel = null,
            string instructions = null)
        {
            return new TextQuestion
            {
                QuestionText = questionText,
                VariableName = variable ?? Guid.NewGuid().FormatGuid(),
                PublicKey = id ?? Guid.NewGuid(),
                VariableLabel = variableLabel,
                Instructions = instructions,
                QuestionType = QuestionType.Text
            };
        }

        public static QuestionnaireExportStructure QuestionnaireExportStructure(QuestionnaireDocument questionnaire)
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor
                .Setup(x => x.MakeValidFileName(It.IsAny<string>()))
                .Returns((string f) => f);

            var QuestionnaireExportStructureFactory = new QuestionnaireExportStructureFactory(Mock.Of<IQuestionnaireStorage>());
            return QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);
        }

        public static InterviewAction InterviewSummary(InterviewExportedAction exportedAction = InterviewExportedAction.ApprovedBySupervisor,
            string originatorName = "inter",
            UserRoles originatorRole = UserRoles.Interviewer,
            Guid? interviewId = null,
            DateTime? timestamp = null,
            string key = null,
            string interviewerName = "inter",
            string supervisorName = "supervisor")
            => new InterviewAction
            {
                Status = exportedAction,
                InterviewId = interviewId ?? Guid.NewGuid(),
                Timestamp = timestamp ?? DateTime.Now,
                Key = key,
                StatusChangeOriginatorRole = originatorRole,
                StatusChangeOriginatorName = originatorName,
                InterviewerName = interviewerName,
                SupervisorName = supervisorName,

            };

        public static InterviewActionsExporter InterviewActionsExporter(ITenantApi<IHeadquartersApi> tenantApi,
            ICsvWriter csvWriter = null)
        {
            return new InterviewActionsExporter(InterviewDataExportSettings(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                tenantApi ?? HeadquartersApi());
        }

        public static IOptions<ExportServiceSettings> InterviewDataExportSettings()
        {
            return Mock.Of<IOptions<ExportServiceSettings>>(x => x.Value == new ExportServiceSettings());
        }

        public static TabularFormatExportService ReadSideToTabularFormatExportService(
            QuestionnaireExportStructure questionnaireExportStructure,
            IFileSystemAccessor fileSystemAccessor = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IAssignmentActionsExporter assignmentsActionsExporter = null)
        {
            var defaultQuestionnaireStorage = new Mock<IQuestionnaireStorage>();
            var questionnaireDocument = Create.QuestionnaireDocument(Guid.Parse("11111111111111111111111111111111"), 555);
            defaultQuestionnaireStorage.SetupIgnoreArgs(x => x.GetQuestionnaireAsync(null, null, CancellationToken.None))
                .ReturnsAsync(questionnaireDocument);
            
            var defaultInterviewsSource = new Mock<IInterviewsToExportSource>();
            defaultInterviewsSource.SetReturnsDefault(new List<InterviewToExport>());
        
            return new TabularFormatExportService(Mock.Of<ILogger<TabularFormatExportService>>(),
                defaultInterviewsSource.Object,
                Mock.Of<IInterviewsExporter>(),
                Mock.Of<ICommentsExporter>(),
                Mock.Of<IDiagnosticsExporter>(),
                Mock.Of<IInterviewActionsExporter>(),
                Mock.Of<IQuestionnaireExportStructureFactory>(x => x.GetQuestionnaireExportStructureAsync(It.IsAny<TenantInfo>(), It.IsAny<QuestionnaireId>(), It.IsAny<Guid?>()) == Task.FromResult(questionnaireExportStructure)),
                questionnaireStorage ?? defaultQuestionnaireStorage.Object,
                Mock.Of<IProductVersion>(),
                Mock.Of<IPdfExporter>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                assignmentsActionsExporter ?? Mock.Of<IAssignmentActionsExporter>(),
                Mock.Of<IQuestionnaireBackupExporter>(),
                Mock.Of<IDdiMetadataFactory>());
        }

        public static CommentsExporter CommentsExporter()
        {
            return new CommentsExporter(Create.InterviewDataExportSettings(),
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICsvWriter>(),
                Create.HeadquartersApi(),
                Mock.Of<ILogger<CommentsExporter>>());
        }

        public static IInterviewErrorsExporter InterviewErrorsExporter()
        {
            return new InterviewErrorsExporter(Mock.Of<ICsvWriter>(), Mock.Of<IFileSystemAccessor>());
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null,
            IEnumerable<Answer> options = null,
            Guid? linkedToQuestionId = null,
            string variable = null,
            bool yesNoView = false,
            Guid? linkedToRosterId = null,
            bool areAnswersOrdered = false,
            bool? isFilteredCombobox = null,
            int? maxAnswersCount = null,
            Guid? categoryId = null,
            string variableLabel = null,
            string questionText = null
            )
            => new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                VariableName = variable,
                YesNoView = yesNoView,
                AreAnswersOrdered = areAnswersOrdered,
                IsFilteredCombobox = isFilteredCombobox,
                MaxAllowedAnswers = maxAnswersCount,
                CategoriesId = categoryId,
                VariableLabel = variableLabel,
                QuestionText = questionText,
            };

        public static Answer Option(string text, string value)
        {
            return new Answer
            {
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
            };
        }

        public static ICsvWriter CsvWriter(List<CsvData> writeTo)
        {
            var csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback((string s, IEnumerable<string[]> p, string t) =>
                {
                    writeTo.Add(new CsvData
                    {
                        File = s,
                        Data = p.ToList()
                    });
                });
            return csvWriterMock.Object;
        }

        public class CsvData
        {
            public string File { get; set; }
            public List<string[]> Data { get; set; }
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null,
            string variable = "numeric_question",
            bool isPrefilled = false,
            string questionText = null,
            Guid? linkedToRosterId = null,
            IEnumerable<Answer> specialValues = null,
            IEnumerable<ValidationCondition> validationConditions = null)
        {
            return new NumericQuestion
            {
                QuestionText = questionText ?? "text",
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variable,
                IsInteger = true,
                Featured = isPrefilled,
                LinkedToRosterId = linkedToRosterId,
                Answers = new List<Answer>(specialValues ?? new Answer[] { }),
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>()
            };
        }

        public static InterviewsExporter InterviewsExporter(
            ICsvWriter csvWriter = null,
            IInterviewFactory interviewFactory = null)
        {
            return new InterviewsExporter(new ExportQuestionService(),
                interviewFactory ?? Mock.Of<IInterviewFactory>(),
                Create.InterviewErrorsExporter(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                Mock.Of<IOptions<ExportServiceSettings>>(s => s.Value == new ExportServiceSettings()),
                Mock.Of<ILogger<InterviewsExporter>>());
        }

        public static StaticText StaticText(
            Guid? publicKey = null,
            List<ValidationCondition> validationConditions = null)
            => new StaticText()
            {
                PublicKey = publicKey ?? Guid.NewGuid(),
                ValidationConditions = validationConditions
            };

        public static Identity Identity(string id, params int[] rosterVector)
        {
            return Identity(Guid.Parse(id), rosterVector);
        }

        public static Identity Identity(Guid? id = null, RosterVector rosterVector = null)
        {
            return Identity(id, rosterVector?.Coordinates.ToArray());
        }

        public static Identity Identity(Guid? id = null, params int[] rosterVector)
        {
            return new Identity(id ?? Guid.NewGuid(), rosterVector ?? RosterVector.Empty);
        }
        public static InterviewEntity InterviewEntity(Guid? interviewId = null,
            EntityType entityType = EntityType.Question,
            Identity identity = null,
            int[] invalidValidations = null,
            bool isEnabled = true,
            int? asInt = null,
            DateTime? asDateTime = null,
            double? asDouble = null,
            string asString = null,
            GeoPosition asGps = null,
            int[] asIntArray = null,
            AnsweredYesNoOption[] asYesNo = null,
            InterviewTextListAnswer[] asList = null,
            int[][] asIntMatrix = null)
        {
            return new InterviewEntity
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                EntityType = entityType,
                Identity = identity ?? Create.Identity(),
                InvalidValidations = invalidValidations ?? Array.Empty<int>(),
                IsEnabled = isEnabled,
                AsInt = asInt,
                AsDateTime = asDateTime,
                AsDouble = asDouble,
                AsString = asString,
                AsGps = asGps,
                AsIntArray = asIntArray,
                AsList = asList,
                AsYesNo = asYesNo,
                AsIntMatrix = asIntMatrix
            };
        }

        public static Group Roster(Guid? rosterId = null,
            IEnumerable<IQuestionnaireEntity> children = null,
            string variable = "roster_var",
            Guid? rosterSizeQuestionId = null,
            FixedRosterTitle[] fixedTitles = null,
            RosterSizeSourceType? rosterSizeSourceType = null,
            string[] obsoleteFixedTitles = null)
        {
            var rosterSizeSource = rosterSizeSourceType ?? (rosterSizeQuestionId.HasValue ? RosterSizeSourceType.Question : RosterSizeSourceType.FixedTitles);
            return new Group
            {
                PublicKey = rosterId ?? Guid.NewGuid(),
                Children = children ?? Enumerable.Empty<IQuestionnaireEntity>(),
                VariableName = variable ?? "var1",
                FixedRosterTitles = fixedTitles ??
                                    obsoleteFixedTitles?.Select((i, x) => new FixedRosterTitle(x, i)).ToArray() ??
                                    Array.Empty<FixedRosterTitle>(),
                IsRoster = true,
                RosterSizeQuestionId = rosterSizeQuestionId,
                RosterSizeSource = rosterSizeSource
            };
        }

        public static Group Roster(Guid? rosterId = null, string variable = null, params IQuestionnaireEntity[] children)
        {
            return new Group(children: children.ToList())
            {
                PublicKey = rosterId ?? Guid.NewGuid(),
                VariableName = variable,
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.FixedTitles
            };
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(
            params IQuestionnaireEntity[] children)
        {
            return QuestionnaireDocumentWithOneChapter(chapterId: null, children: children);
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null,
            Guid? id = null,
            string variable = null,
            params IQuestionnaireEntity[] children)
        {
            var questionnaireDocumentWithOneChapter = new QuestionnaireDocument
            {
                Title = "Questionnaire",
                VariableName = variable,
                PublicKey = id ?? Guid.NewGuid(),
                Children = new List<IQuestionnaireEntity>
                {
                    new Group
                    {
                        PublicKey = chapterId.GetValueOrDefault(),
                        Children = children ?? Array.Empty<IQuestionnaireEntity>()
                    }
                }
            };
            questionnaireDocumentWithOneChapter.QuestionnaireId = new QuestionnaireId(questionnaireDocumentWithOneChapter.PublicKey.ToString("N") + "$" + "145");
            questionnaireDocumentWithOneChapter.ConnectChildrenWithParent();
            return questionnaireDocumentWithOneChapter;
        }

        public static List<HeaderColumn> ColumnHeaders(string[] columnNames)
        {
            return columnNames?.Select(x => new HeaderColumn() { Name = x, Title = x }).ToList() ?? new List<HeaderColumn>();
        }

        public static TextListQuestion TextListQuestion(Guid? questionId = null,
            string variable = null,
            string text = "Question T",
            bool preFilled = false,
            string label = null,
            int? maxAnswersCount = null,
            IEnumerable<ValidationCondition> validationConditions = null)
            => new TextListQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionText = text,
                QuestionType = QuestionType.TextList,
                VariableName = variable ?? "vv" + Guid.NewGuid().ToString("N"),
                Featured = preFilled,
                VariableLabel = label,
                ValidationConditions = validationConditions?.ToList(),
                MaxAnswerCount = maxAnswersCount,
            };

        public static IInterviewFactory InterviewFactory()
        {
            return new InterviewFactory(Create.HeadquartersApi(), Create.TenantDbContext());
        }

        public static TenantDbContext TenantDbContext(string databaseName = null, string tenantName = null)
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString("N"))
                .Options;
            var dbContext = new TenantDbContext(
                Mock.Of<ITenantContext>(x => x.Tenant == new TenantInfo
                (
                    "",
                    TenantId.None,
                    tenantName ?? "none",
                    TenantInfo.DefaultWorkspace
                )),
                Mock.Of<IOptions<DbConnectionSettings>>(x => x.Value == new DbConnectionSettings()),
                options);

            return dbContext;
        }


        public static TenantDbContext NpgsqlTenantDbContext(string databaseName = null, string tenantName = null)
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>().Options;
            var dbContext = new TenantDbContext(
                Mock.Of<ITenantContext>(x => x.Tenant == new TenantInfo(
                    "", TenantId.None,
                    tenantName ?? "none", TenantInfo.DefaultWorkspace
                )),
                Mock.Of<IOptions<DbConnectionSettings>>(x => x.Value == new DbConnectionSettings()
                {DefaultConnection = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=export_service_tests;"
                }),
                options);

            return dbContext;
        }

        public static Variable Variable(Guid? id = null, VariableType type = VariableType.LongInteger, string name = "variable")
            => new Variable()
            {
                PublicKey = id ?? Guid.NewGuid(),
                Type = type,
                Name = name
            };

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled = false, string title = null,
            string label = null)
            => new GpsCoordinateQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                VariableName = variable,
                QuestionType = QuestionType.GpsCoordinates,
                Featured = isPrefilled,
                QuestionText = title,
                VariableLabel = label
            };

        public static Answer Answer(string title, int value)
        {
            return new Answer
            {
                AnswerValue = value.ToString(),
                AnswerText = title
            };
        }

        public static Group Group(Guid? groupId = null, string variable = null, params IQuestionnaireEntity[] children)
        {
            return new Group(children: children.ToList())
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                VariableName = variable
            };
        }

        public static DateTimeQuestion DateTimeQuestion(
            Guid? questionId = null,
            string variable = null, string text = null, bool isTimestamp = false)
            => new DateTimeQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionText = text,
                QuestionType = QuestionType.DateTime,
                VariableName = variable,
                IsTimestamp = isTimestamp
            };

        public static NumericQuestion NumericRealQuestion(Guid? id = null,
            string variable = null,
            string questionText = null,
            IEnumerable<ValidationCondition> validationConditions = null
            )
            => new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variable,
                QuestionText = questionText,
                IsInteger = false,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>()
            };

        public static AnsweredYesNoOption AnsweredYesNoOption(decimal value, bool isYes)
        {
            return new AnsweredYesNoOption(value, isYes);
        }

        public static SingleQuestion SingleOptionQuestion(Guid? id = null,
            string questionText = null,
            string variable = null,
            IEnumerable<Answer> options = null,
            Guid? linkedToQuestionId = null,
            Guid? categoryId = null,
            string variableLabel = null)
        {
            return new SingleQuestion
            {
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                QuestionText = questionText,
                VariableName = variable ?? "single",
                LinkedToQuestionId = linkedToQuestionId,
                CategoriesId = categoryId,
                VariableLabel = variableLabel,
            };
        }

        public static QuestionnaireLevelLabels QuestionnaireLevelLabels(string levelName = "level", params DataExportVariable[] variableLabels)
            => new QuestionnaireLevelLabels(levelName, variableLabels, null);

        public static DataExportVariable LabeledVariable(string variableName = "var", string label = "lbl", Guid? questionId = null, params VariableValueLabel[] variableValueLabels)
            => new DataExportVariable(variableName, label, questionId, variableValueLabels, ExportValueType.Unknown);

        public static VariableValueLabel VariableValueLabel(string value = "1", string label = "l1")
            => new VariableValueLabel(value, label);

        public static InterviewReference InterviewReference(
            string questionnaireId = null,
            Guid? interviewId = null,
            InterviewStatus? status = null,
            string key = "",
            DateTime? updateDateUtc = null)
        {
            return new InterviewReference
            {
                QuestionnaireId = questionnaireId ?? Id.gA.FormatGuid(),
                InterviewId = interviewId ?? Guid.NewGuid(),
                Status = status ?? InterviewStatus.Deleted,
                Key = key,
                UpdateDateUtc = updateDateUtc
            };
        }

        public static IQuestionnaireStorage QuestionnaireStorage(QuestionnaireDocument questionnaire)
        {
            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            questionnaireStorage.Setup(x => x.GetQuestionnaireAsync(questionnaire.QuestionnaireId,
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(questionnaire);
            return questionnaireStorage.Object;
        }

        public static EventsFactory Event = new EventsFactory();
        public static EntityFactory Entity = new EntityFactory();

        public static ServiceProvider SetupEventsProcessor(ServiceCollection services, 
            IHeadquartersApi api, 
            bool withDefaultEventsFilter = false,
            bool noEventsHandlerLogger = false)
        {
            services.AddMockObject<ITenantApi<IHeadquartersApi>, IHeadquartersApi>(
                s => s.For(It.IsAny<TenantInfo>()), api);

            services.AddScoped<ITenantContext, TenantContext>();

            if (withDefaultEventsFilter)
            {
                var filerMock = new Mock<IEventsFilter>();
                filerMock.Setup(c => c.FilterAsync(It.IsAny<List<Event>>(), It.IsAny<CancellationToken>()))
                    .Returns<List<Event>, CancellationToken>((e,c) => Task.FromResult(e));

                services.AddTransient(c => filerMock.Object);
            }

            services.AddDbContext<TenantDbContext>(c =>
            {
                c.UseInMemoryDatabase(Guid.NewGuid().ToString("N"));
                c.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }, ServiceLifetime.Singleton);

            services.AddMock<IDataExportProcessesService>();
            services.AddTransient<EventsProcessor>();
            services.AddTransient<IEventsHandler, EventsHandler>();
            services.AddMock<ILogger<EventsProcessor>>();

            if(!noEventsHandlerLogger) services.AddMock<ILogger<EventsHandler>>();
            
            services.AddOptions();

            var provider = services.BuildServiceProvider();
            provider.SetTenant(new TenantInfo("http://localhost", TenantId.None, "test"));
            return provider;
        }

        public static ValueVector<Guid> ValueVector(params Guid[] rosterIds) => new ValueVector<Guid>(rosterIds);

        public static IDatabaseSchemaService DatabaseSchemaService(
            IQuestionnaireSchemaGenerator questionnaireSchemaGenerator = null,
            TenantDbContext dbContext = null)
        {
            return new DatabaseSchemaService(
                questionnaireSchemaGenerator ?? Mock.Of<IQuestionnaireSchemaGenerator>(),
                dbContext ?? Mock.Of<TenantDbContext>());
        }

        public static IAssignmentActionsExporter AssignmentActionsExporter(ICsvWriter csvWriter = null,
            TenantDbContext dbContext = null,
            IUserStorage userStorage = null)
        {
            return new AssignmentActionsExporter(InterviewDataExportSettings(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                dbContext ?? Create.TenantDbContext(),
                userStorage ?? Mock.Of<IUserStorage>());
        }

        public static AssignmentAction AssignmentAction(long globalSequence, int assignmentId, DateTime timestampUtc, AssignmentExportedAction exportedAction,
            Guid originatorId, Guid responsibleId, string oldValue = null, string newValue = null, string comment = null)
        {
            return new AssignmentAction()
            {
                GlobalSequence = globalSequence,
                AssignmentId = assignmentId,
                Status = exportedAction,
                TimestampUtc = timestampUtc,
                OriginatorId = originatorId,
                ResponsibleId = responsibleId,
                OldValue = oldValue,
                NewValue = newValue,
                Comment = comment,
            };
        }

        public static PdfExporter PdfExporter(ITenantApi<IHeadquartersApi> api = null,
            IFileSystemAccessor fileSystem = null)
        {
            return new PdfExporter(api ?? HeadquartersApi(),
                fileSystem ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ILogger<PdfExporter>>());
        }

        public static QuestionnaireBackupExporter QuestionnaireBackupExporter(IFileSystemAccessor fileSystem = null,
            ITenantApi<IHeadquartersApi> tenantApi = null)
        {
            return new QuestionnaireBackupExporter(fileSystem ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ILogger<QuestionnaireBackupExporter>>(),
                tenantApi?? Mock.Of<ITenantApi<IHeadquartersApi>>());
        }

        internal static IInterviewsDoFilesExporter InterviewsDoFilesExporter(IFileSystemAccessor fileSystemAccessor, QuestionnaireLabelFactory questionnaireLabelFactory = null)
        {
            return new InterviewsDoFilesExporter(
                fileSystemAccessor, 
                questionnaireLabelFactory ?? Create.QuestionnaireLabelFactory()
                );
        }

        private static IQuestionnaireLabelFactory QuestionnaireLabelFactory()
        {
            return new QuestionnaireLabelFactory();
        }

        public static ExportExportFileNameService ExportExportFileNameService(IFileSystemAccessor fileSystemAccessor = null,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new ExportExportFileNameService(
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
        }
    }

    public class EventsFactory
    {
        private int globalSeq = 0;
        private int eventSeq = 0;

        public Event Event(IEvent payload, Guid? interviewId = null, int? globalSeq = null, int? eventSeq = null)
        {
            return new Event()
            {
                EventSourceId = interviewId ?? Guid.NewGuid(),
                Payload = payload,
                GlobalSequence = globalSeq ?? ++this.globalSeq,
                Sequence = eventSeq ?? ++this.eventSeq,
                EventTypeName = payload.GetType().Name,
                EventTimeStamp = DateTime.UtcNow
            };
        }

        public Event CreatedInterview(Guid? interviewId = null, int? globalSeq = null, int? eventSeq = null)
        {
            return Event(new InterviewCreated(), interviewId, globalSeq, eventSeq);
        }

        public Event InterviewOnClientCreated(Guid? interviewId = null, int? globalSeq = null, int? eventSeq = null)
        {
            return Event(new InterviewOnClientCreated(), interviewId, globalSeq, eventSeq);
        }

        public Event TextQuestionAnswered(Guid questionId, string answer = "answer", Guid? interviewId = null, int? globalSeq = null, int? eventSeq = null)
        {
            return Event(new TextQuestionAnswered() { Answer = answer, QuestionId = questionId, RosterVector = RosterVector.Empty }, interviewId, globalSeq, eventSeq);
        }

        public Event NumericIntegerQuestionAnswered(Guid questionId, int answer, Guid interviewId)
        {
            return Event(new NumericIntegerQuestionAnswered() { Answer = answer, QuestionId = questionId, RosterVector = RosterVector.Empty }, interviewId, null, null);
        }

        public Event NumericRealQuestionAnswered(Guid questionId, decimal answer, Guid interviewId, RosterVector rosterVector)
        {
            return Event(new NumericRealQuestionAnswered() { Answer = answer, QuestionId = questionId, RosterVector = rosterVector }, interviewId, null, null);
        }

        public Event TextListQuestionAnswered(Guid questionId, Tuple<decimal, string>[] answer, Guid interviewId)
        {
            return Event(new TextListQuestionAnswered() { Answers = answer, QuestionId = questionId, RosterVector = RosterVector.Empty }, interviewId, null, null);
        }

        public Event RosterInstancesAdded(Guid interviewId, Guid rosterId, RosterVector outerRosterVector, int rosterInstanceId)
        {
            return Event(new RosterInstancesAdded() { Instances = new[] { new AddedRosterInstance() { GroupId = rosterId, OuterRosterVector = outerRosterVector, RosterInstanceId = rosterInstanceId }, } }, interviewId, null, null);
        }

        public Event RosterInstancesRemoved(Guid interviewId, Guid rosterId, RosterVector outerRosterVector, int rosterInstanceId)
        {
            return Event(new RosterInstancesRemoved() { Instances = new[] { new AddedRosterInstance() { GroupId = rosterId, OuterRosterVector = outerRosterVector, RosterInstanceId = rosterInstanceId }, } }, interviewId, null, null);
        }

        public Event QuestionsEnabled(Guid interviewId, Identity[] questions)
        {
            return Event(new QuestionsEnabled() { Questions = questions }, interviewId, null, null);
        }

        public Event QuestionsDisabled(Guid interviewId, Identity[] questions)
        {
            return Event(new QuestionsDisabled() { Questions = questions }, interviewId, null, null);
        }

        public Event AnswersDeclaredInvalid(Guid interviewId, IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
            return Event(new AnswersDeclaredInvalid(failedValidationConditions, DateTimeOffset.UtcNow), interviewId, null, null);
        }

        public Event AnswersDeclaredValid(Guid interviewId, Identity[] questions)
        {
            return Event(new AnswersDeclaredValid() { Questions = questions }, interviewId, null, null);
        }

        public Event VariablesChanged(Guid interviewId, Identity variableId, object value)
        {
            return Event(new VariablesChanged
            {
                ChangedVariables = new[]
                {
                    new ChangedVariable(variableId, value)
                }
            }, interviewId);
        }

        public Event DateTimeQuestionAnswered(Guid interviewId, Identity identity, DateTimeOffset? originDate = null, DateTime? answer = null)
        {
            return Event(new DateTimeQuestionAnswered
            {
                Answer = answer ?? DateTime.Now,
                OriginDate = originDate,
                QuestionId = identity.Id,
                RosterVector = identity.RosterVector
            }, interviewId);
        }
    }

    public class EntityFactory
    {
        public InterviewReference InterviewReference()
        {
            return new InterviewReference();
        }

        public Categories Categories(Guid id, string name = null, CategoryItem[] values = null)
        {
            return new Categories()
            {
                Id = id,
                Name = name ?? string.Empty,
                Values = values ?? new CategoryItem[0]
            };
        }

        public CategoryItem CategoryItem(int value, string title, int? parentId = null)
        {
            return new CategoryItem()
            {
                Id = value,
                Text = title,
                ParentId = parentId
            };
        }

        public DataExportProcessArgs DataExportProcessArgs(string tenant = "testTenant")
        {
            return new DataExportProcessArgs(
                new ExportSettings
                (
                    exportFormat: DataExportFormat.Tabular,
                    questionnaireId: new QuestionnaireId(Guid.Empty.FormatGuid() + "$" + 1),
                    tenant: new TenantInfo("", "", tenant)
                ));
        }
    }

    public static class EventExtensions
    {
        public static PublishedEvent<T> ToPublishedEvent<T>(this Event @event) where T :class, IEvent
        {
            return (PublishedEvent<T>)@event.AsPublishedEvent();
        }
    }
}
