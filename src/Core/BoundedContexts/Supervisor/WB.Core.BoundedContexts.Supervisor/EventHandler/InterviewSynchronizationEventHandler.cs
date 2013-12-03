using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;
//using InterviewDeleted = Main.Core.Events.Questionnaire.Completed.InterviewDeleted;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSynchronizationEventHandler : IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewStatusChanged>,
    IEventHandler
    {
        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnriePropagationStructures;

        public InterviewSynchronizationEventHandler(ISynchronizationDataStorage syncStorage,
            IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter,
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>
                questionnriePropagationStructures)
        {
            this.syncStorage = syncStorage;
            this.interviewDataWriter = interviewDataWriter;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            ResendInterviewForPerson(evnt.EventSourceId, evnt.Payload.InterviewerId);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var newStatus = evnt.Payload.Status;

            if (IsInterviewWithStatusNeedToBeResendToCapi(newStatus))
            {
                ResendInterviewInNewStatus(evnt.EventSourceId, newStatus);
            }

            if (IsInterviewWithStatusNeedToBeDeletedOnCapi(newStatus))
            {
                DeleteInterview(evnt.EventSourceId);
            }
        }

        private bool IsInterviewWithStatusNeedToBeResendToCapi(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.RejectedBySupervisor;
        }

        private bool IsInterviewWithStatusNeedToBeDeletedOnCapi(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.Completed || newStatus == InterviewStatus.Deleted;
        }

        public void ResendInterviewInNewStatus(Guid interviewId, InterviewStatus status)
        {
            var interview = interviewDataWriter.GetById(interviewId);

            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedToUser(interview.Document, interview.Document.ResponsibleId, status);

            syncStorage.SaveInterview(interviewSyncData, interview.Document.ResponsibleId);
        }

        public void ResendInterviewForPerson(Guid interviewId,  Guid responsibleId)
        {
            var interview = interviewDataWriter.GetById(interviewId);

            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedToUser(interview.Document,
                responsibleId, InterviewStatus.InterviewerAssigned);

            syncStorage.SaveInterview(interviewSyncData, interview.Document.ResponsibleId);
        }

        public void DeleteInterview(Guid interviewId)
        {
            syncStorage.MarkInterviewForClientDeleting(interviewId, null);

        }

        private InterviewSynchronizationDto BuildSynchronizationDtoWhichIsAssignedToUser(InterviewData interview, Guid userId,
            InterviewStatus status)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();
            var propagatedGroupInstanceCounts = new Dictionary<InterviewItemId, int>();

            var questionnariePropagationStructure = this.questionnriePropagationStructures.GetById(interview.QuestionnaireId,
                interview.QuestionnaireVersion);

            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.GetAllQuestions())
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Id, interviewLevel.RosterVector,
                        interviewQuestion.Answer,
                        interviewQuestion.Comments.Any()
                            ? interviewQuestion.Comments.Last().Text
                            : null);
                    answeredQuestions.Add(answeredQuestion);
                    if (!interviewQuestion.Enabled)
                        disabledQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));

#warning TLK: validness flag misses undefined state
                    if (!interviewQuestion.Valid)
                        invalidQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                    if (interviewQuestion.Valid)
                        validQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new InterviewItemId(disabledGroup, interviewLevel.RosterVector));
                }

                FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(questionnariePropagationStructure, interviewLevel,
                    propagatedGroupInstanceCounts);
            }
            return new InterviewSynchronizationDto(interview.InterviewId,
                status,
                userId, interview.QuestionnaireId, interview.QuestionnaireVersion,
                answeredQuestions.ToArray(), disabledGroups, disabledQuestions,
                validQuestions, invalidQuestions, propagatedGroupInstanceCounts, interview.WasCompleted);
        }

        private void FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(
            QuestionnaireRosterStructure questionnarieRosterStructure, InterviewLevel interviewLevel,
            Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts)
        {
            if (interviewLevel.RosterVector.Length == 0)
                return;

            var outerVector = CreateOuterVector(interviewLevel);

            foreach (var scopeId in interviewLevel.ScopeIds)
            {
                foreach (var groupId in questionnarieRosterStructure.RosterScopes[scopeId])
                {
                    var groupKey = new InterviewItemId(groupId, outerVector);

                    AddPropagatedGroupToDictionary(propagatedGroupInstanceCounts, groupKey);
                }
            }
        }

        private void AddPropagatedGroupToDictionary(Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts,
            InterviewItemId groupKey)
        {
            if (propagatedGroupInstanceCounts.ContainsKey(groupKey))
            {
                propagatedGroupInstanceCounts[groupKey] = propagatedGroupInstanceCounts[groupKey] + 1;
            }
            else
            {
                propagatedGroupInstanceCounts.Add(groupKey, 1);
            }
        }

        private int[] CreateOuterVector(InterviewLevel interviewLevel)
        {
            var outerVector = new int[interviewLevel.RosterVector.Length - 1];
            for (int i = 0; i < interviewLevel.RosterVector.Length - 1; i++)
            {
                outerVector[i] = interviewLevel.RosterVector[i];
            }
            return outerVector;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[] {typeof (InterviewData), typeof (QuestionnaireRosterStructure)}; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] {typeof (SynchronizationDelta)}; }
        }
    }
}
