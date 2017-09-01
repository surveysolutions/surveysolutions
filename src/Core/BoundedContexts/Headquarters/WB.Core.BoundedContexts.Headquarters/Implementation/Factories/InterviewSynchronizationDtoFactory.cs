﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    [Obsolete("Now we use StatefulInterview.GetSynchronizationDto()")]
    internal class InterviewSynchronizationDtoFactory : IInterviewSynchronizationDtoFactory
    {
        private readonly IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore;
        private readonly IReadSideRepositoryWriter<InterviewStatuses> interviewsRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IRosterStructureService rosterStructureService;

        public InterviewSynchronizationDtoFactory(
            IReadSideRepositoryWriter<InterviewStatuses> interviewsRepository,
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore, 
            IQuestionnaireStorage questionnaireStorage,
            IRosterStructureService rosterStructureService)
        {
            if (interviewsRepository == null) throw new ArgumentNullException(nameof(interviewsRepository));
            
            this.interviewsRepository = interviewsRepository;
            this.interviewLinkedQuestionOptionsStore = interviewLinkedQuestionOptionsStore;
            this.questionnaireStorage = questionnaireStorage;
            this.rosterStructureService = rosterStructureService;
        }

        public InterviewSynchronizationDto BuildFrom(InterviewData interview, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime)
        {
            var result = this.BuildFrom(interview, interview.ResponsibleId, interview.Status, comments, rejectedDateTime, interviewerAssignedDateTime);
            return result;
        }

        public InterviewSynchronizationDto BuildFrom(InterviewData interview,
            Guid userId, 
            InterviewStatus status, 
            string comments, 
            DateTime? rejectedDateTime, 
            DateTime? interviewerAssignedDateTime)
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var disabledStaticTexts = new List<Identity>();
            var validQuestions = new HashSet<InterviewItemId>();
            var invalidQuestions = new HashSet<InterviewItemId>();
            var readonlyQuestions = new HashSet<InterviewItemId>();

            var validStaticTexts = new List<Identity>();
            var invalidStaticTexts = new List<KeyValuePair<Identity, List<FailedValidationCondition>>>();

            var propagatedGroupInstanceCounts = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>();

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(
                    new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion));

            if (questionnaire == null)
                throw new Exception("Questionnaire was not found");
            var rosterScopes = this.rosterStructureService.GetRosterScopes(questionnaire);
            
            Dictionary<Identity, IList<FailedValidationCondition>> failedValidationConditions = new Dictionary<Identity, IList<FailedValidationCondition>>();
            foreach (var interviewLevel in interview.Levels.Values)
            {
                foreach (var interviewQuestion in interviewLevel.QuestionsSearchCache)
                {
                    var answeredQuestion = new AnsweredQuestionSynchronizationDto(interviewQuestion.Key,
                        interviewLevel.RosterVector,
                        interviewQuestion.Value.Answer,
                        GetAllComments(interviewQuestion.Value));

                    if (answeredQuestion.IsAnswered() || answeredQuestion.HasComments())
                    {
                        answeredQuestions.Add(answeredQuestion);
                    }

                    if (interviewQuestion.Value.IsDisabled())
                    {
                        disabledQuestions.Add(new InterviewItemId(interviewQuestion.Value.Id, interviewLevel.RosterVector));
                    }
                    else
                    {
                        if (answeredQuestion.IsAnswered() && interviewQuestion.Value.IsInvalid())
                        {
                            invalidQuestions.Add(new InterviewItemId(interviewQuestion.Key, interviewLevel.RosterVector));
                            failedValidationConditions.Add(new Identity(interviewQuestion.Key, interviewLevel.RosterVector),
                                interviewQuestion.Value.FailedValidationConditions.ToList());
                        }
                        if (!interviewQuestion.Value.IsInvalid())
                        {
                            validQuestions.Add(new InterviewItemId(interviewQuestion.Key, interviewLevel.RosterVector));
                        }
                    }

                    if (interviewQuestion.Value.IsReadonly())
                    {
                        readonlyQuestions.Add(new InterviewItemId(interviewQuestion.Key, interviewLevel.RosterVector));
                    }
                }
                foreach (var disabledGroup in interviewLevel.DisabledGroups)
                {
                    disabledGroups.Add(new InterviewItemId(disabledGroup, interviewLevel.RosterVector));
                }
                foreach (var staticText in interviewLevel.StaticTexts.Values)
                {
                    var staticTextIdentity = new Identity(staticText.Id, interviewLevel.RosterVector);
                    if (staticText.IsEnabled)
                    {
                        if (staticText.IsInvalid)
                        {
                            invalidStaticTexts.Add(new KeyValuePair<Identity, List<FailedValidationCondition>>(
                                staticTextIdentity, staticText.FailedValidationConditions.ToList()));
                        }
                        
                    }
                    else
                    {
                        disabledStaticTexts.Add(staticTextIdentity);
                    }
                }

                this.FillPropagatedGroupInstancesOfCurrentLevelForQuestionnarie(rosterScopes, interviewLevel, propagatedGroupInstanceCounts);
            }

            if (!interviewerAssignedDateTime.HasValue)
            {
                var interviewStatusChangeHistory = this.interviewsRepository.GetById(new ChangeStatusInputModel {InterviewId = interview.InterviewId}.InterviewId);

                var commentedStatusHistoryViews = interviewStatusChangeHistory?
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

                if (commentedStatusHistoryViews != null)
                {
                    var interviewerAssignedStatus =
                        commentedStatusHistoryViews.OrderBy(interviewStatus => interviewStatus.Date).LastOrDefault(
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

            var result = new InterviewSynchronizationDto(interview.InterviewId,
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
                readonlyQuestions,
                validStaticTexts,
                invalidStaticTexts,
                propagatedGroupInstanceCounts,
                failedValidationConditions.ToList(),
                linkedQuestionOptions,
                variableValues,
                disabledVariables,
                interview.WasCompleted,
                interview.CreatedOnClient);
            result.InterviewKey = string.IsNullOrEmpty(interview.InterviewKey) ? null : InterviewKey.Parse(interview.InterviewKey);
            result.AssignmentId = interview.AssignmentId;
            return result;
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
            Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterScopes, InterviewLevel interviewLevel,
            Dictionary<InterviewItemId, RosterSynchronizationDto[]> propagatedGroupInstanceCounts)
        {
            if (interviewLevel.RosterVector.Length == 0)
                return;

            var outerVector = this.CreateOuterVector(interviewLevel);

            foreach (var scopeId in interviewLevel.ScopeVectors)
            {
                foreach (var groupId in rosterScopes[scopeId.Key].RosterIdToRosterTitleQuestionIdMap.Keys)
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

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(
                new QuestionnaireIdentity(interview.QuestionnaireId,
                interview.QuestionnaireVersion));

            var linkedQuestions = questionnaire.Find<IQuestion>(q => q.LinkedToRosterId.HasValue).ToList();

            foreach (var question in questionnaire.Find<IQuestion>(q => q.LinkedToQuestionId.HasValue))
            {
                if(questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == question.LinkedToQuestionId.Value).QuestionType != QuestionType.TextList)
                    linkedQuestions.Add(question);
            }

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