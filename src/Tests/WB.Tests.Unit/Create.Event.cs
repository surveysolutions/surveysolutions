extern alias designer;

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Moq;
using MvvmCross.Plugins.Messenger;
using Ncqrs.Eventing.ServiceModel.Bus;
using NHibernate.Bytecode;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using TemplateImported = designer::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        internal static class Event
        {
            public static ImportFromDesigner ImportFromDesigner(Guid responsibleId, QuestionnaireDocument questionnaire, bool allowCensus, string assembly, long contentVersion)
            {
                return new ImportFromDesigner(responsibleId, questionnaire, allowCensus, assembly, contentVersion);
            }

            public static NewGroupAdded AddGroup(Guid groupId, Guid? parentId = null, string variableName = null)
            {
                return new NewGroupAdded
                {
                    PublicKey = groupId,
                    ParentGroupPublicKey = parentId,
                    VariableName = variableName
                };
            }

            public static NewQuestionAdded AddTextQuestion(Guid questionId, Guid parentId)
            {
                return NewQuestionAdded(
                    publicKey: questionId,
                    groupPublicKey: parentId,
                    questionText: null,
                    questionType: QuestionType.Text,
                    stataExportCaption: null,
                    variableLabel: null,
                    featured: false,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: null,
                    validationExpression: null,
                    validationMessage: null,
                    instructions: null,
                    responsibleId: Guid.NewGuid(),
                    linkedToQuestionId: null,
                    isFilteredCombobox: false,
                    cascadeFromQuestionId: null,
                    capital: false,
                    answerOrder: null,
                    answers: null,
                    isInteger: null);
            }

            public static NewQuestionAdded NumericQuestionAdded(Guid publicKey, Guid groupPublicKey,
             bool? isInteger = null,
             string stataExportCaption = null,
             string questionText = null,
             string variableLabel = null,
             bool featured = false,
             string conditionExpression = null,
             string validationExpression = null,
             string validationMessage = null,
             string instructions = null,
             Guid? responsibleId = null,
             int? countOfDecimalPlaces = null,
             QuestionScope? questionScope = null)
            {
                return Create.Event.NewQuestionAdded(publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope ?? QuestionScope.Interviewer,
                    conditionExpression: conditionExpression,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    questionType:QuestionType.Numeric);
            }

            public static AnswersDeclaredInvalid AnswersDeclaredInvalid(Guid? id = null, decimal[] rosterVector = null)
                => Create.Event.AnswersDeclaredInvalid(questions: new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]),
                });

            public static AnswersDeclaredInvalid AnswersDeclaredInvalid(Identity[] questions)
                => new AnswersDeclaredInvalid(questions);

            public static AnswersDeclaredInvalid AnswersDeclaredInvalid(IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedConditions)
                => new AnswersDeclaredInvalid(failedConditions);

            public static AnswersDeclaredValid AnswersDeclaredValid()
                => new AnswersDeclaredValid(new Identity[]{});

            public static AnswersRemoved AnswersRemoved(params Identity[] questions)
                => new AnswersRemoved(questions);

            public static AnswerRemoved AnswerRemoved(Identity question)
                => new AnswerRemoved(Guid.NewGuid(), question.Id, question.RosterVector, DateTime.Now);

            public static ExpressionsMigratedToCSharp ExpressionsMigratedToCSharpEvent()
                => new ExpressionsMigratedToCSharp();

            public static GeoLocationQuestionAnswered GeoLocationQuestionAnswered(Identity question, double latitude, double longitude)
            {
                return new GeoLocationQuestionAnswered(
                    Guid.NewGuid(), question.Id, question.RosterVector, DateTime.UtcNow, latitude, longitude, 1, 1, DateTimeOffset.Now);
            }

            public static GroupBecameARoster GroupBecameRoster(Guid rosterId)
            {
                return new GroupBecameARoster(Guid.NewGuid(), rosterId);
            }

            public static GroupsDisabled GroupsDisabled(Identity[] groups) => new GroupsDisabled(groups);

            public static GroupsDisabled GroupsDisabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new GroupsDisabled(identities);
            }

            public static GroupsEnabled GroupsEnabled(Identity[] groups) => new GroupsEnabled(groups);

            public static GroupsEnabled GroupsEnabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new GroupsEnabled(identities);
            }

            public static InterviewCreated InterviewCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
                => new InterviewCreated(
                    userId: Guid.NewGuid(),
                    questionnaireId: questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion: questionnaireVersion ?? 7);

            public static InterviewDeleted InterviewDeleted()
                => new InterviewDeleted(userId: Guid.NewGuid());

            public static IPublishedEvent<InterviewerAssigned> InterviewerAssigned(Guid interviewId, Guid userId, Guid interviewerId)
            {
                return new InterviewerAssigned(userId, interviewerId, DateTime.Now)
                        .ToPublishedEvent(eventSourceId: interviewId);
            }

            public static InterviewCompleted InteviewCompleted()
                => new InterviewCompleted(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    "comment");

            public static InterviewFromPreloadedDataCreated InterviewFromPreloadedDataCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
                => new InterviewFromPreloadedDataCreated(
                    Guid.NewGuid(),
                    questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion ?? 1);

            public static InterviewHardDeleted InterviewHardDeleted()
                => new InterviewHardDeleted(userId: Guid.NewGuid());

            public static InterviewOnClientCreated InterviewOnClientCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            {
                return new InterviewOnClientCreated(
                    Guid.NewGuid(),
                    questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion ?? 1);
            }

            public static InterviewReceivedByInterviewer InterviewReceivedByInterviewer()
                => new InterviewReceivedByInterviewer();

            public static InterviewReceivedBySupervisor InterviewReceivedBySupervisor()
                => new InterviewReceivedBySupervisor();

            public static InterviewStatusChanged InterviewStatusChanged(InterviewStatus status, string comment = "hello")
                => new InterviewStatusChanged(status, comment);

            public static InterviewSynchronized InterviewSynchronized(InterviewSynchronizationDto synchronizationDto)
            {
                return new InterviewSynchronized(synchronizationDto);
            }

            public static IPublishedEvent<LookupTableAdded> LookupTableAdded(Guid questionnaireId, Guid entityId)
            {
                return new LookupTableAdded
                {
                    LookupTableId = entityId
                }.ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<LookupTableDeleted> LookupTableDeleted(Guid questionnaireId, Guid entityId)
            {
                return new LookupTableDeleted
                {
                    LookupTableId = entityId
                }.ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<LookupTableUpdated> LookupTableUpdated(Guid questionnaireId, Guid entityId, string name, string fileName)
            {
                return new LookupTableUpdated
                {
                    LookupTableId = entityId,
                    LookupTableName = name,
                    LookupTableFileName = fileName
                }.ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<MacroAdded> MacroAdded(Guid questionnaireId, Guid entityId, Guid? responsibleId = null)
            {
                return new MacroAdded(entityId, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<MacroDeleted> MacroDeleted(Guid questionnaireId, Guid entityId, Guid? responsibleId = null)
            {
                return new MacroDeleted(entityId, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static IPublishedEvent<MacroUpdated> MacroUpdated(Guid questionnaireId, Guid entityId, 
                string name, string content, string description,
                Guid? responsibleId = null)
            {
                return new MacroUpdated(entityId, name, content, description, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public static MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(Guid? questionId = null,
                decimal[] rosterVector = null,
                decimal[][] selectedRosterVectors = null)
            {
                return new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), 
                    questionId ?? Guid.NewGuid(),
                    rosterVector ?? new decimal[]{},
                    DateTime.Now, 
                    selectedRosterVectors ?? new decimal[][]{});
            }

            public static NewQuestionAdded NewQuestionAdded(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string variableLabel = null, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            string mask = null, int? maxAllowedAnswers = null, bool? yesNoView = null, bool? areAnswersOrdered = null, bool hideIfDisabled = false)
            {
                return new NewQuestionAdded(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    yesNoView: yesNoView,
                    maxAllowedAnswers: maxAllowedAnswers,
                    mask: mask,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    validationConditions: new List<ValidationCondition>());
            }

            public static IPublishedEvent<NewUserCreated> NewUserCreated(Guid userId, 
                string name, 
                string password, 
                string email,
                bool islockedBySupervisor, 
                bool isLocked,
                Guid? eventId = null)
            {
                return new NewUserCreated
                {
                    Name = name,
                    Password = password,
                    Email = email,
                    IsLockedBySupervisor = islockedBySupervisor,
                    IsLocked = isLocked,
                    Roles = new[] { UserRoles.Operator }
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static NumericQuestionChanged NumericQuestionChanged(
                Guid publicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null,
                bool hideIfDisabled = false)
            {
                return new NumericQuestionChanged(
                    publicKey: publicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: QuestionScope.Interviewer,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    validationConditions: new List<ValidationCondition>());
            }

            public static NumericQuestionCloned NumericQuestionCloned(Guid publicKey,
                Guid sourceQuestionId,
                Guid groupPublicKey,
                bool? isInteger = null,
                string stataExportCaption = null,
                string questionText = null,
                string variableLabel = null,
                bool featured = false,
                string conditionExpression = null,
                string validationExpression = null,
                string validationMessage = null,
                string instructions = null,
                Guid? responsibleId = null,
                int targetIndex = 0,
                QuestionScope scope = QuestionScope.Interviewer,
                IList<ValidationCondition> validationConditions = null,
                bool hideIfDisabled = false)
            {
                return new NumericQuestionCloned(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: scope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    sourceQuestionnaireId: null,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    validationConditions: validationConditions ?? new List<ValidationCondition>());
            }

            public static QuestionChanged QuestionChanged(Guid questionId, string variableName, QuestionType questionType)
            {
                return QuestionChanged(
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    questionType : questionType
                );
            }

            public static QuestionChanged QuestionChanged(Guid publicKey, Guid targetGroupKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                bool hideIfDisabled = false)
            {
                return new QuestionChanged(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: null,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: hideIfDisabled,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    linkedToRosterId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    targetGroupKey: targetGroupKey,
                    validationConditions: new List<ValidationCondition>(),
                linkedFilterExpression: null);
            }

            public static QuestionChanged QuestionChanged(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null)
            {
                return new QuestionChanged(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: null,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: false,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null,
                    linkedToRosterId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    targetGroupKey: Guid.NewGuid(),
                    validationConditions: new List<ValidationCondition>(),
                linkedFilterExpression: null);
            }


            public static QuestionCloned QuestionCloned(Guid publicKey, Guid sourceQuestionId, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, string variableLabel = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                Guid? sourceQuestionnaireId = null, int targetIndex = 0, int? maxAnswerCount = null, int? countOfDecimalPlaces = null,
                IList<ValidationCondition> validationConditions = null)
            {
                return new QuestionCloned(
                    publicKey: publicKey,
                    groupPublicKey: groupPublicKey,
                    questionText: questionText,
                    stataExportCaption: stataExportCaption,
                    variableLabel: variableLabel,
                    featured: featured,
                    questionScope: questionScope,
                    conditionExpression: conditionExpression,
                    hideIfDisabled: false,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: capital,
                    isInteger: isInteger,
                    questionType: questionType,
                    answerOrder: answerOrder,
                    answers: answers,
                    linkedToQuestionId: null, 
                    linkedToRosterId: null,
                    areAnswersOrdered: null,
                    yesNoView: null,
                    maxAllowedAnswers: null,
                    mask: null,
                    isFilteredCombobox: isFilteredCombobox,
                    cascadeFromQuestionId: cascadeFromQuestionId,
                    sourceQuestionnaireId: sourceQuestionnaireId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    maxAnswerCount: maxAnswerCount,
                    countOfDecimalPlaces: countOfDecimalPlaces,
                    validationConditions: validationConditions ?? new List<ValidationCondition>(),
                linkedFilterExpression: null);
            }

            public static IPublishedEvent<QuestionnaireDeleted> QuestionnaireDeleted(Guid? questionnaireId = null, long? version = null)
            {
                var questionnaireDeleted = new QuestionnaireDeleted
                {

                    QuestionnaireVersion = version ?? 1
                }.ToPublishedEvent(questionnaireId ?? Guid.NewGuid());
                return questionnaireDeleted;
            }

            public static QuestionsDisabled QuestionsDisabled(Identity[] questions) => new QuestionsDisabled(questions);

            public static QuestionsDisabled QuestionsDisabled(Guid? id = null, decimal[] rosterVector = null)
                => Create.Event.QuestionsDisabled(new[]
                {
                    Create.Identity(id ?? Guid.NewGuid(), rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty),
                });

            public static QuestionsEnabled QuestionsEnabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new QuestionsEnabled(identities);
            }

            public static RosterChanged RosterChanged(Guid rosterId, RosterSizeSourceType rosterType, FixedRosterTitle[] titles)
            {
                return new RosterChanged(Guid.NewGuid(), rosterId)
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = rosterType,
                    FixedRosterTitles = titles,
                    RosterTitleQuestionId = null
                };
            }

            public static RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params decimal[][] fullRosterVectors)
            {
                AddedRosterInstance[] instances =
                    fullRosterVectors
                        .Select(fullRosterVector => new AddedRosterInstance(
                            rosterId,
                            outerRosterVector: fullRosterVector.Take(fullRosterVector.Length - 1).ToArray(),
                            rosterInstanceId: fullRosterVector.Last(),
                            sortIndex: null))
                        .ToArray();

                return new RosterInstancesAdded(instances);
            }

            public static RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params RosterVector[] fullRosterVectors)
            {
                AddedRosterInstance[] instances =
                    fullRosterVectors
                        .Select(fullRosterVector => new AddedRosterInstance(
                            rosterId,
                            outerRosterVector: fullRosterVector.Take(fullRosterVector.Length - 1).ToArray(),
                            rosterInstanceId: fullRosterVector.Last(),
                            sortIndex: null))
                        .ToArray();

                return new RosterInstancesAdded(instances);
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

            public static RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
            {
                return new RosterInstancesRemoved(new[]
                {
                    new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m)
                });
            }

            public static RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null, 
                decimal[] rosterVector = null,
                string rosterTitle = null)
            {
                return new RosterInstancesTitleChanged(
                    new[]
                {
                    new ChangedRosterInstanceTitleDto(
                        new RosterInstance(rosterId ?? Guid.NewGuid(), 
                            rosterVector != null ? rosterVector.WithoutLast().ToArray() : new decimal[0], 
                            rosterVector != null ? rosterVector.Last() : 0.0m), 
                        rosterTitle ?? "title")
                });
            }

            public static SingleOptionQuestionAnswered SingleOptionQuestionAnswered(Guid questionId, decimal[] rosterVector, decimal answer, Guid? userId = null)
            {
                return new SingleOptionQuestionAnswered(userId ?? Guid.NewGuid(), questionId, rosterVector, DateTime.UtcNow, answer);
            }

            public static StaticTextAdded StaticTextAdded(Guid? parentId = null, string text = null, Guid? responsibleId = null, Guid? publicKey = null)
            {
                return new StaticTextAdded
                {
                    EntityId = publicKey.GetValueOrDefault(Guid.NewGuid()),
                    ResponsibleId = responsibleId ?? Guid.NewGuid(),
                    ParentId =  parentId ?? Guid.NewGuid(),
                    Text = text
                };
            }

            public static TextQuestionAnswered TextQuestionAnswered(
                Guid? questionId = null, decimal[] rosterVector = null, string answer = null)
                => new TextQuestionAnswered(
                    Guid.NewGuid(),
                    questionId ?? Guid.NewGuid(),
                    rosterVector ?? WB.Core.SharedKernels.DataCollection.RosterVector.Empty,
                    DateTime.Now,
                    answer ?? "answer");

            public static NumericQuestionChanged UpdateNumericIntegerQuestion(Guid questionId, string variableName, string enablementCondition = null, string validationExpression = null)
            {
                return NumericQuestionChanged
                (
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    isInteger : true,
                    conditionExpression : enablementCondition,
                    validationExpression : validationExpression
                );
            }

            public static IPublishedEvent<UserChanged> UserChanged(Guid userId, string password, string email, Guid? eventId = null)
            {
                return new UserChanged
                {
                    PasswordHash = password,
                    Email = email
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserLinkedToDevice> UserLinkedToDevice(Guid userId, string deviceId, DateTime eventTimeStamp)
            {
                return new UserLinkedToDevice
                {
                    DeviceId = deviceId
                }.ToPublishedEvent(eventSourceId: userId, eventTimeStamp: eventTimeStamp);
            }

            public static IPublishedEvent<UserLocked> UserLocked(Guid userId, Guid? eventId = null)
            {
                return new UserLocked
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserLockedBySupervisor> UserLockedBySupervisor(Guid userId, Guid? eventId = null)
            {
                return new UserLockedBySupervisor
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserUnlocked> UserUnlocked(Guid userId, Guid? eventId = null)
            {
                return new UserUnlocked
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static IPublishedEvent<UserUnlockedBySupervisor> UserUnlockedBySupervisor(Guid userId, Guid? eventId = null)
            {
                return new UserUnlockedBySupervisor
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public static YesNoQuestionAnswered YesNoQuestionAnswered(Guid? questionId = null, AnsweredYesNoOption[] answeredOptions = null)
            {
                return new YesNoQuestionAnswered(
                    userId: Guid.NewGuid(),
                    questionId: questionId ?? Guid.NewGuid(),
                    rosterVector: Core.SharedKernels.DataCollection.RosterVector.Empty,
                    answerTimeUtc: DateTime.UtcNow,
                    answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});
            }

            public static class Published
            {
                public static IPublishedEvent<InterviewDeleted> InterviewDeleted(Guid? interviewId = null)
                    => Create.Event.InterviewDeleted().ToPublishedEvent(eventSourceId: interviewId);

                public static IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(Guid? interviewId = null)
                    => Create.Event.InterviewHardDeleted().ToPublishedEvent(eventSourceId: interviewId);


                public static IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
                    Guid interviewId, InterviewStatus status, string comment = "hello", Guid? eventId = null)
                    => Create.Event.InterviewStatusChanged(status, comment).ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);
            }

            internal static class Designer
            {
                public static TemplateImported TemplateImported(QuestionnaireDocument questionnaireDocument)
                {
                    return new TemplateImported { Source = questionnaireDocument };
                }
            }

            public static LinkedOptionsChanged LinkedOptionsChanged(ChangedLinkedOptions[] options = null)
            {
                return new LinkedOptionsChanged(options ?? new ChangedLinkedOptions[] {});
            }
        }
    }
}