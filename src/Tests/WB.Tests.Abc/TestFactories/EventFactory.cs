using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Abc.TestFactories
{
    internal class EventFactory
    {
        public AnswersDeclaredInvalid AnswersDeclaredInvalid(Guid? id = null, decimal[] rosterVector = null)
            => Create.Event.AnswersDeclaredInvalid(questions: new[]
            {
                new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]),
            });

        public AnswersDeclaredInvalid AnswersDeclaredInvalid(params Identity[] questions)
            => new AnswersDeclaredInvalid(questions);


        public AggregateRootEvent AggregateRootEvent(Core.Infrastructure.EventBus.IEvent evnt)
        {
            var rnd = new Random();
            return new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), "origin", Guid.NewGuid(), Guid.NewGuid(),
                rnd.Next(1, 10000000), DateTime.UtcNow, rnd.Next(1, 1000000), evnt));
        }

        public AnswersDeclaredInvalid AnswersDeclaredInvalid(IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedConditions)
            => new AnswersDeclaredInvalid(failedConditions);

        public AnswersDeclaredImplausible AnswersDeclaredImplausible(Identity questionId, int[] failedConditions)
        {
            return new AnswersDeclaredImplausible(new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>(){
                {
                    new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(questionId, failedConditions.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection())
                }
            });
        }

        public AnswersDeclaredPlausible AnswersDeclaredPlausible(params Identity[] questions)
            => new AnswersDeclaredPlausible(questions);

        public AnswersDeclaredValid AnswersDeclaredValid(params Identity[] questions)
            => new AnswersDeclaredValid(questions);

        public AnswersDeclaredValid AnswersDeclaredValid()
            => new AnswersDeclaredValid(new Identity[]{});

        public AnswersRemoved AnswersRemoved(params Identity[] questions)
            => new AnswersRemoved(questions, DateTime.UtcNow);

        public ChangedVariable ChangedVariable(Identity variableIdentity, object value)
            => new ChangedVariable(variableIdentity, value);

        public DateTimeQuestionAnswered DateTimeQuestionAnswered(Guid interviewId, Identity question, DateTime answer)
            => new DateTimeQuestionAnswered(interviewId, question.Id, question.RosterVector, DateTime.UtcNow, answer);

        public DateTimeQuestionAnswered DateTimeQuestionAnswered(
            Guid questionId, DateTime answer, decimal[] propagationVector = null)
        {
            return new DateTimeQuestionAnswered(
                 Guid.NewGuid(),
                 questionId,
                 propagationVector ?? RosterVector.Empty,
                 DateTime.UtcNow, 
                 answer);
        }

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
            => new ImportFromDesigner(responsibleId, questionnaire, allowCensus, assembly, contentVersion, 1);

        public InterviewCreated InterviewCreated(Guid? questionnaireId = null, long? questionnaireVersion = null,
            DateTime? creationTime = null)
            => new InterviewCreated(
                userId: Guid.NewGuid(),
                questionnaireId: questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion: questionnaireVersion ?? 7,
                assignmentId: null,
                creationTime: creationTime ?? DateTime.UtcNow,
                usesExpressionStorage: true);

        public InterviewFromPreloadedDataCreated InterviewFromPreloadedDataCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new InterviewFromPreloadedDataCreated(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1, 
                null);

        public InterviewHardDeleted InterviewHardDeleted(Guid? userId = null)
            => new InterviewHardDeleted(userId: userId ?? Guid.NewGuid());

        public InterviewOnClientCreated InterviewOnClientCreated(Guid? questionnaireId = null, long? questionnaireVersion = null)
            => new InterviewOnClientCreated(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1, null);

        public InterviewReceivedByInterviewer InterviewReceivedByInterviewer()
            => new InterviewReceivedByInterviewer();

        public InterviewReceivedBySupervisor InterviewReceivedBySupervisor()
            => new InterviewReceivedBySupervisor();

        public InterviewStatusChanged InterviewStatusChanged(InterviewStatus status, string comment = "hello",InterviewStatus? previousStatus= null, DateTime? utcTime= null)
            => new InterviewStatusChanged(status, comment, previousStatus, utcTime);

        public InterviewSynchronized InterviewSynchronized(InterviewSynchronizationDto synchronizationDto)
            => new InterviewSynchronized(synchronizationDto);

        public InterviewCompleted InteviewCompleted(DateTime? completionDate = null)
            => new InterviewCompleted(
                Guid.NewGuid(),
                completionDate ?? DateTime.UtcNow,
                "comment");

        public LinkedOptionsChanged LinkedOptionsChanged(ChangedLinkedOptions[] options = null)
            => new LinkedOptionsChanged(
                options ?? new ChangedLinkedOptions[] {});

        public LinkedToListOptionsChanged LinkedToListOptionsChanged(ChangedLinkedToListOptions[] options = null)
            => new LinkedToListOptionsChanged(
                
                options ?? new ChangedLinkedToListOptions[] { });

        public ChangedLinkedToListOptions ChangedLinkedToListOptions(Identity questionIdentity, decimal[] options = null)
            => new ChangedLinkedToListOptions(questionIdentity, options ?? new decimal[0]);

        public MultipleOptionsQuestionAnswered MultipleOptionsQuestionAnswered(
            Guid? questionId = null,
            decimal[] rosterVector = null,
            decimal[] selectedOptions = null)
            => new MultipleOptionsQuestionAnswered(Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? new decimal[] { },
                DateTime.Now,
                selectedOptions ?? new decimal[] { });

        public MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(
            Guid? questionId = null,
            decimal[] rosterVector = null,
            decimal[][] selectedRosterVectors = null)
            => new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), 
                questionId ?? Guid.NewGuid(),
                rosterVector ?? new decimal[]{},
                DateTime.Now, 
                selectedRosterVectors ?? new decimal[][]{});

        public NumericIntegerQuestionAnswered NumericIntegerQuestionAnswered(
            Guid? questionId = null, decimal[] rosterVector = null, int? answer = null, Guid? userId = null, DateTime? answerDate = null)
            => new NumericIntegerQuestionAnswered(
                userId ?? Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                answerDate ?? DateTime.UtcNow,
                answer ?? 1);

        public PictureQuestionAnswered PictureQuestionAnswered(Guid? questionId = null, decimal[] rosterVector = null, string answer = null, DateTime? answerTimeUtc = null)
         => new PictureQuestionAnswered(
               Guid.NewGuid(),
               questionId ?? Guid.NewGuid(),
               rosterVector ?? RosterVector.Empty,
               answerTimeUtc ?? DateTime.Now,
               answer ?? "file.png");

        public QuestionsDisabled QuestionsDisabled(Identity[] questions)
            => new QuestionsDisabled(questions);

        public QuestionsDisabled QuestionsDisabled(Guid? id = null, decimal[] rosterVector = null)
            => Create.Event.QuestionsDisabled(new[]
            {
                Create.Entity.Identity(id ?? Guid.NewGuid(), rosterVector ?? RosterVector.Empty),
            });

        public QuestionsEnabled QuestionsEnabled(params Identity[] questions)
            => new QuestionsEnabled(questions);

        public QuestionsEnabled QuestionsEnabled(Guid? id = null, decimal[] rosterVector = null)
            => Create.Event.QuestionsEnabled(new[]
            {
                Create.Entity.Identity(id ?? Guid.NewGuid(), rosterVector ?? RosterVector.Empty),
            });

        public RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params decimal[][] fullRosterVectors)
            => new RosterInstancesAdded(
                fullRosterVectors
                    .Select(fullRosterVector => new AddedRosterInstance(
                        rosterId,
                        outerRosterVector: fullRosterVector.Take(fullRosterVector.Length - 1).ToArray(),
                        rosterInstanceId: fullRosterVector.Last(),
                        sortIndex: null))
                    .ToArray());

        public RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params RosterVector[] fullRosterVectors)
            => new RosterInstancesAdded(
                fullRosterVectors
                    .Select(fullRosterVector => new AddedRosterInstance(
                        rosterId,
                        outerRosterVector: fullRosterVector.CoordinatesAsDecimals.Take(fullRosterVector.Length - 1).ToArray(),
                        rosterInstanceId: fullRosterVector.Last(),
                        sortIndex: null))
                    .ToArray());

        public RosterInstancesAdded RosterInstancesAdded(
            Guid? rosterGroupId = null,
            decimal[] rosterVector = null,
            decimal? rosterInstanceId = null,
            int? sortIndex = null)
            => new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(rosterGroupId ?? Guid.NewGuid(), rosterVector ?? RosterVector.Empty, rosterInstanceId ?? 0.0m, sortIndex)
            });

        public RosterInstancesTitleChanged RosterInstancesTitleChanged(
            Guid? rosterId = null,
            decimal[] rosterVector = null,
            string rosterTitle = null,
            decimal[] outerRosterVector = null,
            decimal? instanceId = null)
            => new RosterInstancesTitleChanged(new[]
            {
                new ChangedRosterInstanceTitleDto(
                    new RosterInstance(
                        rosterId ?? Guid.NewGuid(),
                        outerRosterVector ?? rosterVector?.Shrink() ?? RosterVector.Empty,
                        instanceId ?? rosterVector?.Last() ?? 0.0m),
                    rosterTitle ?? "title")
            });

        public SingleOptionQuestionAnswered SingleOptionQuestionAnswered(Guid questionId, decimal[] rosterVector, decimal answer, Guid? userId = null, DateTime? answerDate = null)
            => new SingleOptionQuestionAnswered(userId ?? Guid.NewGuid(), questionId, rosterVector, answerDate ?? DateTime.UtcNow, answer);

        public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(params Identity[] staticTexts)
            => StaticTextsDeclaredInvalid(new[] {0}, staticTexts);

        public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> arg)
           => new StaticTextsDeclaredInvalid(arg);

        public StaticTextsDeclaredImplausible StaticTextsDeclaredImplausible(Identity staticText, int[] conditions)
        {
            return new StaticTextsDeclaredImplausible(new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>()
            {
                new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(staticText, conditions.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection())
            });
        }

        public StaticTextsDeclaredPlausible StaticTextsDeclaredPlausible(params Identity[] staticTexts)
            => new StaticTextsDeclaredPlausible(staticTexts);

        public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(int[] failedConditionIndexes,
            params Identity[] staticTexts)
        {
            var failedValidationConditions = failedConditionIndexes.Select(
                x => Create.Entity.FailedValidationCondition(failedConditionIndex: x))
                .ToReadOnlyCollection();
               
            return new StaticTextsDeclaredInvalid(
                staticTexts
                    .Select(identity => new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(
                        identity,
                        failedValidationConditions))
                    .ToList());
        }

        public StaticTextsDeclaredValid StaticTextsDeclaredValid(params Identity[] staticTexts)
            => new StaticTextsDeclaredValid(staticTexts);

        public StaticTextsDisabled StaticTextsDisabled(params Identity[] staticTexts)
            => new StaticTextsDisabled(staticTexts);

        public StaticTextsEnabled StaticTextsEnabled(params Identity[] staticTexts)
            => new StaticTextsEnabled(staticTexts);

        public SubstitutionTitlesChanged SubstitutionTitlesChanged(Identity[] questions = null, Identity[] staticTexts = null, Identity[] groups = null)
            => new SubstitutionTitlesChanged(
                questions ?? new Identity[] {},
                staticTexts ?? new Identity[] {},
                groups ?? new Identity[] {});

        public TextQuestionAnswered TextQuestionAnswered(
            Guid? questionId = null, decimal[] rosterVector = null, string answer = null, Guid? userId = null, DateTime? answerTime = null)
            => new TextQuestionAnswered(
                userId ?? Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                answerTime??DateTime.Now,
                answer ?? "answer");

        public AnswerCommented AnswerCommented(
            Guid? questionId = null, 
            decimal[] rosterVector = null, 
            string comment = null, 
            Guid? userId = null,
            DateTime? answerTime = null)
            => new AnswerCommented(userId ?? Guid.NewGuid(), 
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                answerTime ?? DateTime.Now,
                comment ?? "Comment");

        public TextListQuestionAnswered TextListQuestionAnswered(
           Guid? questionId = null, decimal[] rosterVector = null, Tuple<decimal, string>[] answers = null, DateTime? answerTimeUtc = null)
           => new TextListQuestionAnswered(
               Guid.NewGuid(),
               questionId ?? Guid.NewGuid(),
               rosterVector ?? RosterVector.Empty,
               answerTimeUtc ?? DateTime.Now,
               answers ?? new Tuple<decimal, string>[0]);

        public VariablesChanged VariablesChanged(params ChangedVariable[] changedVariables)
             => new VariablesChanged(changedVariables);

        public VariablesDisabled VariablesDisabled(params Identity[] identites)
            => new VariablesDisabled(identites);

        public VariablesEnabled VariablesEnabled(params Identity[] identites)
             => new VariablesEnabled(identites);

        public YesNoQuestionAnswered YesNoQuestionAnswered(Guid? questionId = null, AnsweredYesNoOption[] answeredOptions = null)
            => new YesNoQuestionAnswered(
                userId: Guid.NewGuid(),
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: RosterVector.Empty,
                answerTimeUtc: DateTime.UtcNow,
                answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});

        public RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null)
            => new RosterInstancesRemoved(new[]
            {
                new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m),
            });

        public RosterInstancesRemoved RosterInstancesRemoved(Guid rosterGroupId, RosterVector[] rosterVectors)
            => new RosterInstancesRemoved(
                rosterVectors
                    .Select(fullRosterVector => new RosterInstance(
                        rosterGroupId,
                        outerRosterVector: fullRosterVector.CoordinatesAsDecimals.Take(fullRosterVector.Length - 1).ToArray(),
                        rosterInstanceId: fullRosterVector.Last())).ToArray());

        public SupervisorAssigned SupervisorAssigned(Guid userId, Guid supervisorId)
            => new SupervisorAssigned(userId, supervisorId);

        public InterviewerAssigned InterviewerAssigned(Guid userId, Guid? interviewerId, DateTime? assignTime)
            => new InterviewerAssigned(userId, interviewerId, assignTime);

        public InterviewApproved InterviewApproved(Guid userId, string comment = null, DateTime? approveTime = null)
            => new InterviewApproved(userId, comment, approveTime);

        public InterviewRejectedByHQ InterviewRejectedByHQ(Guid userId, string comment = null)
            => new InterviewRejectedByHQ(userId, comment);

        public InterviewRejected InterviewRejected(Guid userId, string comment = null, DateTime? rejectTime = null)
            => new InterviewRejected(userId, comment, rejectTime);

        public NumericRealQuestionAnswered NumericRealQuestionAnswered(Identity identity = null, decimal? answer = null, Guid? userId = null, DateTime? answerTime = null)
        {
            return new NumericRealQuestionAnswered(
                   userId ?? Guid.NewGuid(),
                   identity?.Id ?? Guid.NewGuid(),
                   identity?.RosterVector ?? Create.Entity.RosterVector(),
                   answerTime ?? DateTime.UtcNow,
                   answer ?? 0);
        }

        public RosterInstancesAdded RosterInstancesAdded(AddedRosterInstance[] instances)
        {
            return new RosterInstancesAdded(instances);
        }

        public RosterInstancesRemoved RosterInstancesRemoved(RosterInstance[] rosterInstances)
        {
            return new RosterInstancesRemoved(rosterInstances);
        }

        public QuestionsMarkedAsReadonly QuestionsMarkedAsReadonly(params Identity[] questions)
            => new QuestionsMarkedAsReadonly(questions);

        public InterviewKeyAssigned InterviewKeyAssigned(InterviewKey existingInterviewKey = null)
        {
            return new InterviewKeyAssigned(existingInterviewKey ?? new InterviewKey(111));
        }

        public InterviewOpenedBySupervisor InterviewOpenedBySupervisor()
        {
            return new InterviewOpenedBySupervisor(Guid.NewGuid(), DateTime.Now);
        }

        public InterviewPaused InterviewPaused(DateTime? localTime = null, DateTime? utcTime = null)
        {
            return new InterviewPaused(Guid.NewGuid(), localTime ?? DateTime.Now, utcTime ?? DateTime.UtcNow);
        }

        public InterviewResumed InterviewResumed(DateTime? localTime = null, DateTime? utcTime = null)
        {
            return new InterviewResumed(Guid.NewGuid(), localTime ?? DateTime.Now, utcTime ?? DateTime.UtcNow);
        }

        public InterviewOpenedBySupervisor InterviewOpenedBySupervisor(DateTime? localTime = null)
        {
            return new InterviewOpenedBySupervisor(Guid.NewGuid(), localTime ?? DateTime.Now);
        }

        public InterviewClosedBySupervisor InterviewClosedBySupervisor(DateTime? localTime = null)
        {
            return new InterviewClosedBySupervisor(Guid.NewGuid(), localTime ?? DateTime.Now);
        }
    }
}
