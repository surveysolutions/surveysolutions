using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    internal class InterviewSynchronizationDtoFactory : IInterviewSynchronizationDtoFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> interviewsRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewSynchronizationDtoFactory(
            IReadSideRepositoryWriter<InterviewStatuses> interviewsRepository,
            IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage, 
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore, 
            IQuestionnaireStorage questionnaireStorage)
        {
            if (interviewsRepository == null) throw new ArgumentNullException(nameof(interviewsRepository));
            
            this.interviewsRepository = interviewsRepository;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewSynchronizationDto BuildFrom(InterviewData interview, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime)
        {
            var result = this.BuildFrom(interview, interview.ResponsibleId, interview.Status, comments, rejectedDateTime, interviewerAssignedDateTime);
            return result;
        }

        private static string GetLastComment(InterviewQuestion question)
        {
            if (question.Comments == null || !question.Comments.Any())
                return null;
            return question.Comments.Last().Text;
        }

        public InterviewSynchronizationDto BuildFrom(InterviewData interview,
            Guid userId, InterviewStatus status, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var disabledStaticTexts = new List<Identity>();
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();

            var validStaticTexts = new List<Identity>();
            var invalidStaticTexts = new List<KeyValuePair<Identity, List<FailedValidationCondition>>>();

            var propagatedGroupInstanceCounts = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();

            var questionnariePropagationStructure =
                this.questionnaireRosterStructureStorage.GetById(
                    new QuestionnaireIdentity(interview.QuestionnaireId,
                        interview.QuestionnaireVersion).ToString());

            Dictionary<Identity, IList<FailedValidationCondition>> failedValidationConditions = new Dictionary<Identity, IList<FailedValidationCondition>>();
            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.QuestionsSearchCache)
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Key,
                        interviewLevel.RosterVector,
                        interviewQuestion.Value.Answer,
                        GetAllComments(interviewQuestion.Value));

                    if (!answeredQuestion.IsEmpty())
                    {
                        answeredQuestions.Add(answeredQuestion);
                    }
                    if (interviewQuestion.Value.IsDisabled())
                    {
                        disabledQuestions.Add(new InterviewItemId(interviewQuestion.Value.Id, interviewLevel.RosterVector));
                    }

                    if (interviewQuestion.Value.IsInvalid())
                    {
                        invalidQuestions.Add(new InterviewItemId(interviewQuestion.Key, interviewLevel.RosterVector));
                        failedValidationConditions.Add(new Identity(interviewQuestion.Key, interviewLevel.RosterVector), interviewQuestion.Value.FailedValidationConditions.ToList());
                    }
                    if (!interviewQuestion.Value.IsInvalid())
                    {
                        validQuestions.Add(new InterviewItemId(interviewQuestion.Key, interviewLevel.RosterVector));
                    }
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new InterviewItemId(disabledGroup, interviewLevel.RosterVector));
                }
                foreach (var staticText in interviewLevel.StaticTexts.Values)
                {
                    var staticTextIdentity = new Identity(staticText.Id, interviewLevel.RosterVector);
                    if (!staticText.IsEnabled)
                    {
                        disabledStaticTexts.Add(staticTextIdentity);
                    }

                    if (staticText.IsInvalid)
                    {
                        invalidStaticTexts.Add(new KeyValuePair<Identity, List<FailedValidationCondition>>(
                            staticTextIdentity, staticText.FailedValidationConditions.ToList()));
                    }

                    if (!staticText.IsInvalid)
                    {
                        validStaticTexts.Add(staticTextIdentity);
                    }
                }

                this.FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(questionnariePropagationStructure, interviewLevel,
                    propagatedGroupInstanceCounts);
            }

            if (!interviewerAssignedDateTime.HasValue)
            {
                var interviewStatusChangeHistory = this.interviewsRepository.GetById(new ChangeStatusInputModel {InterviewId = interview.InterviewId}.InterviewId);

                var commentedStatusHistroyViews = interviewStatusChangeHistory?
                    .InterviewCommentedStatuses
                    .Where(i => i.Status.ConvertToInterviewStatus().HasValue)
                    .Select(x => new CommentedStatusHistroyView
                    {
                        Comment = x.Comment,
                        Date = x.Timestamp,
                        Status = x.Status.ConvertToInterviewStatus().Value,
                        Responsible = x.StatusChangeOriginatorName
                    })
                    .ToList();

                if (commentedStatusHistroyViews != null)
                {
                    var interviewerAssignedStatus =
                        commentedStatusHistroyViews.OrderBy(interviewStatus => interviewStatus.Date).LastOrDefault(
                            interviewStatus => interviewStatus.Status == InterviewStatus.InterviewerAssigned);

                    if (interviewerAssignedStatus != null)
                    {
                        interviewerAssignedDateTime = interviewerAssignedStatus.Date;
                    }   
                }
            }
            Dictionary<InterviewItemId, RosterVector[]> linkedQuestionOptions = this.CreateLinkedQuestionsOptions(interview);
            
            Dictionary<InterviewItemId, object> variableValues = this.CreateVariableValues(interview);
            HashSet<InterviewItemId> disabledVariables = CreateDisabledVariables(interview);

            return new InterviewSynchronizationDto(interview.InterviewId,
                status, 
                comments,
                rejectedDateTime,
                interviewerAssignedDateTime,
                userId,
                interview.SupervisorId,
                interview.QuestionnaireId,
                interview.QuestionnaireVersion,
                answeredQuestions.ToArray(),
                disabledGroups,
                disabledQuestions,
                disabledStaticTexts,
                validQuestions,
                invalidQuestions,
                validStaticTexts,
                invalidStaticTexts,
                propagatedGroupInstanceCounts,
                failedValidationConditions.ToList(),
                linkedQuestionOptions,
                variableValues,
                disabledVariables,
                interview.WasCompleted,
                interview.CreatedOnClient);
        }

        private static HashSet<InterviewItemId> CreateDisabledVariables(InterviewData interview)
        {
            var result = new HashSet<InterviewItemId>();

            foreach (var interviewLevel in interview.Levels)
            {
                foreach (var disabledVariable in interviewLevel.Value.DisabledVariables)
                {
                    result.Add(new InterviewItemId(disabledVariable,interviewLevel.Value.RosterVector));
                }
            }

            return result;
        }

        private Dictionary<InterviewItemId, object> CreateVariableValues(InterviewData interview)
        {
            var result = new Dictionary<InterviewItemId, object>();
            foreach (var interviewLevel in interview.Levels)
            {
                foreach (var variable in interviewLevel.Value.Variables)
                {
                    result.Add(new InterviewItemId(variable.Key, interviewLevel.Value.RosterVector), variable.Value);
                }
            }
            return result;
        }

        private static CommentSynchronizationDto[] GetAllComments(InterviewQuestion interviewQuestion)
        {
            return (interviewQuestion.Comments?? new List<InterviewQuestionComment>()).Select(x => new CommentSynchronizationDto
            {
                Date = x.Date,
                UserId = x.CommenterId,
                UserRole = x.CommenterRole,
                Text = x.Text
            }).ToArray();
        }

        private void FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(
            QuestionnaireRosterStructure questionnarieRosterStructure, InterviewLevel interviewLevel,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> propagatedGroupInstanceCounts)
        {
            if (interviewLevel.RosterVector.Length == 0)
                return;

            var outerVector = this.CreateOuterVector(interviewLevel);

            foreach (var scopeId in interviewLevel.ScopeVectors)
            {
                foreach (var groupId in questionnarieRosterStructure.RosterScopes[scopeId.Key].RosterIdToRosterTitleQuestionIdMap.Keys)
                {
                    var groupKey = new InterviewItemId(groupId, outerVector);

                    var rosterTitle = interviewLevel.RosterRowTitles.ContainsKey(groupId)
                        ? interviewLevel.RosterRowTitles[groupId]
                        : string.Empty;
                    this.AddPropagatedGroupToDictionary(propagatedGroupInstanceCounts, scopeId.Value, rosterTitle, interviewLevel.RosterVector.Last(), groupKey);
                }
            }
        }

        private void AddPropagatedGroupToDictionary(Dictionary<InterviewItemId, RosterSynchronizationDto[]> propagatedGroupInstanceCounts,
            int? sortIndex, string rosterTitle, decimal rosterInstanceId,
            InterviewItemId groupKey)
        {
            List<RosterSynchronizationDto> currentRosterInstances = propagatedGroupInstanceCounts.ContainsKey(groupKey) ? propagatedGroupInstanceCounts[groupKey].ToList() : new List<RosterSynchronizationDto>();

            currentRosterInstances.Add(new RosterSynchronizationDto(groupKey.Id,
                groupKey.InterviewItemRosterVector, rosterInstanceId, sortIndex, rosterTitle));

            propagatedGroupInstanceCounts[groupKey] = currentRosterInstances.ToArray();
        }

        private decimal[] CreateOuterVector(InterviewLevel interviewLevel)
        {
            var outerVector = new decimal[interviewLevel.RosterVector.Length - 1];
            for (int i = 0; i < interviewLevel.RosterVector.Length - 1; i++)
            {
                outerVector[i] = interviewLevel.RosterVector[i];
            }
            return outerVector;
        }

        private Dictionary<InterviewItemId, RosterVector[]> CreateLinkedQuestionsOptions(InterviewData interview)
        {
            var result = new Dictionary<InterviewItemId, RosterVector[]>();

            var questionnaire =
                this.questionnaireStorage.GetQuestionnaireDocument(
                    new QuestionnaireIdentity(interview.QuestionnaireId,
                        interview.QuestionnaireVersion));

            questionnaire.ConnectChildrenWithParent();

            var linkedQuestions = questionnaire.Find<IQuestion>(q => q.LinkedToRosterId.HasValue || q.LinkedToQuestionId.HasValue).ToArray();
            var interviewLinkedQuestionOptions = this.interviewLinkedQuestionOptionsStore.GetById(interview.InterviewId);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var linkedQuestionRosterScope = InterviewLevelUtils.GetRosterSizeSourcesForEntity(linkedQuestion);

                var interviewLevelsWithTheLinkedQuestion = InterviewLevelUtils.FindLevelsByScope(interview, linkedQuestionRosterScope);

                foreach (var interviewLevel in interviewLevelsWithTheLinkedQuestion)
                {
                    var linkedQuestionIdentity = new Identity(linkedQuestion.PublicKey,
                        interviewLevel.RosterVector);
                    var interviewItemId = new InterviewItemId(linkedQuestionIdentity.Id,
                        linkedQuestionIdentity.RosterVector);

                    result.Add(interviewItemId,
                        InterviewLevelUtils.GetAvailableOptionsForQuestionLinkedOnRoster(linkedQuestion,
                            linkedQuestionIdentity.RosterVector, interview, questionnaire,
                            interviewLinkedQuestionOptions).Select(l => new RosterVector(l.RosterVector)).ToArray());
                }
            }

            return result;
        }
    }
}