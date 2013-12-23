﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core.Supervisor.Views.Interview;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views
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
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions = (questionId) => this.GetAvailableOptions(questionId, interview, questionnaireReferenceInfo);
            var groupStack = new Stack<KeyValuePair<IGroup, int>>();

            groupStack.Push(new KeyValuePair<IGroup, int>(questionnaire.Questionnaire, 0));
            while (groupStack.Count > 0)
            {
                var currentGroup = groupStack.Pop();

                var rootLevel = this.GetRootLevel(interview);
                //### old questionnaires supporting
                if (currentGroup.Key.Propagated == Propagate.AutoPropagated ||
                    //### roster
                    (currentGroup.Key.IsRoster))
                {
                    var rosterGroups = this.GetRosterLevels(currentGroup.Key.PublicKey, interview, questionnaireRosters).ToList();

                    if (rosterGroups.Any())
                    {
                        //we have at least one completed roster group
                        //so for every layer we are creating roster group
                        foreach (var rosterGroup in rosterGroups)
                        {
                            var completedRosterGroup =
                                this.GetCompletedGroup(currentGroup.Key, currentGroup.Value,
                                    rosterGroup.Value, new[] { rootLevel },
                                    idToVariableMap, variableToIdMap, getAvailableOptions, questionnaire.Questionnaire);

                            interviewDetails.Groups.Add(completedRosterGroup);
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
                foreach (var trigger in questionnaireRoster.RosterScopes[scopeId].RosterIdMappedOfRosterTitleQuestionId.Keys)
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


        private InterviewGroupView GetCompletedGroup(IGroup currentGroup, int depth, InterviewLevel interviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels,
            Dictionary<Guid, string> idToVariableMap, Dictionary<string, Guid> variableToIdMap, Func<Guid, Dictionary<decimal[], string>> getAvailableOptions,
            IQuestionnaireDocument questionnaire)
        {
            var completedGroup = new InterviewGroupView(currentGroup.PublicKey)
            {
                Depth = depth,
                Title = currentGroup.Title,
                RosterVector = interviewLevel.RosterVector,
                ParentId = currentGroup.GetParent() != null ? currentGroup.GetParent().PublicKey : (Guid?) null
            };

            foreach (var question in currentGroup.Children.OfType<IQuestion>())
            {
                InterviewQuestion answeredQuestion = interviewLevel.GetQuestion(question.PublicKey);

                Dictionary<string, string> answersForTitleSubstitution =
                    GetAnswersForTitleSubstitution(question, variableToIdMap, interviewLevel, upperInterviewLevels, questionnaire, getAvailableOptions);

                bool isQustionsParentGroupDisabled = interviewLevel.DisabledGroups != null && interviewLevel.DisabledGroups.Contains(currentGroup.PublicKey);

                var interviewQuestion = question.LinkedToQuestionId.HasValue
                    ? new InterviewLinkedQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution, getAvailableOptions, isQustionsParentGroupDisabled)
                    : new InterviewQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution, isQustionsParentGroupDisabled);

                completedGroup.Questions.Add(interviewQuestion);
            }

            return completedGroup;
        }

        private static Dictionary<string, string> GetAnswersForTitleSubstitution(IQuestion question, Dictionary<string, Guid> variableToIdMap,
            InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels, IQuestionnaireDocument questionnaire,
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions)
        {
            return question
                .GetVariablesUsedInTitle()
                .Select(variableName => new
                {
                    Variable = variableName,
                    Answer = GetAnswerForTitleSubstitution(variableName, variableToIdMap, currentInterviewLevel, upperInterviewLevels, questionnaire, getAvailableOptions),
                })
                .Where(x => x.Answer != null)
                .ToDictionary(
                    x => x.Variable,
                    x => x.Answer);
        }

        private static string GetAnswerForTitleSubstitution(string variableName, Dictionary<string, Guid> variableToIdMap,
            InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels, IQuestionnaireDocument questionnaire,
            Func<Guid, Dictionary<decimal[], string>> getAvailableOptions)
        {
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

        private Dictionary<decimal[], string> GetAvailableOptions(Guid questionId, InterviewData interview,
            ReferenceInfoForLinkedQuestions referenceQuestions)
        {
            var optionsSource = this.GetQuestionReferencedQuestion(questionId, referenceQuestions);
            if (optionsSource == null)
                return this.EmptyOptions;

            IEnumerable<InterviewLevel> allAvailableLevelsByScope = this.GetAllAvailableLevelsByScope(interview, optionsSource);

            IDictionary<decimal[], InterviewQuestion> allLinkedQuestions =
                allAvailableLevelsByScope.ToDictionary(interviewLevel => interviewLevel.RosterVector,
                    interviewLevel => interviewLevel.GetQuestion(optionsSource.ReferencedQuestionId));

            return allLinkedQuestions.Where(question => question.Value != null && question.Value.Enabled)
                .ToDictionary(question => question.Key, question => (question.Value.Answer ?? string.Empty).ToString());
        }

        private IEnumerable<InterviewLevel> GetAllAvailableLevelsByScope(InterviewData interview, ReferenceInfoByQuestion optionsSource)
        {
            return interview.Levels.Values.Where(level => level.ScopeIds.ContainsKey(optionsSource.ScopeId));
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