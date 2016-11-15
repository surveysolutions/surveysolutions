using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;


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

        public DateTimeQuestionAnswered DateTimeQuestionAnswered(Guid interviewId, Identity question, DateTime answer)
            => new DateTimeQuestionAnswered(interviewId, question.Id, question.RosterVector, DateTime.UtcNow, answer);

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

        public NumericIntegerQuestionAnswered NumericIntegerQuestionAnswered(
            Guid? questionId = null, decimal[] rosterVector = null, int? answer = null)
            => new NumericIntegerQuestionAnswered(
                Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? RosterVector.Empty,
                DateTime.Now,
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

        public QuestionsEnabled QuestionsEnabled(Identity[] questions)
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
                        outerRosterVector: fullRosterVector.Take(fullRosterVector.Length - 1).ToArray(),
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
                        outerRosterVector ?? rosterVector?.WithoutLast().ToArray() ?? RosterVector.Empty,
                        instanceId ?? rosterVector?.Last() ?? 0.0m),
                    rosterTitle ?? "title")
            });

        public SingleOptionQuestionAnswered SingleOptionQuestionAnswered(Guid questionId, decimal[] rosterVector, decimal answer, Guid? userId = null)
            => new SingleOptionQuestionAnswered(userId ?? Guid.NewGuid(), questionId, rosterVector, DateTime.UtcNow, answer);

        public StaticTextsDeclaredInvalid StaticTextsDeclaredInvalid(params Identity[] staticTexts)
            => new StaticTextsDeclaredInvalid(
                staticTexts
                    .Select(identity => new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(
                        identity,
                        Create.Entity.FailedValidationCondition(failedConditionIndex: 0).ToEnumerable().ToReadOnlyCollection()))
                    .ToList());

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
            Guid? questionId = null, decimal[] rosterVector = null, string answer = null)
            => new TextQuestionAnswered(
                Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                rosterVector ?? WB.Core.SharedKernels.DataCollection.RosterVector.Empty,
                DateTime.Now,
                answer ?? "answer");

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
                rosterVector: Core.SharedKernels.DataCollection.RosterVector.Empty,
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
                        outerRosterVector: fullRosterVector.Take(fullRosterVector.Length - 1).ToArray(),
                        rosterInstanceId: fullRosterVector.Last())).ToArray());

        public SupervisorAssigned SupervisorAssigned(Guid userId, Guid supervisorId)
            => new SupervisorAssigned(userId, supervisorId);

        public InterviewerAssigned InterviewerAssigned(Guid userId, Guid interviewerId, DateTime? assignTime)
            => new InterviewerAssigned(userId, interviewerId, assignTime);

        public InterviewApproved InterviewApproved(Guid userId, string comment = null, DateTime? approveTime = null)
            => new InterviewApproved(userId, comment, approveTime);

        public InterviewRejectedByHQ InterviewRejectedByHQ(Guid userId, string comment = null)
            => new InterviewRejectedByHQ(userId, comment);

        public InterviewRejected InterviewRejected(Guid userId, string comment = null, DateTime? rejectTime = null)
            => new InterviewRejected(userId, comment, rejectTime);
    }
}