using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WB.Services.Export.CsvExport;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.CsvExport.Implementation;
using WB.Services.Export.CsvExport.Implementation.DoFiles;
using WB.Services.Export.DescriptionGenerator;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Tests.CsvExport.Exporters;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.Tenant;

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
            long? interviewDuration = null)
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
                InterviewDuration = interviewDuration
            };

        public static DiagnosticsExporter DiagnosticsExporter(ICsvWriter csvWriter = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITenantApi<IHeadquartersApi> headquartersApi = null)
        {
            return new DiagnosticsExporter(Mock.Of<IOptions<InterviewDataExportSettings>>(x => x.Value == new InterviewDataExportSettings()),
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

        public static QuestionnaireExportStructure QuestionnaireExportStructure(string questionnaireId = null)
        {
            return new QuestionnaireExportStructure
            {
                QuestionnaireId = questionnaireId
            };
        }

        public static HeaderStructureForLevel HeaderStructureForLevel()
        {
            return new HeaderStructureForLevel { LevelScopeVector = new ValueVector<Guid>() };
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, string variableName = null, params IQuestionnaireEntity[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children,
                VariableName = variableName
            };
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

            var QuestionnaireExportStructureFactory = new QuestionnaireExportStructureFactory(
                new MemoryCache(new MemoryCacheOptions()), 
                Mock.Of<IQuestionnaireStorage>());
            return QuestionnaireExportStructureFactory.GetQuestionnaireExportStructure(Create.Tenant(), questionnaire);
        }

        public static InterviewSummary InterviewSummary( InterviewExportedAction status = InterviewExportedAction.ApprovedBySupervisor,
            string originatorName = "inter",
            UserRoles originatorRole = UserRoles.Interviewer,
            Guid? interviewId = null,
            DateTime? timestamp = null,
            string key = null)
            => new InterviewSummary
            {
                Status = status,
                InterviewId = interviewId ?? Guid.NewGuid(),
                Timestamp = timestamp ?? DateTime.Now,
                Key = key,
                StatusChangeOriginatorRole = originatorRole,
                StatusChangeOriginatorName = originatorName,
                InterviewerName = "inter",
                SupervisorName = "supervisor",
            };

        public static InterviewActionsExporter InterviewActionsExporter(ITenantApi<IHeadquartersApi> tenantApi,
            ICsvWriter csvWriter = null)
        {
            return new InterviewActionsExporter(InterviewDataExportSettings(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                tenantApi ?? HeadquartersApi());
        }

        public static IOptions<InterviewDataExportSettings> InterviewDataExportSettings()
        {
            return Mock.Of<IOptions<InterviewDataExportSettings>>(x => x.Value == new InterviewDataExportSettings());
        }

        public static TabularFormatExportService ReadSideToTabularFormatExportService(QuestionnaireExportStructure questionnaireExportStructure,
            ITenantApi<IHeadquartersApi> tenantApi,
            IFileSystemAccessor fileSystemAccessor = null,
            ICsvWriter csvWriter = null)
        {
            return new TabularFormatExportService(Mock.Of<ILogger<TabularFormatExportService>>(),
                tenantApi,
                Mock.Of<IInterviewsExporter>(),
                Mock.Of<ICommentsExporter>(),
                Mock.Of<IDiagnosticsExporter>(),
                Mock.Of<IInterviewActionsExporter>(),
                Mock.Of<IQuestionnaireExportStructureFactory>(x => x.GetQuestionnaireExportStructureAsync(It.IsAny<TenantInfo>(), It.IsAny<QuestionnaireId>()) == Task.FromResult(questionnaireExportStructure)),
                Mock.Of<IQuestionnaireStorage>(),
                
                Mock.Of<IProductVersion>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());
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
            bool areAnswersOrdered = false)
            => new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                VariableName = variable,
                YesNoView = yesNoView,
                AreAnswersOrdered = areAnswersOrdered
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
                Mock.Of<IOptions<Interview.InterviewDataExportSettings>>(s => s.Value == new InterviewDataExportSettings()),
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
            InterviewTextListAnswer[] asList = null)
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
                AsYesNo = asYesNo

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

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(
            params IQuestionnaireEntity[] children)
        {
            return QuestionnaireDocumentWithOneChapter(chapterId: null, children: children);
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, 
            Guid? id = null,
            string variable =null,
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
                QuestionType = QuestionType.Text,
                VariableName = variable ?? "vv" + Guid.NewGuid().ToString("N"),
                Featured = preFilled,
                VariableLabel = label,
                ValidationConditions = validationConditions?.ToList(),
                MaxAnswerCount = maxAnswersCount
               
            };

        public static IInterviewFactory InterviewFactory()
        {
            return new InterviewFactory(Create.HeadquartersApi());
        }

        public static Variable Variable(Guid? id = null, VariableType type = VariableType.LongInteger)
            => new Variable()
            {
                PublicKey = id ?? Guid.NewGuid(),
                Type = type
            };

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled = false, string title = null,
            string label=null)
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

        public static Group Group(Guid? groupId = null, params IQuestionnaireEntity[] children)
        {
            return new Group(children: children.ToList())
            {
                PublicKey = groupId ?? Guid.NewGuid()
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

        public static SingleQuestion SingleOptionQuestion(Guid? id = null, string questionText = null)
        {
            return new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                QuestionText = questionText,
            };
        }

        public static QuestionnaireLevelLabels QuestionnaireLevelLabels(string levelName = "level", params DataExportVariable[] variableLabels)
            => new QuestionnaireLevelLabels(levelName, variableLabels);

        public static DataExportVariable LabeledVariable(string variableName = "var", string label = "lbl", Guid? questionId = null, params VariableValueLabel[] variableValueLabels)
            => new DataExportVariable(variableName, label, questionId, variableValueLabels, ExportValueType.Unknown);

        public static VariableValueLabel VariableValueLabel(string value = "1", string label = "l1")
            => new VariableValueLabel(value, label);
    }
}
