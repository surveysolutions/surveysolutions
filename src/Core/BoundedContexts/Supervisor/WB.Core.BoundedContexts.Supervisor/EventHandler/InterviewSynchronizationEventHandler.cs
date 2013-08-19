using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using InterviewDeleted = Main.Core.Events.Questionnaire.Completed.InterviewDeleted;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSynchronizationEventHandler : IEventHandler<InterviewerAssigned>,
                                                        IEventHandler<InterviewRejected>,
                                                        IEventHandler<InterviewCompleted>,
                                                        IEventHandler<InterviewDeleted>, 
                                                        IEventHandler
    {
        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<InterviewData> interviewDataWriter;
        private readonly IReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnriePropagationStructures;

        public InterviewSynchronizationEventHandler(ISynchronizationDataStorage syncStorage,
                                                    IReadSideRepositoryWriter<InterviewData> interviewDataWriter,
                                                    IReadSideRepositoryWriter<QuestionnairePropagationStructure>
                                                        questionnriePropagationStructures)
        {
            this.syncStorage = syncStorage;
            this.interviewDataWriter = interviewDataWriter;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            var interview = interviewDataWriter.GetById(evnt.EventSourceId);

            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedTpUser(interview, evnt.Payload.InterviewerId, InterviewStatus.InterviewerAssigned);           
            
            syncStorage.SaveInterview(interviewSyncData, evnt.Payload.UserId);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            var interview = interviewDataWriter.GetById(evnt.EventSourceId);

            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedTpUser(interview, interview.ResponsibleId, InterviewStatus.RejectedBySupervisor);

            syncStorage.SaveInterview(interviewSyncData, interview.ResponsibleId);
        }

        private InterviewSynchronizationDto BuildSynchronizationDtoWhichIsAssignedTpUser(InterviewData interview, Guid userId, InterviewStatus status)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<ItemPublicKey>();
            var disabledQuestions = new HashSet<ItemPublicKey>();
            var invalidQuestions = new HashSet<ItemPublicKey>();
            var propagatedGroupInstanceCounts = new Dictionary<ItemPublicKey, int>();

            var questionnariePropagationStructure = this.questionnriePropagationStructures.GetById(interview.QuestionnaireId);

            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.Questions)
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Id,interviewLevel.PropagationVector,
                                                                                  interviewQuestion.Answer,
                                                                                  interviewQuestion.Comments);
                    answeredQuestions.Add(answeredQuestion);
                    if (!interviewQuestion.Enabled)
                        disabledQuestions.Add(new ItemPublicKey(interviewQuestion.Id, interviewLevel.PropagationVector));
                    if (!interviewQuestion.Valid)
                        invalidQuestions.Add(new ItemPublicKey(interviewQuestion.Id, interviewLevel.PropagationVector));
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new ItemPublicKey(disabledGroup, interviewLevel.PropagationVector));
                }

                FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(questionnariePropagationStructure, interviewLevel, propagatedGroupInstanceCounts);
            }
            return new InterviewSynchronizationDto(interview.InterviewId,
                                                   status,
                                                   userId, interview.QuestionnaireId,
                                                   answeredQuestions, disabledGroups, disabledQuestions,
                                                   invalidQuestions, propagatedGroupInstanceCounts);
        }

        private void FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(
            QuestionnairePropagationStructure questionnariePropagationStructure, InterviewLevel interviewLevel,
            Dictionary<ItemPublicKey, int> propagatedGroupInstanceCounts)
        {
            if (interviewLevel.PropagationVector.Length == 0)
                return;

            var outerVector = CreateOuterVector(interviewLevel);

            foreach (var groupId in questionnariePropagationStructure.PropagationScopes[interviewLevel.ScopeId])
            {
                var groupKey = new ItemPublicKey(groupId, outerVector);

                AddPropagatedGroupToDictionary(propagatedGroupInstanceCounts, groupKey);
            }
        }

        private void AddPropagatedGroupToDictionary(Dictionary<ItemPublicKey, int> propagatedGroupInstanceCounts,
                                                    ItemPublicKey groupKey)
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
            var outerVector = new int[interviewLevel.PropagationVector.Length - 1];
            for (int i = 0; i < interviewLevel.PropagationVector.Length - 1; i++)
            {
                outerVector[i] = interviewLevel.PropagationVector[i];
            }
            return outerVector;
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, null);

        }
        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, null);
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews {
            get { return new Type[] {typeof (InterviewData), typeof (QuestionnairePropagationStructure)}; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] {typeof (SynchronizationDelta)}; }
        }
    }
}
