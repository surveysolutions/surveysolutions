using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NHibernate.Transform;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Integration
{
    internal static class Create
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
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICompilerSettings>());
        }

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

        public class Event
        {
            private static class Default
            {
                public static readonly Guid UserId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAABBB");
                public static readonly DateTime AnswerTime = new DateTime(2014, 1, 1);
            }


            public static SingleOptionQuestionAnswered SingleOptionQuestionAnswered(
                Guid questionId, decimal answer, decimal[] propagationVector = null, Guid? userId = null, DateTime? answerTime = null)
            {
                return new SingleOptionQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    answer);
            }

            public static NumericIntegerQuestionAnswered NumericIntegerQuestionAnswered(
                Guid questionId, int answer, decimal[] propagationVector = null, Guid? userId = null, DateTime? answerTime = null)
            {
                return new NumericIntegerQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    answer);
            }

            public static NumericRealQuestionAnswered NumericRealQuestionAnswered(
                Guid questionId, decimal answer, decimal[] propagationVector = null, Guid? userId = null, DateTime? answerTime = null)
            {
                return new NumericRealQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    answer);
            }

            public static TextQuestionAnswered TextQuestionAnswered(
                Guid questionId, string answer, decimal[] propagationVector = null, Guid? userId = null, DateTime? answerTime = null)
            {
                return new TextQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    answer);
            }

            public static DateTimeQuestionAnswered DateTimeQuestionAnswered(
                Guid questionId, DateTime answer, decimal[] propagationVector = null, Guid? userId = null, DateTime? answerTime = null)
            {
                return new DateTimeQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    answer);
            }

            public static MultipleOptionsQuestionAnswered MultipleOptionsQuestionAnswered(
                Guid questionId, Guid? userId = null, decimal[] propagationVector = null, DateTime? answerTime = null, decimal[] selectedValues = null)
            {
                return new MultipleOptionsQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    selectedValues);
            }

            public static MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(
                Guid questionId, Guid? userId = null, decimal[] propagationVector = null, DateTime? answerTime = null, decimal[][] selectedValues = null)
            {
                return new MultipleOptionsLinkedQuestionAnswered(
                    userId ?? Default.UserId,
                    questionId,
                    propagationVector ?? Empty.RosterVector,
                    answerTime ?? Default.AnswerTime,
                    selectedValues);
            }

            public static AnswersDeclaredValid AnswersDeclaredValid(params Identity[] questions)
            {
                return new AnswersDeclaredValid(questions);
            }

            public static AnswersDeclaredInvalid AnswersDeclaredInvalid(params Identity[] questions)
            {
                return new AnswersDeclaredInvalid(questions);
            }

            public static QuestionsEnabled QuestionsEnabled(params Identity[] questions)
            {
                return new QuestionsEnabled(questions);
            }

            public static QuestionsDisabled QuestionsDisabled(params Identity[] questions)
            {
                return new QuestionsDisabled(questions);
            }

            public static GroupsEnabled GroupsEnabled(params Identity[] groups)
            {
                return new GroupsEnabled(groups);
            }

            public static GroupsDisabled GroupsDisabled(params Identity[] groups)
            {
                return new GroupsDisabled(groups);
            }

            public static RosterInstancesAdded RosterInstancesAdded(params AddedRosterInstance[] rosterInstances)
            {
                return new RosterInstancesAdded(rosterInstances);
            }

            public static RosterInstancesAdded RosterInstancesAdded(Guid? rosterGroupId = null,
                decimal[] rosterVector = null,
                decimal? rosterInstanceId = null,
                int? sortIndex = null)
            {
                return new RosterInstancesAdded(new[]
                {
                    new AddedRosterInstance(rosterGroupId ?? Guid.NewGuid(), rosterVector ?? new decimal[0], rosterInstanceId ?? 0.0m, sortIndex)
                });
            }

            public static RosterInstancesRemoved RosterInstancesRemoved(params RosterInstance[] rosterInstances)
            {
                return new RosterInstancesRemoved(rosterInstances);
            }
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToList() ?? new List<IComposite>(),
            };
        }

        public static Group Chapter(string title = "Chapter X", IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                children: children);
        }

        public static IQuestion Question(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = id ?? Guid.NewGuid(),
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, 
            IEnumerable<Answer> answers = null, Guid? linkedToQuestionId = null, string variable = null, Guid? linkedToRosterId=null)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(answers ?? new Answer[] {}),
                LinkedToQuestionId = linkedToQuestionId,
                StataExportCaption = variable,
                LinkedToRosterId = linkedToRosterId
            };
        }

        public static TextListQuestion ListQuestion(Guid? id = null, string variable = null, string enablementCondition = null, 
            string validationExpression = null)
        {
            return new TextListQuestion
            {
                QuestionType = QuestionType.TextList,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static TextQuestion TextQuestion(Guid? id = null, string variable = null, string enablementCondition = null, 
            string validationExpression = null)
        {
            return new TextQuestion
            {
                QuestionType = QuestionType.Text,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = null, string enablementCondition = null, 
            string validationExpression = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid id, string variable, IList<ValidationCondition> validationExpression)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id,
                StataExportCaption = variable,
                IsInteger = true,
                ValidationConditions = validationExpression?? new List<ValidationCondition>()
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, 
            string validationExpression = null, Guid? cascadeFromQuestionId = null, List<Answer> options = null, Guid? linkedToQuestionId = null, Guid? linkedToRosterId=null)
        {
            return new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId
            };
        }

        public static Answer Option(Guid? id = null, string text = null, string value = null, string parentValue = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        public static NumericQuestion NumericRealQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static DateTimeQuestion DateTimeQuestion(Guid id, string variable, string enablementCondition = null, string validationExpression = null)
        {
            return new DateTimeQuestion
            {
                PublicKey = id,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new GpsCoordinateQuestion
            {
                QuestionType = QuestionType.GpsCoordinates,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static Group Roster(Guid? id = null, 
            string title = "Roster X",
            string variable = null, 
            string enablementCondition = null,
            string[] fixedTitles = null, 
            IEnumerable<IComposite> children = null, 
            RosterSizeSourceType rosterSizeSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null,
            Guid? rosterTitleQuestionId = null,
            FixedRosterTitle[] fixedRosterTitles = null)
        {
            Group group = Create.Group(
                id: id,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = rosterSizeSourceType;
            if (rosterSizeSourceType == RosterSizeSourceType.FixedTitles)
            {
                if (fixedRosterTitles == null)
                {
                    group.RosterFixedTitles = fixedTitles ?? new[] { "Roster X-1", "Roster X-2", "Roster X-3" };
                }
                else
                {
                    group.FixedRosterTitles = fixedRosterTitles;
                }
            }
            group.RosterSizeQuestionId = rosterSizeQuestionId;
            group.RosterTitleQuestionId = rosterTitleQuestionId;

            return group;
        }

        public static Group Group(Guid? id = null, string title = "Group X", string variable = null,
            string enablementCondition = null, IEnumerable<IComposite> children = null)
        {
            return new Group(title)
            {
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,              
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static Questionnaire Questionnaire(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaire = new Questionnaire(
                Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                new ReferenceInfoForLinkedQuestionsFactory(), 
                new QuestionnaireRosterStructureFactory(),
                Mock.Of<IExportViewFactory>());

            questionnaire.ImportFromDesigner(new ImportFromDesigner(Guid.NewGuid(), questionnaireDocument, false, "base64 string of assembly", 1));

            return questionnaire;
        }

        public static Interview Interview(Guid? questionnaireId = null,
            IPlainQuestionnaireRepository questionnaireRepository = null, IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                expressionProcessorStatePrototypeProvider ?? Mock.Of<IInterviewExpressionStatePrototypeProvider>());

            interview.CreateInterview(
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                1,
                new Guid("D222D222D222D222D222D222D222D222"),
                new Dictionary<Guid, object>(),
                new DateTime(2012, 12, 20),
                new Guid("F111F111F111F111F111F111F111F111"));

            return interview;
        }

        public static Identity Identity(Guid id, decimal[] rosterVector = null)
        {
            return new Identity(id, rosterVector ?? Empty.RosterVector);
        }

        public static AddedRosterInstance AddedRosterInstance(Guid groupId, decimal[] outerRosterVector = null,
            decimal rosterInstanceId = 0, int? sortIndex = null)
        {
            return new AddedRosterInstance(groupId, outerRosterVector ?? Empty.RosterVector, rosterInstanceId, sortIndex);
        }

        public static RosterInstance RosterInstance(Guid groupId, decimal[] outerRosterVector = null, decimal rosterInstanceId = 0)
        {
            return new RosterInstance(groupId, outerRosterVector ?? Empty.RosterVector, rosterInstanceId);
        }

        public static HeaderStructureForLevel HeaderStructureForLevel(string levelName = "table name", string[] referenceNames = null, ValueVector<Guid> levelScopeVector = null)
        {
            return new HeaderStructureForLevel()
            {
                LevelScopeVector = levelScopeVector ?? new ValueVector<Guid>(),
                LevelName = levelName,
                LevelIdColumnName = "Id",
                IsTextListScope = referenceNames != null,
                ReferencedNames = referenceNames,
                HeaderItems =
                    new Dictionary<Guid, ExportedHeaderItem>
                    {
                        { Guid.NewGuid(), ExportedHeaderItem() },
                        { Guid.NewGuid(), ExportedHeaderItem(QuestionType.Numeric, new[] { "a" }) }
                    }
            };
        }

        public static ExportedHeaderItem ExportedHeaderItem(QuestionType type = QuestionType.Text, string[] columnNames = null)
        {
            return new ExportedHeaderItem() { ColumnNames = columnNames ?? new[] { "1" }, QuestionType = type };
        }

        public static QuestionnaireExportStructure QuestionnaireExportStructure(params HeaderStructureForLevel[] levels)
        {
            var header = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
            if (levels != null && levels.Length > 0)
            {
                header = levels.ToDictionary((i) => i.LevelScopeVector, (i) => i);
            }
            return new QuestionnaireExportStructure() { HeaderToLevelMap = header };
        }

        public static CommandService CommandService(IAggregateRootRepository repository = null, 
            IEventBus eventBus = null, 
            IAggregateSnapshotter snapshooter = null,
            IServiceLocator serviceLocator = null)
        {
            return new CommandService(
                repository ?? Mock.Of<IAggregateRootRepository>(),
                eventBus ?? Mock.Of<IEventBus>(),
                snapshooter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator ?? Mock.Of<IServiceLocator>());
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
                    .Select(x => Create.CommittedEvent(payload: x.Payload,
                        eventSourceId: x.EventSourceId,
                        eventSequence: x.EventSequence)));
        }

        public static FileSystemIOAccessor FileSystemIOAccessor()
        {
            return new FileSystemIOAccessor();
        }

        public static SequentialCommandService SequentialCommandService(IAggregateRootRepository repository = null, ILiteEventBus eventBus = null, IAggregateSnapshotter snapshooter = null)
        {
            return new SequentialCommandService(
                repository ?? Mock.Of<IAggregateRootRepository>(),
                eventBus ?? Mock.Of<ILiteEventBus>(),
                snapshooter ?? Mock.Of<IAggregateSnapshotter>(), Mock.Of<IServiceLocator>());
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

        public static FixedRosterTitle FixedRosterTitle(decimal value, string title)
        {
            return new FixedRosterTitle(value, title);
        }

        public static LookupTable LookupTable(string tableName)
        {
            return new LookupTable
            {
                TableName = tableName
            };
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
            ISessionProvider sessionProvider = null, PostgreConnectionSettings postgreConnectionSettings = null, ISerializer serializer = null)
            where TEntity : class, IReadSideRepositoryEntity
        {
            return new PostgresReadSideKeyValueStorage<TEntity>(
                sessionProvider ?? Mock.Of<ISessionProvider>(),
                postgreConnectionSettings ?? new PostgreConnectionSettings(),
                Mock.Of<ILogger>(),
                serializer ?? new NewtonJsonSerializer(new JsonSerializerSettingsFactory()));
        }
    }
}
