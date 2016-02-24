using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class InterviewSynchronizationDtoFactory : IInterviewSynchronizationDtoFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> interviewsRepository;

        public InterviewSynchronizationDtoFactory(
            IReadSideRepositoryWriter<InterviewStatuses> interviewsRepository,
            IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage)
        {
            if (interviewsRepository == null) throw new ArgumentNullException(nameof(interviewsRepository));
            
            this.interviewsRepository = interviewsRepository;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
        }

        public InterviewSynchronizationDto BuildFrom(InterviewData interview, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime)
        {
            var result = BuildFrom(interview, interview.ResponsibleId, interview.Status, comments, rejectedDateTime, interviewerAssignedDateTime);
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
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();
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
                        interviewQuestion.Value.Answer, GetLastComment(interviewQuestion.Value));

                    FillAllComments(answeredQuestion, interviewQuestion.Value);

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

            return new InterviewSynchronizationDto(interview.InterviewId,
                status, 
                comments,
                rejectedDateTime,
                interviewerAssignedDateTime,
                userId,
                interview.QuestionnaireId,
                interview.QuestionnaireVersion,
                answeredQuestions.ToArray(),
                disabledGroups,
                disabledQuestions,
                validQuestions,
                invalidQuestions,
                propagatedGroupInstanceCounts,
                failedValidationConditions.ToList(),
                interview.WasCompleted,
                interview.CreatedOnClient);
        }

        private static void FillAllComments(AnsweredQuestionSynchronizationDto answeredQuestion, InterviewQuestion interviewQuestion)
        {
            answeredQuestion.AllComments = (interviewQuestion.Comments?? new List<InterviewQuestionComment>()).Select(x => new CommentSynchronizationDto
            {
                Date = x.Date,
                UserId = x.CommenterId,
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
    }
}