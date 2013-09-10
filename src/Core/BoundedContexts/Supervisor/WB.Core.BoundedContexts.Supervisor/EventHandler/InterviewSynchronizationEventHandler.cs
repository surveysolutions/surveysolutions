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
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using InterviewDeleted = Main.Core.Events.Questionnaire.Completed.InterviewDeleted;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class InterviewSynchronizationEventHandler : IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<AnswerCommented>,
        /*,
                                                        IEventHandler<InterviewRejected>,
                                                        IEventHandler<InterviewCompleted>,
                                                        IEventHandler<InterviewDeleted>, */
        IEventHandler<AnswerDeclaredValid>,IEventHandler<GroupPropagated>,IEventHandler<QuestionEnabled>,IEventHandler<AnswerDeclaredInvalid>,IEventHandler<QuestionDisabled>,IEventHandler<GroupDisabled>,IEventHandler<InterviewCompleted>,
    IEventHandler
    {
        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<InterviewData> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure> questionnriePropagationStructures;

        public InterviewSynchronizationEventHandler(ISynchronizationDataStorage syncStorage,
                                                    IReadSideRepositoryWriter<InterviewData> interviewDataWriter,
                                                    IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure>
                                                        questionnriePropagationStructures)
        {
            this.syncStorage = syncStorage;
            this.interviewDataWriter = interviewDataWriter;
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            ResendInterviewInStatus(evnt.EventSourceId, InterviewStatus.InterviewerAssigned);
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            var newStatus = evnt.Payload.Status;

            if (IsInterviewWithStatusNeedToBeResendToCapi(newStatus))
            {
                ResendInterviewInStatus(evnt.EventSourceId, newStatus);
            }

            if (IsInterviewWithStatusNeedToBeDeletedOnCapi(newStatus))
            {
                DeleteInterview(evnt.EventSourceId);
            }
        }

        private  bool IsInterviewWithStatusNeedToBeResendToCapi(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.RejectedBySupervisor;
        }
        private bool IsInterviewWithStatusNeedToBeDeletedOnCapi(InterviewStatus newStatus)
        {
            return newStatus == InterviewStatus.Completed || newStatus == InterviewStatus.Deleted;
        }

        public void ResendInterviewInStatus(Guid interviewId, InterviewStatus status)
        {
            var interview = interviewDataWriter.GetById(interviewId);

            var interviewSyncData = BuildSynchronizationDtoWhichIsAssignedTpUser(interview,
                                                                                 interview.ResponsibleId, status);

            syncStorage.SaveInterview(interviewSyncData, interview.ResponsibleId);
        }

        public void DeleteInterview(Guid interviewId)
        {
            syncStorage.MarkInterviewForClientDeleting(interviewId, null);

        }

        private InterviewSynchronizationDto BuildSynchronizationDtoWhichIsAssignedTpUser(InterviewData interview, Guid userId, InterviewStatus status)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();
            var propagatedGroupInstanceCounts = new Dictionary<InterviewItemId, int>();

            var questionnariePropagationStructure = this.questionnriePropagationStructures.GetById(interview.QuestionnaireId, interview.QuestionnaireVersion);

            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.Questions)
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Id, interviewLevel.PropagationVector,
                                                                                  interviewQuestion.Answer,
                                                                                  interviewQuestion.Comments.Any() ?
                                                                                        interviewQuestion.Comments.Last().Text :
                                                                                        null);
                    answeredQuestions.Add(answeredQuestion);
                    if (!interviewQuestion.Enabled)
                        disabledQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.PropagationVector));

                    #warning TLK: validness flag misses undefined state
                    if (!interviewQuestion.Valid)
                        invalidQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.PropagationVector));
                    if (interviewQuestion.Valid)
                        validQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.PropagationVector));
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new InterviewItemId(disabledGroup, interviewLevel.PropagationVector));
                }

                FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(questionnariePropagationStructure, interviewLevel, propagatedGroupInstanceCounts);
            }
            return new InterviewSynchronizationDto(interview.InterviewId,
                                                   status,
                                                   userId, interview.QuestionnaireId, interview.QuestionnaireVersion,
                                                   answeredQuestions, disabledGroups, disabledQuestions,
                                                   validQuestions, invalidQuestions, propagatedGroupInstanceCounts);
        }

        private void FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(
            QuestionnairePropagationStructure questionnariePropagationStructure, InterviewLevel interviewLevel,
            Dictionary<InterviewItemId, int> propagatedGroupInstanceCounts)
        {
            if (interviewLevel.PropagationVector.Length == 0)
                return;

            var outerVector = CreateOuterVector(interviewLevel);

            foreach (var groupId in questionnariePropagationStructure.PropagationScopes[interviewLevel.ScopeId])
            {
                var groupKey = new InterviewItemId(groupId, outerVector);

                AddPropagatedGroupToDictionary(propagatedGroupInstanceCounts, groupKey);
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
            var outerVector = new int[interviewLevel.PropagationVector.Length - 1];
            for (int i = 0; i < interviewLevel.PropagationVector.Length - 1; i++)
            {
                outerVector[i] = interviewLevel.PropagationVector[i];
            }
            return outerVector;
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

        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionEnabled> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionDisabled> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<GroupDisabled> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {

        }
    }
}
