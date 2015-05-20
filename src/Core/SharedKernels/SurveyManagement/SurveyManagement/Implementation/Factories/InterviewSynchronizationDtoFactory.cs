using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class InterviewSynchronizationDtoFactory : IInterviewSynchronizationDtoFactory
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures;
        public InterviewSynchronizationDtoFactory(IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures)
        {
            if (questionnriePropagationStructures == null) throw new ArgumentNullException("questionnriePropagationStructures");
            this.questionnriePropagationStructures = questionnriePropagationStructures;
        }

        public InterviewSynchronizationDto BuildFrom(InterviewData interview, string comments)
        {
            var result = BuildFrom(interview, interview.ResponsibleId, interview.Status, comments);
            return result;
        }

        private string GetLastComment(InterviewQuestion question)
        {
            if (question.Comments == null || !question.Comments.Any())
                return null;
            return question.Comments.Last().Text;
        }
        public InterviewSynchronizationDto BuildFrom(InterviewData interview, Guid userId, InterviewStatus status, string comments)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();
            var propagatedGroupInstanceCounts = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();

            var questionnariePropagationStructure = this.questionnriePropagationStructures.AsVersioned().Get(interview.QuestionnaireId.FormatGuid(),
                interview.QuestionnaireVersion);

            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.GetAllQuestions())
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Id,
                        interviewLevel.RosterVector,
                        interviewQuestion.Answer, GetLastComment(interviewQuestion));

                    FillAllComments(answeredQuestion, interviewQuestion);

                    if (!answeredQuestion.IsEmpty())
                    {
                        answeredQuestions.Add(answeredQuestion);
                    }
                    if (interviewQuestion.IsDisabled())
                    {
                        disabledQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                    }

                    if (interviewQuestion.IsInvalid())
                    {
                        invalidQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                    }
                    if (!interviewQuestion.IsInvalid())
                    {
                        validQuestions.Add(new InterviewItemId(interviewQuestion.Id, interviewLevel.RosterVector));
                    }
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new InterviewItemId(disabledGroup, interviewLevel.RosterVector));
                }

                this.FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(questionnariePropagationStructure, interviewLevel,
                    propagatedGroupInstanceCounts);
            }
            return new InterviewSynchronizationDto(interview.InterviewId,
                status, comments,
                userId,
                interview.QuestionnaireId,
                interview.QuestionnaireVersion,
                answeredQuestions.ToArray(),
                disabledGroups,
                disabledQuestions,
                validQuestions,
                invalidQuestions,
                null,
                propagatedGroupInstanceCounts,
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
                groupKey.InterviewItemPropagationVector, rosterInstanceId, sortIndex, rosterTitle));

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