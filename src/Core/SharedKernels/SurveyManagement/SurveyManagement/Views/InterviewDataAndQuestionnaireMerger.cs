using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class InterviewDataAndQuestionnaireMerger : IInterviewDataAndQuestionnaireMerger
    {
        protected static ISubstitutionService SubstitutionService
        {
            get { return ServiceLocator.Current.GetInstance<ISubstitutionService>(); }
        }

        public InterviewDetailsView Merge(InterviewData interview, 
            QuestionnaireDocumentVersioned questionnaire, 
            ReferenceInfoForLinkedQuestions questionnaireReferenceInfo,
            QuestionnaireRosterStructure questionnaireRosters,
            UserDocument user)
        {
            questionnaire.Questionnaire.ConnectChildrenWithParent();

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
            return (currentGroup.IsRoster);
        }

        private InterviewLevel GetRootLevel(InterviewData interview)
        {
            return interview.Levels.FirstOrDefault(w => w.Value.ScopeVectors.Count==1 && w.Value.ScopeVectors.ContainsKey(new ValueVector<Guid>())).Value;
        }

        private IEnumerable<KeyValuePair<string, InterviewLevel>> GetRosterLevels(Guid groupId, InterviewData interviewData,
            QuestionnaireRosterStructure questionnaireRoster)
        {
        //    Guid[] rosterScope = null;
            //totally not efficient
            foreach (var scopeId in questionnaireRoster.RosterScopes.Keys)
            {
                foreach (var trigger in questionnaireRoster.RosterScopes[scopeId].RosterIdToRosterTitleQuestionIdMap.Keys)
                {
                    if (trigger == groupId)
                    {
                      //  rosterScope = scopeId;


                        return interviewData.Levels.Where(w => w.Value.ScopeVectors.ContainsKey(scopeId));
                   //     break;
                    }
                }
            }
           // if (rosterScope==null)
                throw new ArgumentException(string.Format("group {0} is missing in any roster scope of questionnaire", groupId));


        }

        private IEnumerable<InterviewGroupView> GetCompletedRosterGroups(IGroup currentGroup, int depth, InterviewLevel interviewLevel,
            IEnumerable<InterviewLevel> upperInterviewLevels,
            Dictionary<Guid, string> idToVariableMap, Dictionary<string, Guid> variableToIdMap,
            Func<Guid, decimal[], Dictionary<decimal[], string>> getAvailableOptions,
            QuestionnaireDocument questionnaire, InterviewData interview,
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
            QuestionnaireDocument questionnaire)
        {
            Guid nearestParentRosterId = GetNearestParentRosterId(questionnaire, currentGroup.PublicKey);
            var rosterTitleFromLevel = interviewLevel.RosterRowTitles.ContainsKey(nearestParentRosterId)
                ? interviewLevel.RosterRowTitles[nearestParentRosterId]
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
            foreach (var entity in currentGroup.Children)
            {
                if (entity is IGroup) continue;

                InterviewEntityView interviewEntity = null;

                var question = entity as IQuestion;
                if (question != null)
                {
                    var answeredQuestion = interviewLevel.QuestionsSearchCahche.ContainsKey(question.PublicKey) ? interviewLevel.QuestionsSearchCahche[question.PublicKey] : null;
                    
                    Dictionary<string, string> answersForTitleSubstitution =
                        GetAnswersForTitleSubstitution(question, variableToIdMap, interviewLevel, upperInterviewLevels, questionnaire, (questionId) => getAvailableOptions(questionId, interviewLevel.RosterVector), rosterTitleFromLevel);

                    bool isQustionsParentGroupDisabled = interviewLevel.DisabledGroups != null &&
                        IsQuestionParentGroupDisabled(disabledGroups, currentGroup);

                    interviewEntity = question.LinkedToQuestionId.HasValue
                        ? new InterviewLinkedQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution, (questionId) => getAvailableOptions(questionId, interviewLevel.RosterVector), isQustionsParentGroupDisabled, interviewLevel.RosterVector)
                        : new InterviewQuestionView(question, answeredQuestion, idToVariableMap, answersForTitleSubstitution, isQustionsParentGroupDisabled, interviewLevel.RosterVector);
    
                }

                var staticText = entity as IStaticText;
                if (staticText != null)
                {
                    interviewEntity = new InterviewStaticTextView(staticText);
                }
                
                completedGroup.Entities.Add(interviewEntity);
            }

            return completedGroup;
        }

        private Guid GetNearestParentRosterId(QuestionnaireDocument questionnaire, Guid groupId)
        {
            var currentGroup = questionnaire.Find<IGroup>(groupId);
            while (currentGroup != null)
            {
                if (currentGroup.IsRoster)
                    break;
                currentGroup = (IGroup) currentGroup.GetParent();
            }
            return currentGroup == null ? Guid.Empty : currentGroup.PublicKey;
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
            if (variableName == SubstitutionService.RosterTitleSubstitutionReference)
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

            if (interviewQuestion.IsDisabled())
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

                        var selectedAnswer =
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
                ?? upperInterviewLevels.Select(level => GetQuestion(questionId, level)).FirstOrDefault(q => q != null);
        }

        private static InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel)
        {
            return currentInterviewLevel.QuestionsSearchCahche.ContainsKey(questionId)? currentInterviewLevel.QuestionsSearchCahche[questionId] : null;
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
                interviewLevel => interviewLevel.QuestionsSearchCahche.ContainsKey(optionsSource.ReferencedQuestionId)?interviewLevel.QuestionsSearchCahche[optionsSource.ReferencedQuestionId] : null);

            return allLinkedQuestions.Where(question => question.Value != null && !question.Value.IsDisabled() && question.Value.Answer != null)
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
                        level.ScopeVectors.ContainsKey(optionsSource.ReferencedQuestionRosterScope) &&
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