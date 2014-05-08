﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class InterviewDataAndQuestionnaireMerger : IInterviewDataAndQuestionnaireMerger
    {
        public InterviewDetailsView Merge(InterviewData interview, 
            QuestionnaireDocumentVersioned questionnaire, 
            ReferenceInfoForLinkedQuestions questionnaireReferenceInfo,
            QuestionnaireRosterStructure questionnaireRosters,
            UserDocument user)
        {
            Dictionary<Guid, string> idToVariableMap = questionnaire.Questionnaire.GetAllQuestions().ToDictionary(
                x => x.PublicKey,
                x => x.StataExportCaption);

            Dictionary<string, Guid> variableToIdMap = questionnaire.Questionnaire.GetAllQuestions().ToDictionary(
                x => x.StataExportCaption,
                x => x.PublicKey);

            var interviewDetails = new InterviewDetailsView()
            {
                Responsible = new UserLight(interview.ResponsibleId, user.UserName),
                QuestionnairePublicKey = interview.QuestionnaireId,
                Title = questionnaire.Questionnaire.Title,
                Description = questionnaire.Questionnaire.Description,
                PublicKey = interview.InterviewId,
                Status = interview.Status
            };

            Func<Guid, decimal[], Dictionary<decimal[], string>> getAvailableOptions = (questionId, questionRosterVector) => this.GetAvailableOptions(questionId, questionRosterVector, interview, questionnaireReferenceInfo, questionnaireRosters);
            var groupStack = new Stack<KeyValuePair<IGroup, int>>();

            groupStack.Push(new KeyValuePair<IGroup, int>(questionnaire.Questionnaire, 0));
            while (groupStack.Count > 0)
            {
                var currentGroup = groupStack.Pop();

                var rootLevel = this.GetRootLevel(interview);
                
                if (IsRoster(currentGroup.Key))
                {
                    var rosterLevels = this.GetRosterLevels(currentGroup.Key.PublicKey, interview, questionnaireRosters).ToList();

                    if (rosterLevels.Any())
                    {
                        //we have at least one completed roster group
                        //so for every layer we are creating roster group
                        foreach (var rosterGroup in rosterLevels)
                        {
                            var completedRosterGroups =
                                this.GetCompletedRosterGroups(currentGroup.Key, currentGroup.Value,
                                    rosterGroup.Value, new[] {rootLevel},
                                    idToVariableMap, variableToIdMap, getAvailableOptions, questionnaire.Questionnaire,
                                    interview, questionnaireRosters);

                            interviewDetails.Groups.AddRange(completedRosterGroups);
                        }
                    }
                }
                else
                {
                    interviewDetails.Groups.Add(
                        this.GetCompletedGroup(currentGroup.Key, currentGroup.Value,
                            rootLevel, new InterviewLevel[] { },
                            idToVariableMap, variableToIdMap, getAvailableOptions, questionnaire.Questionnaire));

                    foreach (var group in currentGroup.Key.Children.OfType<IGroup>().Reverse())
                    {
                        group.SetParent(currentGroup.Key);
                        groupStack.Push(new KeyValuePair<IGroup, int>(group, currentGroup.Value + 1));
                    }
                }
            }

            return interviewDetails;
        }

        private static bool IsRoster(IGroup currentGroup)
        {
                    //### old questionnaires supporting
            return currentGroup.Propagated == Propagate.AutoPropagated ||
                   //### roster
                   (currentGroup.IsRoster);
        }

        private InterviewLevel GetRootLevel(InterviewData interview)
        {
            return interview.Levels.FirstOrDefault(w => w.Value.ScopeIds.Count==1 && w.Value.ScopeIds.ContainsKey(interview.InterviewId)).Value;
        }

        private IEnumerable<KeyValuePair<string, InterviewLevel>> GetRosterLevels(Guid groupId, InterviewData interviewData,
            QuestionnaireRosterStructure questionnaireRoster)
        {
            Guid? rosterScope = null;
            //totally not efficient
            foreach (var scopeId in questionnaireRoster.RosterScopes.Keys)
            {
                foreach (var trigger in questionnaireRoster.RosterScopes[scopeId].RosterIdToRosterTitleQuestionIdMap.Keys)
                {
                    if (trigger == groupId)
                    {
                        rosterScope = scopeId;
                        break;
                    }
                }
            }
            if (!rosterScope.HasValue)
                throw new ArgumentException(string.Format("group {0} is missing in any roster scope of questionnaire", groupId));


            return interviewData.Levels.Where(w => w.Value.ScopeIds.ContainsKey(rosterScope.Value));
        }

        private IEnumerable<InterviewGroupView> GetCompletedRosterGroups(IGroup currentGroup, int depth, InterviewLevel interviewLevel,
            IEnumerable<InterviewLevel> upperInterviewLevels,
            Dictionary<Guid, string> idToVariableMap, Dictionary<string, Guid> variableToIdMap,
            Func<Guid, decimal[], Dictionary<decimal[], string>> getAvailableOptions,
            IQuestionnaireDocument questionnaire, InterviewData interview,
            QuestionnaireRosterStructure questionnaireRosters)
        {
            var result = new List<InterviewGroupView>();
            result.Add(this.GetCompletedGroup(currentGroup, depth, interviewLevel, upperInterviewLevels, idToVariableMap, variableToIdMap,
                getAvailableOptions, questionnaire));

            foreach (var nestedGroup in currentGroup.Children.OfType<IGroup>())
            {
                //nested roster supporting
                if (IsRoster(nestedGroup))
                {
                    var rosterLevels =
                        this.GetRosterLevels(nestedGroup.PublicKey, interview, questionnaireRosters)
                            .Where(
                                kv =>
                                    kv.Value.RosterVector.Take(kv.Value.RosterVector.Length - 1)
                                        .SequenceEqual(interviewLevel.RosterVector))
                            .ToList();

                    if (rosterLevels.Any())
                    {
                        foreach (var rosterGroup in rosterLevels)
                        {
                            var completedRosterGroups =
                                this.GetCompletedRosterGroups(nestedGroup, depth + 1,
                                    rosterGroup.Value, upperInterviewLevels.Union(new[] {interviewLevel}),
                                    idToVariableMap, variableToIdMap, getAvailableOptions, questionnaire, interview,
                                    questionnaireRosters);

                            result.AddRange(completedRosterGroups);
                        }
                    }
                }
                else
                {
                    result.AddRange(GetCompletedRosterGroups(nestedGroup, depth + 1, interviewLevel, upperInterviewLevels, idToVariableMap,
                        variableToIdMap,
                        getAvailableOptions, questionnaire, interview, questionnaireRosters));    
                }
                
            }
            return result;
        }

        private InterviewGroupView GetCompletedGroup(IGroup currentGroup, int depth, InterviewLevel interviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels,
            Dictionary<Guid, string> idToVariableMap, Dictionary<string, Guid> variableToIdMap, Func<Guid, decimal[], Dictionary<decimal[], string>> getAvailableOptions,
            IQuestionnaireDocument questionnaire)
        {
            var rosterTitleFromLevel = interviewLevel.RosterRowTitles.ContainsKey(currentGroup.PublicKey)
                ? interviewLevel.RosterRowTitles[currentGroup.PublicKey]
                : null;

            var rosterTitle = !string.IsNullOrEmpty(rosterTitleFromLevel)
                ? string.Format("{0}: {1}", currentGroup.Title, rosterTitleFromLevel)
                : currentGroup.Title;
            var completedGroup = new InterviewGroupView(currentGroup.PublicKey)
            {
                Depth = depth,
                Title = rosterTitle,
                RosterVector = interviewLevel.RosterVector,
                ParentId = currentGroup.GetParent() != null ? currentGroup.GetParent().PublicKey : (Guid?) null
            };
            var disabledGroups = interviewLevel.DisabledGroups.Union(upperInterviewLevels.SelectMany(upper => upper.DisabledGroups));
            foreach (var question in currentGroup.Children.OfType<IQuestion>())
            {
                InterviewQuestion answeredQuestion = interviewLevel.GetQuestion(question.PublicKey);

                Dictionary<string, string> answersForTitleSubstitution =
                    GetAnswersForTitleSubstitution(question, variableToIdMap, interviewLevel, upperInterviewLevels, questionnaire, (questionId) => getAvailableOptions(questionId, interviewLevel.RosterVector), rosterTitleFromLevel);

                bool isQustionsParentGroupDisabled = interviewLevel.DisabledGroups != null &&
                    IsQuestionParentGroupDisabled(disabledGroups, currentGroup);

                var interviewQuestion = question.LinkedToQuestionId.HasValue
                    ? new InterviewLinkedQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution, (questionId) => getAvailableOptions(questionId, interviewLevel.RosterVector), isQustionsParentGroupDisabled)
                    : new InterviewQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution, isQustionsParentGroupDisabled);

                completedGroup.Questions.Add(interviewQuestion);
            }

            return completedGroup;
        }

        private static bool IsQuestionParentGroupDisabled(IEnumerable<Guid> disabledGroups, IGroup group)
        {
            if (disabledGroups.Contains(group.PublicKey))
                return true;

            var parent = group.GetParent() as IGroup;

            if (parent == null || parent is QuestionnaireDocument)
                return false;
            return IsQuestionParentGroupDisabled(disabledGroups, parent);
        }

        private static Dictionary<string, string> GetAnswersForTitleSubstitution(IQuestion question, Dictionary<string, Guid> variableToIdMap,
            InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels, IQuestionnaireDocument questionnaire,
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions, string rosterTitle)
        {
            return question
                .GetVariablesUsedInTitle()
                .Select(variableName => new
                {
                    Variable = variableName,
                    Answer = GetAnswerForTitleSubstitution(variableName, variableToIdMap, currentInterviewLevel, upperInterviewLevels, questionnaire, getAvailableOptions, rosterTitle),
                })
                .Where(x => x.Answer != null)
                .ToDictionary(
                    x => x.Variable,
                    x => x.Answer);
        }

        private static string GetAnswerForTitleSubstitution(string variableName, Dictionary<string, Guid> variableToIdMap,
            InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels, IQuestionnaireDocument questionnaire,
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions, string rosterTitle)
        {
            if (variableName == SubstitutionUtils.RosterTitleSubstitutionReference)
            {
                if (string.IsNullOrEmpty(rosterTitle))
                    return null;
                return rosterTitle;
            }

            if (!variableToIdMap.ContainsKey(variableName))
                return null;

            Guid questionId = variableToIdMap[variableName];

            InterviewQuestion interviewQuestion = GetQuestion(questionId, currentInterviewLevel, upperInterviewLevels);

            if (interviewQuestion == null)
                return null;

            if (!interviewQuestion.Enabled)
                return null;

            return GetFormattedAnswerForTitleSubstitution(interviewQuestion, questionnaire, getAvailableOptions);
        }

        private static string GetFormattedAnswerForTitleSubstitution(InterviewQuestion interviewQuestion, IQuestionnaireDocument questionnaire,
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions)
        {
            if (interviewQuestion.Answer == null)
                return null;

            var question = questionnaire.Find<IQuestion>(interviewQuestion.Id);

            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    return (string) interviewQuestion.Answer;

                case QuestionType.Numeric:
                    return Convert.ToDecimal(interviewQuestion.Answer).ToString(CultureInfo.InvariantCulture);

                case QuestionType.DateTime:
                    DateTime dateTime = DateTime.Parse(Convert.ToString(interviewQuestion.Answer));
                    return dateTime.ToString("M/d/yyyy");

                case QuestionType.SingleOption:
                    if (!question.LinkedToQuestionId.HasValue)
                    {
                        decimal selectedValue = Convert.ToDecimal(interviewQuestion.Answer);

                        IAnswer selectedAnswer =
                            question.Answers.SingleOrDefault(option => Convert.ToDecimal(option.AnswerValue) == selectedValue);

                        return selectedAnswer != null ? selectedAnswer.AnswerText : null;
                    }
                    else
                    {
                        decimal[] selectedRosterVector = ((IEnumerable)interviewQuestion.Answer).OfType<decimal>().ToArray();

                        Dictionary<decimal[], string> availableOptions = getAvailableOptions(interviewQuestion.Id);

                        KeyValuePair<decimal[], string> selectedOption = availableOptions.SingleOrDefault(option => option.Key.SequenceEqual(selectedRosterVector));

                        return selectedOption.Value;
                    }

                default:
                    return null;
            }
        }

        private static InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels)
        {
            return GetQuestion(questionId, currentInterviewLevel)
                ?? upperInterviewLevels.Select(level => GetQuestion(questionId, level)).FirstOrDefault();
        }

        private static InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel)
        {
            return currentInterviewLevel.GetQuestion(questionId);
        }

        private Dictionary<decimal[], string> GetAvailableOptions(Guid questionId, decimal[] questionRosterVector, InterviewData interview,
            ReferenceInfoForLinkedQuestions referenceQuestions, QuestionnaireRosterStructure questionnaireRosters)
        {
            var optionsSource = this.GetQuestionReferencedQuestion(questionId, referenceQuestions);
            if (optionsSource == null)
                return this.EmptyOptions;

            IEnumerable<InterviewLevel> allAvailableLevelsByScope = this.GetAllAvailableLevelsByScope(interview, questionRosterVector, optionsSource);

            IDictionary<decimal[], InterviewQuestion> allLinkedQuestions =
                allAvailableLevelsByScope.ToDictionary(interviewLevel => interviewLevel.RosterVector,
                    interviewLevel => interviewLevel.GetQuestion(optionsSource.ReferencedQuestionId));

            return allLinkedQuestions.Where(question => question.Value != null && question.Value.Enabled && question.Value.Answer != null)
                .ToDictionary(question => question.Key,
                    question =>
                        CreateLinkedQuestionOption(question.Value, question.Key, questionRosterVector, optionsSource, questionnaireRosters,
                            interview));
        }

        private IEnumerable<InterviewLevel> GetAllAvailableLevelsByScope(InterviewData interview, decimal[] questionRosterVector, ReferenceInfoByQuestion optionsSource)
        {
            return
                interview.Levels.Values.Where(
                    level =>
                        level.ScopeIds.ContainsKey(optionsSource.ReferencedQuestionRosterScope.Last()) &&
                            LinkedQuestionUtils.IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(level.RosterVector, optionsSource.ReferencedQuestionRosterScope,questionRosterVector,optionsSource.LinkedQuestionRosterScope));
        }

        private string CreateLinkedQuestionOption(InterviewQuestion referencedQuestion, decimal[] referencedQuestionRosterVector, decimal[] linkedQuestionRosterVector,
            ReferenceInfoByQuestion optionsSource, QuestionnaireRosterStructure questionnaireRosters, InterviewData interview)
        {
            return LinkedQuestionUtils.BuildLinkedQuestionOptionTitle(referencedQuestion.Answer.ToString(),
                (firstScreenInScopeId, levelRosterVector) =>
                {
                    var levelFromScope =
                        interview.Levels.Values.FirstOrDefault(level => level.RosterVector.SequenceEqual(levelRosterVector));
                    if (levelFromScope != null)
                    {
                        if (levelFromScope.RosterRowTitles.ContainsKey(firstScreenInScopeId))
                        {
                            return levelFromScope.RosterRowTitles[firstScreenInScopeId];
                        }
                    }
                    return string.Empty;
                }, referencedQuestionRosterVector, optionsSource.ReferencedQuestionRosterScope, linkedQuestionRosterVector,
                optionsSource.LinkedQuestionRosterScope, questionnaireRosters);
        }

        private ReferenceInfoByQuestion GetQuestionReferencedQuestion(Guid questionId, ReferenceInfoForLinkedQuestions referenceQuestions)
        {
            if (!referenceQuestions.ReferencesOnLinkedQuestions.ContainsKey(questionId))
                return null;
            return referenceQuestions.ReferencesOnLinkedQuestions[questionId];
        }

        private Dictionary<decimal[], string> EmptyOptions
        {
            get { return new Dictionary<decimal[], string>(); }
        }
    }
}