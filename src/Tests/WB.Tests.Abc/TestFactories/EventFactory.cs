using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Events;
using Ncqrs.Eventing;
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
            => new AnswersDeclaredInvalid(
                questions.ToDictionary<Identity, Identity, IReadOnlyList<FailedValidationCondition>>(question => question, question => new List<FailedValidationCondition>()),
                DateTimeOffset.Now);


        internal AnswersDeclaredInvalid AnswerDeclaredInvalid(Identity identity, int[] failedValidations)
        {
            var errors = failedValidations.Select(x => Create.Entity.FailedValidationCondition(x)).ToReadOnlyCollection();
            var failedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {identity, errors}
            };
            return new AnswersDeclaredInvalid(failedValidationConditions, DateTimeOffset.Now);
        }

        public AggregateRootEvent AggregateRootEvent(Core.Infrastructure.EventBus.IEvent evnt)
        {
            var rnd = new Random();
            return new AggregateRootEvent(new CommittedEvent(Guid.NewGuid(), "origin", Guid.NewGuid(), Guid.NewGuid(),
                rnd.Next(1, 10000000), DateTime.UtcNow, rnd.Next(1, 1000000), evnt));
        }

        public AnswersDeclaredInvalid AnswersDeclaredInvalid(IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedConditions)
            => new AnswersDeclaredInvalid(failedConditions, DateTimeOffset.Now);

        public AnswersDeclaredImplausible AnswersDeclaredImplausible(Identity questionId, int[] failedConditions, DateTimeOffset? originDate = null)
        {
            return new AnswersDeclaredImplausible(new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>(){
                {
                    new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(questionId, failedConditions.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection())
                }
            }, originDate ?? DateTimeOffset.Now);
        }

        public AnswersDeclaredPlausible AnswersDeclaredPlausible( params Identity[] questions)
            => new AnswersDeclaredPlausible(questions, DateTimeOffset.Now);

        public AnswersDeclaredValid AnswersDeclaredValid( params Identity[] questions)
            => new AnswersDeclaredValid(questions, DateTimeOffset.Now);

        public AnswersDeclaredValid AnswersDeclaredValid(DateTimeOffset? originDate = null)
            => new AnswersDeclaredValid(new Identity[]{}, originDate?? DateTimeOffset.Now);

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

        public GroupsDisabled GroupsDisabled(Identity[] groups, DateTimeOffset? originDate = null)
            => new GroupsDisabled(groups, originDate?? DateTimeOffset.Now);

        public GroupsDisabled GroupsDisabled(Guid? id = null, decimal[] rosterVector = null, DateTimeOffset? originDate = null)
            => new GroupsDisabled(new[]
            {
                new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
            }, originDate?? DateTimeOffset.Now);

        public GroupsEnabled GroupsEnabled(Identity[] groups, DateTimeOffset? originDate = null)
            => new GroupsEnabled(groups, originDate?? DateTimeOffset.Now);

        public GroupsEnabled GroupsEnabled(Guid? id = null, decimal[] rosterVector = null, DateTimeOffset? originDate = null)
            => new GroupsEnabled(new[]
            {
                new Identity(id ?? Guid.NewGuid(), rosterVector ?? new decimal[0]), 
            }, originDate?? DateTimeOffset.Now);

        public ImportFromDesigner ImportFromDesigner(Guid responsibleId, QuestionnaireDocument questionnaire, bool allowCensus, string assembly, long contentVersion)
            => new ImportFromDesigner(responsibleId, questionnaire, allowCensus, assembly, contentVersion, 1);

        public InterviewCreated InterviewCreated(Guid? questionnaireId = null, long? questionnaireVersion = null,
            DateTimeOffset? originDate = null)
            => new InterviewCreated(
                userId: Guid.NewGuid(),
                questionnaireId: questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion: questionnaireVersion ?? 7,
                assignmentId: null,
                originDate: originDate ?? DateTimeOffset.Now,
                usesExpressionStorage: true);

        public InterviewFromPreloadedDataCreated InterviewFromPreloadedDataCreated(Guid? questionnaireId = null, long? questionnaireVersion = null, DateTimeOffset? originDate = null)
            => new InterviewFromPreloadedDataCreated(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1,
                null,
                originDate ?? DateTimeOffset.Now);

        public InterviewHardDeleted InterviewHardDeleted(Guid? userId = null, DateTimeOffset? originDate = null)
            => new InterviewHardDeleted(userId: userId ?? Guid.NewGuid(), originDate: originDate ?? DateTimeOffset.Now);

        public InterviewOnClientCreated InterviewOnClientCreated(Guid? questionnaireId = null, long? questionnaireVersion = null, DateTimeOffset? originDate = null)
            => new InterviewOnClientCreated(
                Guid.NewGuid(),
                questionnaireId ?? Guid.NewGuid(),
                questionnaireVersion ?? 1, 
                null,
                originDate ?? DateTimeOffset.Now);

        public InterviewReceivedByInterviewer InterviewReceivedByInterviewer(DateTimeOffset? originDate = null)
            => new InterviewReceivedByInterviewer(originDate ?? DateTimeOffset.Now);

        public InterviewReceivedBySupervisor InterviewReceivedBySupervisor(DateTimeOffset? originDate = null)
            => new InterviewReceivedBySupervisor(originDate ?? DateTimeOffset.Now);

        public InterviewStatusChanged InterviewStatusChanged(InterviewStatus status, string comment = "hello",InterviewStatus? previousStatus= null, DateTimeOffset? originDate = null)
            => new InterviewStatusChanged(status, comment, originDate ?? DateTimeOffset.Now, previousStatus);

        public InterviewSynchronized InterviewSynchronized(InterviewSynchronizationDto synchronizationDto, DateTimeOffset? originDate = null)
            => new InterviewSynchronized(synchronizationDto, originDate ?? DateTimeOffset.Now);

        public InterviewCompleted InteviewCompleted(DateTime? completionDate = null)
            => new InterviewCompleted(
                Guid.NewGuid(),
                completionDate ?? DateTime.UtcNow,
                "comment");

        public LinkedOptionsChanged LinkedOptionsChanged(ChangedLinkedOptions[] options = null, DateTimeOffset? originDate = null)
            => new LinkedOptionsChanged(
                options ?? new ChangedLinkedOptions[] {}, originDate ?? DateTimeOffset.Now);

        public LinkedToListOptionsChanged LinkedToListOptionsChanged(ChangedLinkedToListOptions[] options = null, DateTimeOffset? originDate = null)
            => new LinkedToListOptionsChanged(
                
                options ?? new ChangedLinkedToListOptions[] { }, originDate ?? DateTimeOffset.Now);

        public ChangedLinkedToListOptions ChangedLinkedToListOptions(Identity questionIdentity, decimal[] options = null)
            => new ChangedLinkedToListOptions(questionIdentity, options ?? new decimal[0]);

        public MultipleOptionsQuestionAnswered MultipleOptionsQuestionAnswered(
            Guid? questionId = null,
            decimal[] rosterVector = null,
            decimal[] selectedOptions = null,
            DateTimeOffset? originDate = null)
            => new MultipleOptionsQuestionAnswered(Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? new decimal[] { },
                DateTimeOffset.Now, 
                selectedOptions ?? new decimal[] { });

        public MultipleOptionsLinkedQuestionAnswered MultipleOptionsLinkedQuestionAnswered(
            Guid? questionId = null,
            decimal[] rosterVector = null,
            decimal[][] selectedRosterVectors = null,
            DateTimeOffset? originDate = null)
            => new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), 
                questionId ?? Guid.NewGuid(),
                rosterVector ?? new decimal[]{},
                DateTime.Now, 
                selectedRosterVectors ?? new decimal[][]{});

        public NumericIntegerQuestionAnswered NumericIntegerQuestionAnswered(
            Guid? questionId = null, decimal[] rosterVector = null, int? answer = null, Guid? userId = null,  DateTimeOffset? originDate = null)
            => new NumericIntegerQuestionAnswered(
                userId ?? Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                originDate ?? DateTimeOffset.Now,
                answer ?? 1);

        public PictureQuestionAnswered PictureQuestionAnswered(Guid? questionId = null, decimal[] rosterVector = null, string answer = null,  DateTimeOffset? originDate = null)
         => new PictureQuestionAnswered(
               Guid.NewGuid(),
               questionId ?? Guid.NewGuid(),
               rosterVector ?? RosterVector.Empty,
               originDate ?? DateTimeOffset.Now,
               answer ?? "file.png");

        public QuestionsDisabled QuestionsDisabled(Identity[] questions, DateTimeOffset? originDate = null)
            => new QuestionsDisabled(questions, originDate ?? DateTimeOffset.Now);

        public QuestionsDisabled QuestionsDisabled(Guid? id = null, decimal[] rosterVector = null)
            => Create.Event.QuestionsDisabled(new[]
            {
                Create.Entity.Identity(id ?? Guid.NewGuid(), rosterVector ?? RosterVector.Empty),
            });

        public QuestionsEnabled QuestionsEnabled(DateTimeOffset originDate , params Identity[] questions)
            => new QuestionsEnabled(questions, originDate);

        public QuestionsEnabled QuestionsEnabled( params Identity[] questions)
            => new QuestionsEnabled(questions, DateTimeOffset.Now);

        public QuestionsEnabled QuestionsEnabled(Guid? id = null, decimal[] rosterVector = null, DateTimeOffset? originDate = null)
            => Create.Event.QuestionsEnabled( originDate ?? DateTimeOffset.Now, 
                new[]
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
                    .ToArray(),
                DateTimeOffset.Now);

        public RosterInstancesAdded RosterInstancesAdded(Guid rosterId, params RosterVector[] fullRosterVectors)
            => new RosterInstancesAdded(
                fullRosterVectors
                    .Select(fullRosterVector => new AddedRosterInstance(
                        rosterId,
                        outerRosterVector: fullRosterVector.CoordinatesAsDecimals.Take(fullRosterVector.Length - 1).ToArray(),
                        rosterInstanceId: fullRosterVector.Last(),
                        sortIndex: null))
                    .ToArray(),
                DateTimeOffset.Now);

        public RosterInstancesAdded RosterInstancesAdded(
            Guid? rosterGroupId = null,
            decimal[] rosterVector = null,
            decimal? rosterInstanceId = null,
            int? sortIndex = null, 
            DateTimeOffset? originDate = null)
            => new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(rosterGroupId ?? Guid.NewGuid(), rosterVector ?? RosterVector.Empty, rosterInstanceId ?? 0.0m, sortIndex)
            },
                originDate ?? DateTimeOffset.Now);

        public RosterInstancesTitleChanged RosterInstancesTitleChanged(
            Guid? rosterId = null,
            decimal[] rosterVector = null,
            string rosterTitle = null,
            decimal[] outerRosterVector = null,
            decimal? instanceId = null,
            DateTimeOffset? originDate = null)
            => new RosterInstancesTitleChanged(new[]
            {
                new ChangedRosterInstanceTitleDto(
                    new RosterInstance(
                        rosterId ?? Guid.NewGuid(),
                        outerRosterVector ?? rosterVector?.Shrink() ?? RosterVector.Empty,
                        instanceId ?? rosterVector?.Last() ?? 0.0m),
                    rosterTitle ?? "title")
            }, originDate ?? DateTimeOffset.Now);

        public SingleOptionQuestionAnswered SingleOptionQuestionAnswered(Guid questionId, decimal[] rosterVector, decimal answer, Guid? userId = null, DateTime? answerDate = null)
            => new SingleOptionQuestionAnswered(userId ?? Guid.NewGuid(), questionId, rosterVector, answerDate ?? DateTime.UtcNow, answer);

        public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(params Identity[] staticTexts)
            => StaticTextsDeclaredInvalid(new[] {0}, staticTexts);

        public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> arg, DateTimeOffset? originDate = null)
           => new StaticTextsDeclaredInvalid(arg, originDate ?? DateTimeOffset.Now);

        public StaticTextsDeclaredImplausible StaticTextsDeclaredImplausible(Identity staticText, int[] conditions, DateTimeOffset? originDate = null)
        {
            return new StaticTextsDeclaredImplausible(new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>()
            {
                new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(staticText, conditions.Select(x => new FailedValidationCondition(x)).ToReadOnlyCollection())
            }, originDate ?? DateTimeOffset.Now);
        }

        public StaticTextsDeclaredPlausible StaticTextsDeclaredPlausible(params Identity[] staticTexts)
            => new StaticTextsDeclaredPlausible(staticTexts, DateTimeOffset.Now);

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
                    .ToList(), DateTimeOffset.Now);
        }

        public StaticTextsDeclaredValid StaticTextsDeclaredValid(params Identity[] staticTexts)
            => new StaticTextsDeclaredValid(staticTexts, DateTimeOffset.Now);

        public StaticTextsDisabled StaticTextsDisabled(DateTimeOffset? originDate = null, params Identity[] staticTexts)
            => new StaticTextsDisabled(staticTexts, originDate ?? DateTimeOffset.Now);

        public StaticTextsDisabled StaticTextsDisabled(params Identity[] staticTexts)
            => new StaticTextsDisabled(staticTexts, DateTimeOffset.Now);

        public StaticTextsEnabled StaticTextsEnabled(DateTimeOffset? originDate = null, params Identity[] staticTexts)
            => new StaticTextsEnabled(staticTexts, originDate ?? DateTimeOffset.Now);

        public StaticTextsEnabled StaticTextsEnabled( params Identity[] staticTexts)
            => new StaticTextsEnabled(staticTexts, DateTimeOffset.Now);

        public SubstitutionTitlesChanged SubstitutionTitlesChanged(Identity[] questions = null, Identity[] staticTexts = null, Identity[] groups = null, DateTimeOffset? originDate = null)
            => new SubstitutionTitlesChanged(
                questions ?? new Identity[] {},
                staticTexts ?? new Identity[] {},
                groups ?? new Identity[] {}, originDate ?? DateTimeOffset.Now);

        public TextQuestionAnswered TextQuestionAnswered(
            Guid? questionId = null, decimal[] rosterVector = null, string answer = null, Guid? userId = null, DateTimeOffset? originDate = null)
            => new TextQuestionAnswered(
                userId ?? Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                originDate ?? DateTimeOffset.Now,
                answer ?? "answer");

        public AnswerCommented AnswerCommented(
            Guid? questionId = null, 
            decimal[] rosterVector = null, 
            string comment = null, 
            Guid? userId = null,
            DateTimeOffset? originDate = null)
            => new AnswerCommented(userId ?? Guid.NewGuid(), 
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                originDate ?? DateTimeOffset.Now,
                comment ?? "Comment");

        public TextListQuestionAnswered TextListQuestionAnswered(
           Guid? questionId = null, decimal[] rosterVector = null, Tuple<decimal, string>[] answers = null, DateTimeOffset? originDate = null)
           => new TextListQuestionAnswered(
               Guid.NewGuid(),
               questionId ?? Guid.NewGuid(),
               rosterVector ?? RosterVector.Empty,
               originDate ?? DateTimeOffset.Now,
               answers ?? new Tuple<decimal, string>[0]);

        public VariablesChanged VariablesChanged(DateTimeOffset originDate, params ChangedVariable[] changedVariables)
             => new VariablesChanged(changedVariables, originDate );

        public VariablesChanged VariablesChanged(params ChangedVariable[] changedVariables)
            => new VariablesChanged(changedVariables, DateTimeOffset.Now);

        public VariablesDisabled VariablesDisabled(DateTimeOffset originDate , params Identity[] identites)
            => new VariablesDisabled(identites, originDate);

        public VariablesDisabled VariablesDisabled(params Identity[] identites)
            => new VariablesDisabled(identites, DateTimeOffset.Now);

        public VariablesEnabled VariablesEnabled(DateTimeOffset originDate, params Identity[] identites)
             => new VariablesEnabled(identites, originDate);

        public VariablesEnabled VariablesEnabled( params Identity[] identites)
            => new VariablesEnabled(identites, DateTimeOffset.Now);

        public YesNoQuestionAnswered YesNoQuestionAnswered(Guid? questionId = null, AnsweredYesNoOption[] answeredOptions = null, DateTimeOffset? originDate = null)
            => new YesNoQuestionAnswered(
                userId: Guid.NewGuid(),
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: RosterVector.Empty,
                originDate: originDate ?? DateTimeOffset.Now,
                answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] {});

        public RosterInstancesRemoved RosterInstancesRemoved(Guid? rosterGroupId = null, DateTimeOffset? originDate = null)
            => new RosterInstancesRemoved(new[]
            {
                new RosterInstance(rosterGroupId ?? Guid.NewGuid(), new decimal[0], 0.0m),
            }, originDate ?? DateTimeOffset.Now);

        public RosterInstancesRemoved RosterInstancesRemoved(Guid rosterGroupId, RosterVector[] rosterVectors, DateTimeOffset? originDate = null)
            => new RosterInstancesRemoved(
                rosterVectors
                    .Select(fullRosterVector => new RosterInstance(
                        rosterGroupId,
                        outerRosterVector: fullRosterVector.CoordinatesAsDecimals.Take(fullRosterVector.Length - 1).ToArray(),
                        rosterInstanceId: fullRosterVector.Last())).ToArray(), originDate ?? DateTimeOffset.Now);

        public SupervisorAssigned SupervisorAssigned(Guid userId, Guid supervisorId, DateTimeOffset? originDate = null)
            => new SupervisorAssigned(userId, supervisorId, originDate ?? DateTimeOffset.Now);

        public InterviewerAssigned InterviewerAssigned(Guid userId, Guid? interviewerId, DateTimeOffset? originDate = null)
            => new InterviewerAssigned(userId, interviewerId, originDate ?? DateTimeOffset.Now);

        public InterviewApproved InterviewApproved(Guid userId, string comment = null, DateTimeOffset? originDate = null)
            => new InterviewApproved(userId, comment, originDate ?? DateTimeOffset.Now);

        public InterviewRejectedByHQ InterviewRejectedByHQ(Guid userId, string comment = null, DateTimeOffset? originDate = null)
            => new InterviewRejectedByHQ(userId, comment, originDate ?? DateTimeOffset.Now);

        public InterviewRejected InterviewRejected(Guid userId, string comment = null, DateTimeOffset? originDate = null)
            => new InterviewRejected(userId, comment, originDate ?? DateTimeOffset.Now);

        public NumericRealQuestionAnswered NumericRealQuestionAnswered(Identity identity = null, decimal? answer = null, Guid? userId = null, DateTimeOffset? originDate = null)
        {
            return new NumericRealQuestionAnswered(
                   userId ?? Guid.NewGuid(),
                   identity?.Id ?? Guid.NewGuid(),
                   identity?.RosterVector ?? Create.Entity.RosterVector(),
                   originDate ?? DateTimeOffset.Now,
                   answer ?? 0);
        }

        public RosterInstancesAdded RosterInstancesAdded(AddedRosterInstance[] instances, DateTimeOffset? originDate = null)
        {
            return new RosterInstancesAdded(instances, originDate ?? DateTimeOffset.Now);
        }

        public RosterInstancesRemoved RosterInstancesRemoved(RosterInstance[] rosterInstances, DateTimeOffset? originDate = null)
        {
            return new RosterInstancesRemoved(rosterInstances, originDate ?? DateTimeOffset.Now);
        }

        public QuestionsMarkedAsReadonly QuestionsMarkedAsReadonly(DateTimeOffset? originDate = null, params Identity[] questions)
            => new QuestionsMarkedAsReadonly(questions, originDate ?? DateTimeOffset.Now);

        public InterviewKeyAssigned InterviewKeyAssigned(InterviewKey existingInterviewKey = null, DateTimeOffset? originDate = null)
        {
            return new InterviewKeyAssigned(existingInterviewKey ?? new InterviewKey(111), originDate ?? DateTimeOffset.Now);
        }

        public InterviewOpenedBySupervisor InterviewOpenedBySupervisor()
        {
            return new InterviewOpenedBySupervisor(Guid.NewGuid(), DateTime.Now);
        }

        public InterviewPaused InterviewPaused(DateTimeOffset? originDate = null)
        {
            return new InterviewPaused(Guid.NewGuid(), originDate ?? DateTimeOffset.Now);
        }

        public InterviewResumed InterviewResumed(DateTimeOffset? originDate = null)
        {
            return new InterviewResumed(Guid.NewGuid(), originDate ?? DateTimeOffset.Now);
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
