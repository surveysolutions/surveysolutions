using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class InterviewDataAndQuestionnaireMerger : IInterviewDataAndQuestionnaireMerger
    {
        private readonly ISubstitutionService substitutionService;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures;
        private readonly IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions;

        public InterviewDataAndQuestionnaireMerger(
            ISubstitutionService substitutionService, 
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructures,
            IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions> questionnaireReferenceInfoForLinkedQuestions)
        {
            this.substitutionService = substitutionService;
            this.questionnaireRosterStructures = questionnaireRosterStructures;
            this.questionnaireReferenceInfoForLinkedQuestions = questionnaireReferenceInfoForLinkedQuestions;
        }

        private class InterviewInfoInternal
        {
            public InterviewData Interview { get; }
            public IQuestionnaireDocument Questionnaire { get; }
            public QuestionnaireRosterStructure Rosters { get; }
            public ReferenceInfoForLinkedQuestions LinkedQuestionReferences { get; }
            public Dictionary<Guid, string> QuestionIdToVariable { get; }
            public Dictionary<string, Guid> VariableToQuestionId { get; }

            public InterviewInfoInternal(
                InterviewData interview,
                IQuestionnaireDocument questionnaire,
                QuestionnaireRosterStructure rosters,
                ReferenceInfoForLinkedQuestions linkedQuestionReferences,
                Dictionary<Guid, string> questionIdToVariable,
                Dictionary<string, Guid> variableToQuestionId)
            {
                this.Interview = interview;
                this.Questionnaire = questionnaire;
                this.Rosters = rosters;
                this.LinkedQuestionReferences = linkedQuestionReferences;
                this.QuestionIdToVariable = questionIdToVariable;
                this.VariableToQuestionId = variableToQuestionId;
            }
        }

        public InterviewDetailsView Merge(InterviewData interview, IQuestionnaireDocument questionnaire, UserLight responsible)
        {
            var questionnaireId = new QuestionnaireIdentity(interview.QuestionnaireId, interview.QuestionnaireVersion).ToString();
            
            questionnaire.ConnectChildrenWithParent();
            
            var interviewInfo = new InterviewInfoInternal(
                interview: interview,
                questionnaire: questionnaire,
                rosters: this.questionnaireRosterStructures.GetById(questionnaireId),
                linkedQuestionReferences: this.questionnaireReferenceInfoForLinkedQuestions.GetById(questionnaireId),
                questionIdToVariable: questionnaire.GetAllQuestions().ToDictionary(x => x.PublicKey,x => x.StataExportCaption),
                variableToQuestionId: questionnaire.GetAllQuestions().ToDictionary(x => x.StataExportCaption, x => x.PublicKey));

            var interviewGroups = new List<InterviewGroupView>();
            var groupStack = new Stack<KeyValuePair<IGroup, int>>();

            groupStack.Push(new KeyValuePair<IGroup, int>(questionnaire, 0));
            while (groupStack.Count > 0)
            {
                var currentGroup = groupStack.Pop();

                var rootLevel = this.GetRootLevel(interview);
                
                if (IsRoster(currentGroup.Key))
                {
                    var rosterLevels = this.GetRosterLevels(currentGroup.Key.PublicKey, interviewInfo).ToList();

                    if (rosterLevels.Any())
                    {
                        //we have at least one completed roster group
                        //so for every layer we are creating roster group
                        foreach (var rosterGroup in rosterLevels)
                        {
                            var completedRosterGroups = this.GetCompletedRosterGroups(
                                currentGroup: currentGroup.Key,
                                depth: currentGroup.Value,
                                interviewLevel: rosterGroup.Value,
                                upperInterviewLevels: new List<InterviewLevel>() {rootLevel},
                                interviewInfo: interviewInfo);

                            interviewGroups.AddRange(completedRosterGroups);
                        }
                    }
                }
                else
                {
                    interviewGroups.Add(this.GetCompletedGroup(
                            currentGroup: currentGroup.Key,
                            depth:  currentGroup.Value,
                            interviewLevel: rootLevel,
                            upperInterviewLevels: new List<InterviewLevel>(), 
                            interviewInfo: interviewInfo));

                    foreach (var group in currentGroup.Key.Children.OfType<IGroup>().Reverse())
                    {
                        group.SetParent(currentGroup.Key);
                        groupStack.Push(new KeyValuePair<IGroup, int>(group, currentGroup.Value + 1));
                    }
                }
            }

            return new InterviewDetailsView()
            {
                Groups = interviewGroups,
                Responsible = responsible,
                QuestionnairePublicKey = interview.QuestionnaireId,
                Title = questionnaire.Title,
                Description = questionnaire.Description,
                PublicKey = interview.InterviewId,
                Status = interview.Status,
                ReceivedByInterviewer = interview.ReceivedByInterviewer
            };
        }

        private static bool IsRoster(IGroup currentGroup)
        {
            return (currentGroup.IsRoster);
        }

        private InterviewLevel GetRootLevel(InterviewData interview)
        {
            return interview.Levels.FirstOrDefault(w => w.Value.ScopeVectors.Count==1 && w.Value.ScopeVectors.ContainsKey(new ValueVector<Guid>())).Value;
        }

        private IEnumerable<KeyValuePair<string, InterviewLevel>> GetRosterLevels(Guid groupId, InterviewInfoInternal interviewInfo)
        {
            foreach (var scopeId in interviewInfo.Rosters.RosterScopes.Keys)
            {
                if (interviewInfo.Rosters.RosterScopes[scopeId].RosterIdToRosterTitleQuestionIdMap.Keys.Any(rosterId => rosterId == groupId))
                {
                    return interviewInfo.Interview.Levels.Where(w => w.Value.ScopeVectors.ContainsKey(scopeId))
                            .OrderBy(x => x.Value.ScopeVectors.First().Value ?? x.Value.RosterVector.Last());
                }
            }
            throw new ArgumentException($"group {groupId} is missing in any roster scope of questionnaire");
        }

        private IEnumerable<InterviewGroupView> GetCompletedRosterGroups(IGroup currentGroup, int depth, InterviewLevel interviewLevel,
            List<InterviewLevel> upperInterviewLevels, InterviewInfoInternal interviewInfo)
        {
            var result = new List<InterviewGroupView>
            {
                this.GetCompletedGroup(currentGroup, depth, interviewLevel, upperInterviewLevels, interviewInfo)
            };

            foreach (var nestedGroup in currentGroup.Children.OfType<IGroup>())
            {
                //nested roster supporting
                if (IsRoster(nestedGroup))
                {
                    var rosterLevels = this.GetRosterLevels(nestedGroup.PublicKey, interviewInfo)
                        .Where(kv => kv.Value.RosterVector.Take(kv.Value.RosterVector.Length - 1)
                            .SequenceEqual(interviewLevel.RosterVector))
                        .ToList();

                    if (rosterLevels.Any())
                    {
                        foreach (var rosterGroup in rosterLevels)
                        {
                            var completedRosterGroups = this.GetCompletedRosterGroups(nestedGroup, depth + 1,
                                rosterGroup.Value, upperInterviewLevels.Union(new[] {interviewLevel}).ToList(), interviewInfo);

                            result.AddRange(completedRosterGroups);
                        }
                    }
                }
                else
                {
                    result.AddRange(GetCompletedRosterGroups(nestedGroup, depth + 1, interviewLevel, upperInterviewLevels, interviewInfo));    
                }
                
            }
            return result;
        }

        private InterviewGroupView GetCompletedGroup(
            IGroup currentGroup, 
            int depth, 
            InterviewLevel interviewLevel, 
            List<InterviewLevel> upperInterviewLevels,
            InterviewInfoInternal interviewInfo)
        {
            Guid nearestParentRosterId = GetNearestParentRosterId(interviewInfo.Questionnaire, currentGroup.PublicKey);
            var rosterTitleFromLevel = interviewLevel.RosterRowTitles.ContainsKey(nearestParentRosterId)
                ? interviewLevel.RosterRowTitles[nearestParentRosterId]
                : null;

            var rosterTitle = !string.IsNullOrEmpty(rosterTitleFromLevel)
                ? $"{currentGroup.Title}: {rosterTitleFromLevel}"
                : currentGroup.Title;
            var completedGroup = new InterviewGroupView(currentGroup.PublicKey)
            {
                Depth = depth,
                Title = rosterTitle,
                RosterVector = interviewLevel.RosterVector,
                ParentId = currentGroup.GetParent() != null ? currentGroup.GetParent().PublicKey : (Guid?) null
            };
            var disabledGroups = interviewLevel.DisabledGroups.Union(upperInterviewLevels.SelectMany(upper => upper.DisabledGroups)).ToList();
            foreach (var entity in currentGroup.Children)
            {
                if (entity is IGroup) continue;

                InterviewEntityView interviewEntity = null;

                var question = entity as IQuestion;
                if (question != null)
                {
                    var answeredQuestion = interviewLevel.QuestionsSearchCache.ContainsKey(question.PublicKey) ? interviewLevel.QuestionsSearchCache[question.PublicKey] : null;

                    var answersForTitleSubstitution = GetAnswersForTitleSubstitution(question, interviewLevel, upperInterviewLevels, rosterTitleFromLevel, interviewInfo);

                    bool isQuestionsParentGroupDisabled = interviewLevel.DisabledGroups != null && IsQuestionParentGroupDisabled(disabledGroups, currentGroup);

                    if (question.LinkedToQuestionId.HasValue)
                        interviewEntity = new InterviewLinkedQuestionView(question, answeredQuestion,
                            interviewInfo.QuestionIdToVariable,
                            answersForTitleSubstitution,
                            (questionId) => GetAvailableOptions(questionId, interviewLevel.RosterVector, interviewInfo),
                            isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                    else if (question.LinkedToRosterId.HasValue)
                        interviewEntity = new InterviewLinkedQuestionView(question, answeredQuestion,
                            interviewInfo.QuestionIdToVariable,
                            answersForTitleSubstitution,
                            (questionId) => GetAvailableOptionsForQuestionLinkedOnRoster(question, interviewLevel.RosterVector, question.LinkedToRosterId.Value, interviewInfo),
                            isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                    else
                        interviewEntity = new InterviewQuestionView(question,
                            answeredQuestion,
                            answersForTitleSubstitution, 
                            isQuestionsParentGroupDisabled, 
                            interviewLevel.RosterVector,
                            interviewInfo.Interview.Status);

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

        private Guid GetNearestParentRosterId(IQuestionnaireDocument questionnaire, Guid groupId)
        {
            var currentGroup = questionnaire.Find<IGroup>(groupId);
            while (currentGroup != null)
            {
                if (currentGroup.IsRoster)
                    break;
                currentGroup = (IGroup) currentGroup.GetParent();
            }
            return currentGroup?.PublicKey ?? Guid.Empty;
        }


        private static bool IsQuestionParentGroupDisabled(List<Guid> disabledGroups, IGroup group)
        {
            if (disabledGroups.Contains(group.PublicKey))
                return true;

            var parent = group.GetParent() as IGroup;

            if (parent == null || parent is QuestionnaireDocument)
                return false;
            return IsQuestionParentGroupDisabled(disabledGroups, parent);
        }

        private Dictionary<string, string> GetAnswersForTitleSubstitution(
            IQuestion question, 
            InterviewLevel currentInterviewLevel, 
            List<InterviewLevel> upperInterviewLevels, 
            string rosterTitle, 
            InterviewInfoInternal interviewInfo)
        {
            return question.GetVariablesUsedInTitle()
                .Select(variableName => new
                {
                    Variable = variableName,
                    Answer = this.GetAnswerForTitleSubstitution(variableName, currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo),
                })
                .Where(x => x.Answer != null)
                .ToDictionary(x => x.Variable, x => x.Answer);
        }

        private string GetAnswerForTitleSubstitution(string variableName, 
            InterviewLevel currentInterviewLevel, List<InterviewLevel> upperInterviewLevels, string rosterTitle, InterviewInfoInternal interviewInfo)
        {
            if (variableName == this.substitutionService.RosterTitleSubstitutionReference)
            {
                if (string.IsNullOrEmpty(rosterTitle))
                    return null;
                return rosterTitle;
            }

            if (!interviewInfo.VariableToQuestionId.ContainsKey(variableName))
                return null;

            Guid questionId = interviewInfo.VariableToQuestionId[variableName];

            InterviewQuestion interviewQuestion = GetQuestion(questionId, currentInterviewLevel, upperInterviewLevels);

            if (interviewQuestion == null)
                return null;

            if (interviewQuestion.IsDisabled())
                return null;

            return GetFormattedAnswerForTitleSubstitution(interviewQuestion, interviewInfo);
        }

        private static string GetFormattedAnswerForTitleSubstitution(InterviewQuestion interviewQuestion, InterviewInfoInternal interviewInfo)
        {
            if (interviewQuestion.Answer == null)
                return null;

            var question = interviewInfo.Questionnaire.Find<IQuestion>(interviewQuestion.Id);

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
                    if (!question.LinkedToQuestionId.HasValue && !question.LinkedToRosterId.HasValue)
                    {
                        decimal selectedValue = Convert.ToDecimal(interviewQuestion.Answer);

                        var selectedAnswer = question.Answers.SingleOrDefault(option => Convert.ToDecimal(option.AnswerValue) == selectedValue);

                        return selectedAnswer?.AnswerText;
                    }

                    decimal[] selectedRosterVector = ((IEnumerable) interviewQuestion.Answer).OfType<decimal>().ToArray();

                    Dictionary<decimal[], string> availableOptions = question.LinkedToQuestionId.HasValue
                        ? GetAvailableOptions(interviewQuestion.Id, selectedRosterVector, interviewInfo)
                        : GetAvailableOptionsForQuestionLinkedOnRoster(question, selectedRosterVector,
                            question.LinkedToRosterId.Value, interviewInfo);

                    KeyValuePair<decimal[], string> selectedOption = availableOptions.SingleOrDefault(option => option.Key.SequenceEqual(selectedRosterVector));

                    return selectedOption.Value;

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
            return currentInterviewLevel.QuestionsSearchCache.ContainsKey(questionId)? currentInterviewLevel.QuestionsSearchCache[questionId] : null;
        }

        private static Dictionary<decimal[], string> GetAvailableOptions(Guid questionId, decimal[] questionRosterVector, InterviewInfoInternal interviewInfo)
        {
            var optionsSource = GetQuestionReferencedQuestion(questionId, interviewInfo.LinkedQuestionReferences);
            if (optionsSource == null)
                return new Dictionary<decimal[], string>();

            IEnumerable<InterviewLevel> allAvailableLevelsByScope = GetAllAvailableLevelsByScope(interviewInfo.Interview, questionRosterVector, optionsSource.ReferencedQuestionRosterScope, optionsSource.LinkedQuestionRosterScope);

            IDictionary<decimal[], InterviewQuestion> allLinkedQuestions =
                allAvailableLevelsByScope.ToDictionary(interviewLevel => interviewLevel.RosterVector,
                interviewLevel => interviewLevel.QuestionsSearchCache.ContainsKey(optionsSource.ReferencedQuestionId)?interviewLevel.QuestionsSearchCache[optionsSource.ReferencedQuestionId] : null);

            return allLinkedQuestions.Where(question => question.Value != null && !question.Value.IsDisabled() && question.Value.Answer != null)
                .ToDictionary(question => question.Key,
                    question => CreateLinkedQuestionOption(question.Value.Answer.ToString(), question.Key, questionRosterVector, optionsSource.ReferencedQuestionRosterScope, optionsSource.LinkedQuestionRosterScope, interviewInfo));
        }

        private static Dictionary<decimal[], string> GetAvailableOptionsForQuestionLinkedOnRoster(IQuestion questionId, decimal[] questionRosterVector, Guid rosterId, InterviewInfoInternal interviewInfo)
        {
            var referencedRosterScope = GetRosterSizeSourcesForEntity(interviewInfo.Questionnaire.Find<IGroup>(rosterId));
            var linkedQuestionRosterScope = GetRosterSizeSourcesForEntity(questionId);
            IEnumerable<InterviewLevel> allAvailableLevelsByScope = GetAllAvailableLevelsByScope(
                interviewInfo.Interview, questionRosterVector, referencedRosterScope, linkedQuestionRosterScope);
            
            return
                allAvailableLevelsByScope.ToDictionary(interviewLevel => interviewLevel.RosterVector,
                    interviewLevel => CreateLinkedQuestionOption(interviewLevel.RosterRowTitles[rosterId], interviewLevel.RosterVector, questionRosterVector, referencedRosterScope, linkedQuestionRosterScope, interviewInfo)/* interviewLevel.RosterRowTitles[rosterId]*/);
        }

        private static IEnumerable<InterviewLevel> GetAllAvailableLevelsByScope(InterviewData interview, decimal[] questionRosterVector, ValueVector<Guid> referencedRosterScope, ValueVector<Guid> linkedQuestionRosterScope)
        {
            return
                interview.Levels.Values.Where(
                    level =>
                        level.ScopeVectors.ContainsKey(referencedRosterScope) &&
                        LinkedQuestionUtils.IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(level.RosterVector,
                            referencedRosterScope, questionRosterVector, linkedQuestionRosterScope));
        }

        private static string CreateLinkedQuestionOption(
            string answer, 
            decimal[] referencedQuestionRosterVector, 
            decimal[] linkedQuestionRosterVector,
            ValueVector<Guid> referencedRosterScope, 
            ValueVector<Guid> linkedQuestionRosterScope, 
            InterviewInfoInternal interviewInfo)
        {
            return LinkedQuestionUtils.BuildLinkedQuestionOptionTitle(answer,
                (levelRosterVector) =>
                {
                    var levelFromScope =
                        interviewInfo.Interview.Levels.Values.FirstOrDefault(level => level.RosterVector.SequenceEqual(levelRosterVector));
                    if (levelFromScope != null)
                    {
                        if (levelFromScope.RosterRowTitles.Any())
                        {
                            return levelFromScope.RosterRowTitles.First().Value;
                        }
                    }
                    return string.Empty;
                }, referencedQuestionRosterVector, referencedRosterScope, linkedQuestionRosterVector,
                linkedQuestionRosterScope);
        }

        private static ReferenceInfoByQuestion GetQuestionReferencedQuestion(Guid questionId, ReferenceInfoForLinkedQuestions referenceQuestions)
        {
            return !referenceQuestions.ReferencesOnLinkedQuestions.ContainsKey(questionId)
                ? null
                : referenceQuestions.ReferencesOnLinkedQuestions[questionId];
        }

        public static ValueVector<Guid> GetRosterSizeSourcesForEntity(IComposite entity)
        {
            var rosterSizes = new List<Guid>();
            while (!(entity is IQuestionnaireDocument))
            {
                var group = entity as IGroup;
                if (group != null)
                {
                    if (group.IsRoster)
                        rosterSizes.Add(group.RosterSizeQuestionId ?? group.PublicKey);

                }
                entity = entity.GetParent();
            }
            rosterSizes.Reverse();
            return rosterSizes.ToArray();
        }
    }
}