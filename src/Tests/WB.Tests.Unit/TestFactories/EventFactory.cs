﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.Commands;

namespace WB.Tests.Unit.TestFactories
{
    internal class EventFactory
    {
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

        public ChangedVariable ChangedVariable(Identity variableIdentity, object value)
            => new ChangedVariable(variableIdentity, value);

        public GeoLocationQuestionAnswered GeoLocationQuestionAnswered(Identity question, double latitude, double longitude)
            => new GeoLocationQuestionAnswered(
                Guid.NewGuid(), question.Id, question.RosterVector, DateTime.UtcNow, latitude, longitude, 1, 1, DateTimeOffset.Now);

        public GroupsDisabled GroupsDisabled(Identity[] groups)
            => new GroupsDisabled(groups);

        public GroupsDisabled GroupsDisabled(Guid? id = null, decimal[] rosterVector = null)
            => new GroupsDisabled(new[]
            {
                new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
            });

        public GroupsEnabled GroupsEnabled(Identity[] groups)
            => new GroupsEnabled(groups);

        public GroupsEnabled GroupsEnabled(Guid? id = null, decimal[] rosterVector = null)
            => new GroupsEnabled(new[]
            {
                new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
            });

        public ImportFromDesigner ImportFromDesigner(Guid responsibleId, QuestionnaireDocument questionnaire, bool allowCensus, string assembly, long contentVersion)
            => new ImportFromDesigner(responsibleId, questionnaire, allowCensus, assembly, contentVersion);

        public InterviewCreated InterviewCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new InterviewCreated(
                userId: Guid.NewGuid(),
                questionnaireId: questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion: questionnaireVersion ?? 7);

        public InterviewFromPreloadedDataCreated InterviewFromPreloadedDataCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new InterviewFromPreloadedDataCreated(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1);

        public InterviewHardDeleted InterviewHardDeleted(Guid? userId = null)
            => new InterviewHardDeleted(userId: userId ?? Guid.NewGuid());

        public InterviewOnClientCreated InterviewOnClientCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new InterviewOnClientCreated(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1);

        public InterviewReceivedByInterviewer InterviewReceivedByInterviewer()
            => new InterviewReceivedByInterviewer();

        public InterviewReceivedBySupervisor InterviewReceivedBySupervisor()
            => new InterviewReceivedBySupervisor();

        public InterviewStatusChanged InterviewStatusChanged(InterviewStatus status, string comment = "hello")
            => new InterviewStatusChanged(status, comment);

        public InterviewSynchronized InterviewSynchronized(InterviewSynchronizationDto synchronizationDto)
            => new InterviewSynchronized(synchronizationDto);

        public InterviewCompleted InteviewCompleted()
            => new InterviewCompleted(
                Guid.NewGuid(),
                DateTime.UtcNow,
                "comment");

        public LinkedOptionsChanged LinkedOptionsChanged(ChangedLinkedOptions[] options = null)
            => new LinkedOptionsChanged(
                options ?? new ChangedLinkedOptions[] {});

        public MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(
            Guid? questionId = null,
            decimal[] rosterVector = null,
            decimal[][] selectedRosterVectors = null)
            => new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), 
                questionId ?? Guid.NewGuid(),
                rosterVector ?? new decimal[]{},
                DateTime.Now, 
                selectedRosterVectors ?? new decimal[][]{});

        public NewQuestionAdded NewQuestionAdded(
            Guid publicKey, Guid? groupPublicKey = null, string questionText = null, bool? isInteger = null,
            string stataExportCaption = null, Guid? linkedToQuestionId = null, bool capital = false, string variableLabel = null, string validationExpression = null, string validationMessage = null,
            QuestionScope questionScope = QuestionScope.Interviewer, string instructions = null, Answer[] answers = null, bool featured = false, Guid? responsibleId = null,
            QuestionType questionType = QuestionType.Text, bool? isFilteredCombobox = null, Guid? cascadeFromQuestionId = null, string conditionExpression = null, Order? answerOrder = null,
            string mask = null, int? maxAllowedAnswers = null, bool? yesNoView = null, bool? areAnswersOrdered = null, bool hideIfDisabled = false,
            QuestionProperties properties = null)
            => new NewQuestionAdded(
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

        public StaticTextsDisabled StaticTextsDisabled(params Identity[] ids)
            {
                return new StaticTextsDisabled(ids);
            }

        public StaticTextsEnabled StaticTextsEnabled(params Identity[] ids)
            {
                return new StaticTextsEnabled(ids);
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

        public SubstitutionTitlesChanged SubstitutionTitlesChanged(
                Identity[] questions = null, Identity[] staticTexts = null)
                => new SubstitutionTitlesChanged(
                    questions ?? new Identity[] {},
                    staticTexts ?? new Identity[] {});

        public TextQuestionAnswered TextQuestionAnswered(
                Guid? questionId = null, decimal[] rosterVector = null, string answer = null)
                => new TextQuestionAnswered(
                    Guid.NewGuid(),
                    questionId ?? Guid.NewGuid(),
                    rosterVector ?? WB.Core.SharedKernels.DataCollection.RosterVector.Empty,
                    DateTime.Now,
                    answer ?? "answer");

        public VariablesChanged VariablesChanged(params ChangedVariable[] changedVariables)
             => new VariablesChanged(changedVariables);

        public VariablesDisabled VariablesDisabled(params Identity[] identites)
                => new VariablesDisabled(identites);

        public VariablesEnabled VariablesEnabled(params Identity[] identites)
             => new VariablesEnabled(identites);

        public YesNoQuestionAnswered YesNoQuestionAnswered(Guid? questionId = null, AnsweredYesNoOption[] answeredOptions = null)
            {
                return new YesNoQuestionAnswered(
                    userId: Guid.NewGuid(),
                    questionId: questionId ?? Guid.NewGuid(),
                    rosterVector: Core.SharedKernels.DataCollection.RosterVector.Empty,
                    answerTimeUtc: DateTime.UtcNow,
                    answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});
            }

        public RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
        {
            return new RosterInstancesRemoved(new[]
                {
                    new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m)
                });
        }

        public RosterInstancesTitleChanged RosterInstancesTitleChanged(Guid? rosterId = null,
            string rosterTitle = null,
            decimal[] outerRosterVector = null,
            decimal? instanceId = null)
        {
            return new RosterInstancesTitleChanged(
                new[]
                {
                    new ChangedRosterInstanceTitleDto(new RosterInstance(rosterId ?? Guid.NewGuid(), outerRosterVector ?? new decimal[0], instanceId ?? 0.0m), rosterTitle ?? "title")
                });
        }
    }
}