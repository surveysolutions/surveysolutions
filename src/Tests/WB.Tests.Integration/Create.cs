using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.CommandBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Services.Sql;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Integration
{
    internal static class Create
    {
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
                Children = children != null ? children.ToList() : new List<IComposite>(),
            };
        }

        public static Group Chapter(string title = "Chapter X", IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                children: children);
        }

        public static IQuestion Question(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = id ?? Guid.NewGuid(),
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null, bool isMandatory = false,
            IEnumerable<Answer> answers = null, Guid? linkedToQuestionId = null, string variable = null)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Mandatory = isMandatory,
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(answers ?? new Answer[] {}),
                LinkedToQuestionId = linkedToQuestionId,
                StataExportCaption = variable
            };
        }

        public static TextListQuestion ListQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new TextListQuestion
            {
                QuestionType = QuestionType.TextList,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static TextQuestion TextQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new TextQuestion
            {
                QuestionType = QuestionType.Text,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, bool isMandatory = false,
            Guid? cascadeFromQuestionId = null, List<Answer> options = null)
        {
            return new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Mandatory = isMandatory,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId
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

        public static Group Roster(Guid? id = null, string title = "Roster X", string variable = null, string enablementCondition = null,
            string[] fixedTitles = null, IEnumerable<IComposite> children = null, RosterSizeSourceType rosterSizeSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null, Guid? rosterTitleQuestionId = null)
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
                group.RosterFixedTitles = fixedTitles ?? new[] {"Roster X-1", "Roster X-2", "Roster X-3"};
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
            return new Questionnaire(Guid.NewGuid(), questionnaireDocument, false, "base64 string of assembly");
        }

        public static Interview Interview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, object> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null)
        {
            return new Interview(
                interviewId ?? new Guid("A0A0A0A0B0B0B0B0A0A0A0A0B0B0B0B0"),
                userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"), 1,
                answersToFeaturedQuestions ?? new Dictionary<Guid, object>(),
                answersTime ?? new DateTime(2012, 12, 20),
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"));
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

        public static CommandService CommandService(IAggregateRootRepository repository = null, IEventBus eventBus = null, IAggregateSnapshotter snapshooter = null)
        {
            return new CommandService(
                repository ?? Mock.Of<IAggregateRootRepository>(),
                eventBus ?? Mock.Of<IEventBus>(),
                snapshooter ?? Mock.Of<IAggregateSnapshotter>());
        }
    }
}
