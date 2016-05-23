extern alias designer;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;

namespace WB.Tests.Unit.TestFactories
{
    internal class EventFactory
    {
            public ImportFromDesigner ImportFromDesigner(Guid responsibleId, QuestionnaireDocument questionnaire, bool allowCensus, string assembly, long contentVersion)
            {
                return new ImportFromDesigner(responsibleId, questionnaire, allowCensus, assembly, contentVersion);
            }

            public NewGroupAdded AddGroup(Guid groupId, Guid? parentId = null, string variableName = null)
            {
                return new NewGroupAdded
                {
                    PublicKey = groupId,
                    ParentGroupPublicKey = parentId,
                    VariableName = variableName
                };
            }

            public NewQuestionAdded AddTextQuestion(Guid questionId, Guid parentId)
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

            public NewQuestionAdded NumericQuestionAdded(Guid publicKey, Guid groupPublicKey,
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

            public AnswersDeclaredInvalid AnswersDeclaredInvalid(Guid? id = null, decimal[] rosterVector = null)
                => Create.Event.AnswersDeclaredInvalid(questions: new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]),
                });

            public AnswersDeclaredInvalid AnswersDeclaredInvalid(Identity[] questions)
                => new AnswersDeclaredInvalid(questions);

            public AnswersDeclaredInvalid AnswersDeclaredInvalid(IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedConditions)
                => new AnswersDeclaredInvalid(failedConditions);

            public AnswersDeclaredValid AnswersDeclaredValid(Identity[] questions)
                => new AnswersDeclaredValid(questions);

            public AnswersDeclaredValid AnswersDeclaredValid()
                => new AnswersDeclaredValid(new Identity[]{});

            public AnswersRemoved AnswersRemoved(params Identity[] questions)
                => new AnswersRemoved(questions);

            public AnswerRemoved AnswerRemoved(Identity question)
                => new AnswerRemoved(Guid.NewGuid(), question.Id, question.RosterVector, DateTime.Now);

            public ExpressionsMigratedToCSharp ExpressionsMigratedToCSharpEvent()
                => new ExpressionsMigratedToCSharp();

            public GeoLocationQuestionAnswered GeoLocationQuestionAnswered(Identity question, double latitude, double longitude)
            {
                return new GeoLocationQuestionAnswered(
                    Guid.NewGuid(), question.Id, question.RosterVector, DateTime.UtcNow, latitude, longitude, 1, 1, DateTimeOffset.Now);
            }

            public GroupBecameARoster GroupBecameRoster(Guid rosterId)
            {
                return new GroupBecameARoster(Guid.NewGuid(), rosterId);
            }

            public GroupsDisabled GroupsDisabled(Identity[] groups) => new GroupsDisabled(groups);

            public GroupsDisabled GroupsDisabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new GroupsDisabled(identities);
            }

            public GroupsEnabled GroupsEnabled(Identity[] groups) => new GroupsEnabled(groups);

            public GroupsEnabled GroupsEnabled(Guid? id = null, decimal[] rosterVector = null)
            {
                var identities = new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
                };
                return new GroupsEnabled(identities);
            }

            public InterviewCreated InterviewCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
                => new InterviewCreated(
                    userId: Guid.NewGuid(),
                    questionnaireId: questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion: questionnaireVersion ?? 7);

            public InterviewDeleted InterviewDeleted()
                => new InterviewDeleted(userId: Guid.NewGuid());

            public IPublishedEvent<InterviewerAssigned> InterviewerAssigned(Guid interviewId, Guid userId, Guid interviewerId)
            {
                return new InterviewerAssigned(userId, interviewerId, DateTime.Now)
                        .ToPublishedEvent(eventSourceId: interviewId);
            }

            public InterviewCompleted InteviewCompleted()
                => new InterviewCompleted(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    "comment");

            public InterviewFromPreloadedDataCreated InterviewFromPreloadedDataCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
                => new InterviewFromPreloadedDataCreated(
                    Guid.NewGuid(),
                    questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion ?? 1);

            public InterviewHardDeleted InterviewHardDeleted()
                => new InterviewHardDeleted(userId: Guid.NewGuid());

            public InterviewOnClientCreated InterviewOnClientCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            {
                return new InterviewOnClientCreated(
                    Guid.NewGuid(),
                    questionnaireId ?? Guid.NewGuid(),
                    questionnaireVersion ?? 1);
            }

            public InterviewReceivedByInterviewer InterviewReceivedByInterviewer()
                => new InterviewReceivedByInterviewer();

            public InterviewReceivedBySupervisor InterviewReceivedBySupervisor()
                => new InterviewReceivedBySupervisor();

            public InterviewStatusChanged InterviewStatusChanged(InterviewStatus status, string comment = "hello")
                => new InterviewStatusChanged(status, comment);

            public VariablesDisabled VariablesDisabled(params Identity[] identites)
                => new VariablesDisabled(identites);

            public VariablesEnabled VariablesEnabled(params Identity[] identites)
             => new VariablesEnabled(identites);

            public VariablesChanged VariablesChanged(params ChangedVariable[] changedVariables)
             => new VariablesChanged(changedVariables);

            public InterviewSynchronized InterviewSynchronized(InterviewSynchronizationDto synchronizationDto)
            {
                return new InterviewSynchronized(synchronizationDto);
            }

            public IPublishedEvent<LookupTableAdded> LookupTableAdded(Guid questionnaireId, Guid entityId)
            {
                return new LookupTableAdded
                {
                    LookupTableId = entityId
                }.ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public IPublishedEvent<LookupTableDeleted> LookupTableDeleted(Guid questionnaireId, Guid entityId)
            {
                return new LookupTableDeleted
                {
                    LookupTableId = entityId
                }.ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public IPublishedEvent<LookupTableUpdated> LookupTableUpdated(Guid questionnaireId, Guid entityId, string name, string fileName)
            {
                return new LookupTableUpdated
                {
                    LookupTableId = entityId,
                    LookupTableName = name,
                    LookupTableFileName = fileName
                }.ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public IPublishedEvent<MacroAdded> MacroAdded(Guid questionnaireId, Guid entityId, Guid? responsibleId = null)
            {
                return new MacroAdded(entityId, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public IPublishedEvent<MacroDeleted> MacroDeleted(Guid questionnaireId, Guid entityId, Guid? responsibleId = null)
            {
                return new MacroDeleted(entityId, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public IPublishedEvent<MacroUpdated> MacroUpdated(Guid questionnaireId, Guid entityId, 
                string name, string content, string description,
                Guid? responsibleId = null)
            {
                return new MacroUpdated(entityId, name, content, description, responsibleId ?? Guid.NewGuid())
                    .ToPublishedEvent(eventSourceId: questionnaireId);
            }

            public MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(Guid? questionId = null,
                decimal[] rosterVector = null,
                decimal[][] selectedRosterVectors = null)
            {
                return new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), 
                    questionId ?? Guid.NewGuid(),
                    rosterVector ?? new decimal[]{},
                    DateTime.Now, 
                    selectedRosterVectors ?? new decimal[][]{});
            }

            public NewQuestionAdded NewQuestionAdded(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string variableLabel = null, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            string mask = null, int? maxAllowedAnswers = null, bool? yesNoView = null, bool? areAnswersOrdered = null, bool hideIfDisabled = false,
            QuestionProperties properties = null)
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
                    properties: properties ?? new QuestionProperties(false, false),
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

            public IPublishedEvent<NewUserCreated> NewUserCreated(Guid userId, 
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

            public NumericQuestionChanged NumericQuestionChanged(
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
                bool hideIfDisabled = false,
            QuestionProperties properties = null)
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
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId ?? Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    validationConditions: new List<ValidationCondition>());
            }

            public NumericQuestionCloned NumericQuestionCloned(Guid publicKey,
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
                bool hideIfDisabled = false,
            QuestionProperties properties = null)
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
                    properties: properties ?? new QuestionProperties(false, false),
                    responsibleId: responsibleId.HasValue ? responsibleId.Value : Guid.NewGuid(),
                    capital: false,
                    isInteger: isInteger,
                    countOfDecimalPlaces: null,
                    sourceQuestionnaireId: null,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    validationConditions: validationConditions ?? new List<ValidationCondition>());
            }

            public QuestionChanged QuestionChanged(Guid questionId, string variableName, QuestionType questionType)
            {
                return QuestionChanged(
                    publicKey : questionId,
                    stataExportCaption : variableName,
                    questionType : questionType
                );
            }

            public QuestionChanged QuestionChanged(Guid publicKey, Guid targetGroupKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                bool hideIfDisabled = false,
            QuestionProperties properties = null)
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
                    properties: properties ?? new QuestionProperties(false, false),
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

            public QuestionChanged QuestionChanged(Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            QuestionProperties properties = null)
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
                    properties: properties ?? new QuestionProperties(false, false),
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


            public QuestionCloned QuestionCloned(Guid publicKey, Guid sourceQuestionId, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
                string stataExportCaption = null, Guid? linkedToQuestionId = null, string variableLabel = null, bool capital = false, string validationExpression = null, string validationMessage = null,
                QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
                QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
                Guid? sourceQuestionnaireId = null, int targetIndex = 0, int? maxAnswerCount = null, int? countOfDecimalPlaces = null,
                IList<ValidationCondition> validationConditions = null,
            QuestionProperties properties = null)
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
                    properties: properties ?? new QuestionProperties(false, false),
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

            public IPublishedEvent<QuestionnaireDeleted> QuestionnaireDeleted(Guid? questionnaireId = null, long? version = null)
            {
                var questionnaireDeleted = new QuestionnaireDeleted
                {

                    QuestionnaireVersion = version ?? 1
                }.ToPublishedEvent(questionnaireId ?? Guid.NewGuid());
                return questionnaireDeleted;
            }

            public QuestionsDisabled QuestionsDisabled(Identity[] questions) => new QuestionsDisabled(questions);

            public QuestionsDisabled QuestionsDisabled(Guid? id = null, decimal[] rosterVector = null)
                => Create.Event.QuestionsDisabled(new[]
                {
                    Create.Other.Identity(id ?? Guid.NewGuid(), rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty),
                });

            public QuestionsEnabled QuestionsEnabled(Identity[] questions) => new QuestionsEnabled(questions);

            public QuestionsEnabled QuestionsEnabled(Guid? id = null, decimal[] rosterVector = null)
                => Create.Event.QuestionsEnabled(new[]
                {
                    new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]),
                });

            public RosterChanged RosterChanged(Guid rosterId, RosterSizeSourceType rosterType, FixedRosterTitle[] titles)
            {
                return new RosterChanged(Guid.NewGuid(), rosterId)
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = rosterType,
                    FixedRosterTitles = titles,
                    RosterTitleQuestionId = null
                };
            }

            public RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params decimal[][] fullRosterVectors)
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

            public RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params RosterVector[] fullRosterVectors)
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

            public RosterInstancesAdded RosterInstancesAdded(Guid? rosterGroupId = null,
                decimal[] rosterVector = null,
                decimal? rosterInstanceId = null,
                int? sortIndex = null)
            {
                return new RosterInstancesAdded(new[]
                {
                    new AddedRosterInstance(rosterGroupId ?? Guid.NewGuid(), rosterVector ?? new decimal[0], rosterInstanceId ?? 0.0m, sortIndex)
                });
            }

            public RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
            {
                return new RosterInstancesRemoved(new[]
                {
                    new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m)
                });
            }

            public RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null, 
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

            public SingleOptionQuestionAnswered SingleOptionQuestionAnswered(Guid questionId, decimal[] rosterVector, decimal answer, Guid? userId = null)
            {
                return new SingleOptionQuestionAnswered(userId ?? Guid.NewGuid(), questionId, rosterVector, DateTime.UtcNow, answer);
            }

            public StaticTextAdded StaticTextAdded(Guid? parentId = null, string text = null, Guid? responsibleId = null, Guid? publicKey = null)
            {
                return new StaticTextAdded(
                    publicKey.GetValueOrDefault(Guid.NewGuid()),
                    responsibleId ?? Guid.NewGuid(),
                    parentId ?? Guid.NewGuid(),
                    text,
                    null,
                    false,
                    null);
            }

            public StaticTextUpdated StaticTextUpdated(Guid? parentId = null, string text = null, string attachment = null, 
                Guid? responsibleId = null, Guid? publicKey = null, string enablementCondition = null, bool hideIfDisabled = false, 
                IList<ValidationCondition> validationConditions = null)
            {
                return new StaticTextUpdated(
                    publicKey.GetValueOrDefault(Guid.NewGuid()),
                    responsibleId ?? Guid.NewGuid(),
                    text,
                    attachment,
                    hideIfDisabled,
                    enablementCondition,
                    validationConditions);
            }


            public StaticTextCloned StaticTextCloned(Guid? parentId = null, string text = null, string attachment = null,
                Guid? responsibleId = null, Guid? publicKey = null, Guid? sourceQuestionnaireId = null, Guid? sourceEntityId = null,
                string enablementCondition = null, bool hideIfDisabled = false, IList<ValidationCondition> validationConditions = null, int targetIndex = 0)
            {
                return new StaticTextCloned(
                    publicKey.GetValueOrDefault(Guid.NewGuid()),
                    responsibleId ?? Guid.NewGuid(),
                    parentId ?? Guid.NewGuid(),
                    sourceQuestionnaireId,
                    sourceEntityId ?? Guid.NewGuid(),
                    targetIndex,
                    text,
                    attachment,
                    enablementCondition,
                    hideIfDisabled,
                    validationConditions);
            }

            public TextQuestionAnswered TextQuestionAnswered(
                Guid? questionId = null, decimal[] rosterVector = null, string answer = null)
                => new TextQuestionAnswered(
                    Guid.NewGuid(),
                    questionId ?? Guid.NewGuid(),
                    rosterVector ?? WB.Core.SharedKernels.DataCollection.RosterVector.Empty,
                    DateTime.Now,
                    answer ?? "answer");

            public NumericQuestionChanged UpdateNumericIntegerQuestion(Guid questionId, string variableName, string enablementCondition = null, string validationExpression = null)
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

            public IPublishedEvent<UserChanged> UserChanged(Guid userId, string password, string email, Guid? eventId = null)
            {
                return new UserChanged
                {
                    PasswordHash = password,
                    Email = email
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public IPublishedEvent<UserLinkedToDevice> UserLinkedToDevice(Guid userId, string deviceId, DateTime eventTimeStamp)
            {
                return new UserLinkedToDevice
                {
                    DeviceId = deviceId
                }.ToPublishedEvent(eventSourceId: userId, eventTimeStamp: eventTimeStamp);
            }

            public IPublishedEvent<UserLocked> UserLocked(Guid userId, Guid? eventId = null)
            {
                return new UserLocked
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public IPublishedEvent<UserLockedBySupervisor> UserLockedBySupervisor(Guid userId, Guid? eventId = null)
            {
                return new UserLockedBySupervisor
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public IPublishedEvent<UserUnlocked> UserUnlocked(Guid userId, Guid? eventId = null)
            {
                return new UserUnlocked
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public IPublishedEvent<UserUnlockedBySupervisor> UserUnlockedBySupervisor(Guid userId, Guid? eventId = null)
            {
                return new UserUnlockedBySupervisor
                {
                }.ToPublishedEvent(eventSourceId: userId, eventId: eventId);
            }

            public YesNoQuestionAnswered YesNoQuestionAnswered(Guid? questionId = null, AnsweredYesNoOption[] answeredOptions = null)
            {
                return new YesNoQuestionAnswered(
                    userId: Guid.NewGuid(),
                    questionId: questionId ?? Guid.NewGuid(),
                    rosterVector: Core.SharedKernels.DataCollection.RosterVector.Empty,
                    answerTimeUtc: DateTime.UtcNow,
                    answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});
            }

            public LinkedOptionsChanged LinkedOptionsChanged(ChangedLinkedOptions[] options = null)
            {
                return new LinkedOptionsChanged(options ?? new ChangedLinkedOptions[] {});
            }

            public StaticTextsEnabled StaticTextsEnabled(params Identity[] ids)
            {
                return new StaticTextsEnabled(ids);
            }

            public StaticTextsDisabled StaticTextsDisabled(params Identity[] ids)
            {
                return new StaticTextsDisabled(ids);
            }

            public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(params Identity[] staticTextIdentity)
            {
                List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedConditions = staticTextIdentity.Select(x => 
                    new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(
                        x, new ReadOnlyCollection<FailedValidationCondition>(new List<FailedValidationCondition> {new FailedValidationCondition(0)}))).ToList();
                return new StaticTextsDeclaredInvalid(failedConditions);
            }

            public StaticTextsDeclaredValid StaticTextsDeclaredValid(params Identity[] staticTextIdentity)
            {
                return new StaticTextsDeclaredValid(staticTextIdentity);
            }

            public ChangedVariable ChangedVariable(Identity variableIdentity, object value)
            {
                return new ChangedVariable(variableIdentity, value);
            }

            public SubstitutionTitlesChanged SubstitutionTitlesChanged(
                Identity[] questions = null, Identity[] staticTexts = null)
                => new SubstitutionTitlesChanged(
                    questions ?? new Identity[] {},
                    staticTexts ?? new Identity[] {});
        }
}