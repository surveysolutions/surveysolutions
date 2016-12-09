using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class InterviewDataAndQuestionnaireMerger : IInterviewDataAndQuestionnaireMerger
    {
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        private readonly IInterviewEntityViewFactory interviewEntityViewFactory;

        public InterviewDataAndQuestionnaireMerger(
            ISubstitutionService substitutionService, 
            IVariableToUIStringService variableToUiStringService,
            IInterviewEntityViewFactory interviewEntityViewFactory
            )
        {
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
            this.interviewEntityViewFactory = interviewEntityViewFactory;
        }

        private class InterviewInfoInternal
        {
            public InterviewData Interview { get; }
            public QuestionnaireDocument Questionnaire { get; }
            public Dictionary<string, Guid> VariableToQuestionId { get; }
            public Dictionary<string, Guid> VariableToVariableId { get; }
            public Dictionary<string, Guid> VariableToRosterId { get; }
            public Dictionary<string, AttachmentInfoView> Attachments { get; }

            public InterviewInfoInternal(
                InterviewData interview,
                QuestionnaireDocument questionnaire,
                Dictionary<string, Guid> variableToQuestionId,
                Dictionary<string, Guid> variableToVariableId,
                Dictionary<string, Guid> variableToRosterId,
                Dictionary<string, AttachmentInfoView> attachments)
            {
                this.Interview = interview;
                this.Questionnaire = questionnaire;
                this.VariableToQuestionId = variableToQuestionId;
                this.VariableToVariableId = variableToVariableId;
                this.VariableToRosterId = variableToRosterId;
                this.Attachments = attachments;
            }
        }


        public InterviewDetailsView Merge(InterviewData interview, 
            QuestionnaireDocument questionnaire, 
            UserLight responsible, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions, 
            IEnumerable<AttachmentInfoView> attachmentInfoViews)
        {
            Dictionary<string, AttachmentInfoView> attachmentInfos = new Dictionary<string, AttachmentInfoView>();

            if (questionnaire.Attachments != null && attachmentInfoViews != null)
            {
                foreach (var attachment in questionnaire.Attachments)
                {
                    AttachmentInfoView attachmentInfoView = attachmentInfoViews.FirstOrDefault(x => x.ContentHash == attachment.ContentId);
                    attachmentInfos.Add(attachment.Name, attachmentInfoView);
                }
            }

            var interviewInfo = new InterviewInfoInternal(
                interview: interview,
                questionnaire: questionnaire,
                variableToQuestionId: questionnaire.GetAllQuestions().ToDictionary(x => x.StataExportCaption, x => x.PublicKey),
                variableToVariableId: questionnaire.Find<IVariable>().ToDictionary(x => x.Name, x => x.PublicKey),
                variableToRosterId:   questionnaire.Find<IGroup>(g => g.IsRoster).ToDictionary(x => x.VariableName, x => x.PublicKey),
                attachments: attachmentInfos);

            var interviewGroups = new List<InterviewGroupView>();
            var groupStack = new Stack<KeyValuePair<IGroup, int>>();

            groupStack.Push(new KeyValuePair<IGroup, int>(questionnaire, 0));
            while (groupStack.Count > 0)
            {
                var currentGroup = groupStack.Pop();

                var rootLevel = this.GetRootLevel(interview);
                
                if (IsRoster(currentGroup.Key))
                {
                    var rosterLevels = this.GetRosterLevels(currentGroup.Key, interviewInfo).ToList();

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
                                interviewInfo: interviewInfo,
                                interviewLinkedQuestionOptions: interviewLinkedQuestionOptions);

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
                            interviewInfo: interviewInfo,
                            interviewLinkedQuestionOptions: interviewLinkedQuestionOptions));

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
                ReceivedByInterviewer = interview.ReceivedByInterviewer,
                CurrentTranslation = interview.CurrentLanguage,
                IsAssignedToInterviewer = !interview.IsMissingAssignToInterviewer
            };
        }

        private InterviewLevel GetRootLevel(InterviewData interview)
        {
            return interview.Levels.FirstOrDefault(w => w.Value.ScopeVectors.Count==1 && w.Value.ScopeVectors.ContainsKey(new ValueVector<Guid>())).Value;
        }

        private IEnumerable<KeyValuePair<string, InterviewLevel>> GetRosterLevels(IGroup group, InterviewInfoInternal interviewInfo)
        {
            var groupScope = InterviewLevelUtils.GetRosterSizeSourcesForEntity(group);

            return interviewInfo.Interview.Levels.Where(w => w.Value.ScopeVectors.ContainsKey(groupScope))
                          .OrderBy(x => x.Value.ScopeVectors.First().Value ?? x.Value.RosterVector.Last());
        }

        private IEnumerable<InterviewGroupView> GetCompletedRosterGroups(IGroup currentGroup, int depth, InterviewLevel interviewLevel,
            List<InterviewLevel> upperInterviewLevels, InterviewInfoInternal interviewInfo,
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            var result = new List<InterviewGroupView>
            {
                this.GetCompletedGroup(currentGroup, depth, interviewLevel, upperInterviewLevels, interviewInfo,interviewLinkedQuestionOptions)
            };

            foreach (var nestedGroup in currentGroup.Children.OfType<IGroup>())
            {
                //nested roster supporting
                if (IsRoster(nestedGroup))
                {
                    var rosterLevels = this.GetRosterLevels(nestedGroup, interviewInfo)
                        .Where(kv => kv.Value.RosterVector.Take(kv.Value.RosterVector.Length - 1)
                            .SequenceEqual(interviewLevel.RosterVector))
                        .ToList();

                    if (rosterLevels.Any())
                    {
                        foreach (var rosterGroup in rosterLevels)
                        {
                            var completedRosterGroups = this.GetCompletedRosterGroups(nestedGroup, depth + 1,
                                rosterGroup.Value, upperInterviewLevels.Union(new[] {interviewLevel}).ToList(), interviewInfo, interviewLinkedQuestionOptions);

                            result.AddRange(completedRosterGroups);
                        }
                    }
                }
                else
                {
                    result.AddRange(this.GetCompletedRosterGroups(nestedGroup, depth + 1, interviewLevel, upperInterviewLevels, interviewInfo, interviewLinkedQuestionOptions));    
                }
                
            }
            return result;
        }

        private InterviewGroupView GetCompletedGroup(
            IGroup currentGroup, 
            int depth, 
            InterviewLevel interviewLevel, 
            List<InterviewLevel> upperInterviewLevels,
            InterviewInfoInternal interviewInfo,
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            Guid nearestParentRosterId = this.GetNearestParentRosterId(interviewInfo.Questionnaire, currentGroup.PublicKey);
            var rosterTitleFromLevel = interviewLevel.RosterRowTitles.ContainsKey(nearestParentRosterId)
                ? interviewLevel.RosterRowTitles[nearestParentRosterId]
                : null;

            var currentGroupTitle = this.GetTitleWithSubstitutedVariables(currentGroup.Title, interviewLevel, upperInterviewLevels, rosterTitleFromLevel, interviewInfo, interviewLinkedQuestionOptions);

            var rosterTitle = !string.IsNullOrEmpty(rosterTitleFromLevel)
                ? $"{currentGroupTitle}: {rosterTitleFromLevel}"
                : currentGroupTitle;

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

                    var answersForTitleSubstitution = this.GetAnswersForStaticTextSubstitution(question, interviewLevel, upperInterviewLevels, rosterTitleFromLevel, interviewInfo, interviewLinkedQuestionOptions);

                    bool isQuestionsParentGroupDisabled = interviewLevel.DisabledGroups != null && IsQuestionParentGroupDisabled(disabledGroups, currentGroup);

                    if (question.LinkedToQuestionId.HasValue)
                        if(interviewInfo.Questionnaire.FirstOrDefault<IQuestion>(x => x.PublicKey == question.LinkedToQuestionId.Value).QuestionType == QuestionType.TextList)
                        {
                            InterviewQuestion interviewQuestion = this.GetQuestion(question.LinkedToQuestionId.Value, interviewLevel, upperInterviewLevels);

                            interviewEntity = this.interviewEntityViewFactory.BuildInterviewLinkedToListQuestionView(question, answeredQuestion,
                                answersForTitleSubstitution, interviewQuestion?.Answer,
                                isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                        }
                        else
                            interviewEntity = this.interviewEntityViewFactory.BuildInterviewLinkedToRosterQuestionView(question, answeredQuestion,
                                answersForTitleSubstitution,
                                this.GetAvailableOptions(question, interviewLevel.RosterVector, interviewInfo, interviewLinkedQuestionOptions),
                                isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                    else if (question.LinkedToRosterId.HasValue)
                        interviewEntity = this.interviewEntityViewFactory.BuildInterviewLinkedToRosterQuestionView(question, answeredQuestion,
                            answersForTitleSubstitution,
                            this.GetAvailableOptionsForQuestionLinkedOnRoster(question, interviewLevel.RosterVector, interviewInfo, interviewLinkedQuestionOptions),
                            isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                    else
                        interviewEntity = this.interviewEntityViewFactory.BuildInterviewQuestionView(question,
                            answeredQuestion,
                            answersForTitleSubstitution, 
                            isQuestionsParentGroupDisabled, 
                            interviewLevel.RosterVector,
                            interviewInfo.Interview.Status);

                    completedGroup.Entities.Add(interviewEntity);
                    continue;
                }

                var staticText = entity as IStaticText;
                if (staticText != null)
                {
                    AttachmentInfoView attachment = null;
                    if (interviewInfo.Attachments != null && !string.IsNullOrWhiteSpace(staticText.AttachmentName))
                    {
                        interviewInfo.Attachments.TryGetValue(staticText.AttachmentName, out attachment); 
                    }

                    var answersForTitleSubstitution = this.GetAnswersForStaticTextSubstitution(staticText, interviewLevel, upperInterviewLevels, rosterTitleFromLevel, interviewInfo, interviewLinkedQuestionOptions);
                    var interviewStaticText = interviewLevel.StaticTexts.ContainsKey(staticText.PublicKey) ? interviewLevel.StaticTexts[staticText.PublicKey] : null;
                    var interviewAttachmentViewModel = attachment == null ? null : this.interviewEntityViewFactory.BuildInterviewAttachmentViewModel(attachment.ContentHash, attachment.ContentType, staticText.AttachmentName);
                    var interviewStaticTextView = this.interviewEntityViewFactory.BuildInterviewStaticTextView(staticText, interviewStaticText, answersForTitleSubstitution, interviewAttachmentViewModel);

                    interviewEntity = interviewStaticTextView;
                    
                    completedGroup.Entities.Add(interviewEntity);
                }
            }

            return completedGroup;
        }


        private string GetTitleWithSubstitutedVariables(string title, 
            InterviewLevel currentInterviewLevel,
            List<InterviewLevel> upperInterviewLevels,
            string rosterTitle,
            InterviewInfoInternal interviewInfo, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            IEnumerable<string> usedVariables = this.substitutionService.GetAllSubstitutionVariableNames(title);

            Dictionary<string, string> answersForTitleSubstitution = GetAnswersForTitleSubstitution(title,
                currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo, interviewLinkedQuestionOptions);

            foreach (string usedVariable in usedVariables)
            {
                string escapedVariable = $"%{usedVariable}%";
                string actualAnswerOrDots = answersForTitleSubstitution.ContainsKey(usedVariable) ? answersForTitleSubstitution[usedVariable] : "[...]";

                title = title.Replace(escapedVariable, actualAnswerOrDots);
            }

            return title;
        }

        private Dictionary<string, string> GetAnswersForStaticTextSubstitution(
            IStaticText staticText, 
            InterviewLevel currentInterviewLevel, 
            List<InterviewLevel> upperInterviewLevels, 
            string rosterTitle, 
            InterviewInfoInternal interviewInfo, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            var allSubstitutionVariableNames = this.substitutionService.GetAllSubstitutionVariableNames(staticText.Text).ToHashSet();
            foreach (var validationCondition in staticText.ValidationConditions)
            {
                var validationConditionSubstitutionVariableNames = this.substitutionService.GetAllSubstitutionVariableNames(validationCondition.Message);
                validationConditionSubstitutionVariableNames.ForEach(variableName => allSubstitutionVariableNames.Add(variableName));
            }

            return this.GetAnswersForVariables(currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo, interviewLinkedQuestionOptions, allSubstitutionVariableNames);
        }

        private Dictionary<string, string> GetAnswersForStaticTextSubstitution(
            IQuestion question, 
            InterviewLevel currentInterviewLevel, 
            List<InterviewLevel> upperInterviewLevels, 
            string rosterTitle, 
            InterviewInfoInternal interviewInfo, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            var allSubstitutionVariableNames = this.substitutionService.GetAllSubstitutionVariableNames(question.QuestionText).ToHashSet();
            foreach (var validationCondition in question.ValidationConditions)
            {
                var validationConditionSubstitutionVariableNames = this.substitutionService.GetAllSubstitutionVariableNames(validationCondition.Message);
                validationConditionSubstitutionVariableNames.ForEach(variableName => allSubstitutionVariableNames.Add(variableName));
            }

            return this.GetAnswersForVariables(currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo, interviewLinkedQuestionOptions, allSubstitutionVariableNames);
        }

        private Dictionary<string, string> GetAnswersForVariables(InterviewLevel currentInterviewLevel, List<InterviewLevel> upperInterviewLevels, string rosterTitle, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions, IEnumerable<string> allSubstitutionVariableNames)
        {
            return allSubstitutionVariableNames
                .Select(variableName => new
                {
                    Variable = variableName,
                    Answer = this.GetAnswerForTitleSubstitution(variableName, currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo, interviewLinkedQuestionOptions),
                })
                .Where(x => x.Answer != null)
                .ToDictionary(x => x.Variable, x => x.Answer);
        }

        private Dictionary<string, string> GetAnswersForTitleSubstitution(
            string text, 
            InterviewLevel currentInterviewLevel, 
            List<InterviewLevel> upperInterviewLevels, 
            string rosterTitle, 
            InterviewInfoInternal interviewInfo, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            var allSubstitutionVariableNames = this.substitutionService.GetAllSubstitutionVariableNames(text);
            return this.GetAnswersForVariables(currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo, interviewLinkedQuestionOptions, allSubstitutionVariableNames);
        }

        private string GetAnswerForTitleSubstitution(string variableName, 
            InterviewLevel currentInterviewLevel,
            List<InterviewLevel> upperInterviewLevels, 
            string rosterTitle, 
            InterviewInfoInternal interviewInfo, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            if (variableName == this.substitutionService.RosterTitleSubstitutionReference)
            {
                if (string.IsNullOrEmpty(rosterTitle))
                    return null;
                return rosterTitle;
            }

            if (interviewInfo.VariableToRosterId.ContainsKey(variableName))
                return this.SubstituteRosterTitle(variableName, currentInterviewLevel, upperInterviewLevels, interviewInfo);

            if (!interviewInfo.VariableToQuestionId.ContainsKey(variableName))
                return this.SubstituteVariableValue(variableName, currentInterviewLevel, upperInterviewLevels, interviewInfo);

            Guid questionId = interviewInfo.VariableToQuestionId[variableName];

            InterviewQuestion interviewQuestion = this.GetQuestion(questionId, currentInterviewLevel, upperInterviewLevels);

            if (interviewQuestion == null)
                return null;

            if (interviewQuestion.IsDisabled())
                return null;

            return this.GetFormattedAnswerForTitleSubstitution(interviewQuestion, interviewInfo, interviewLinkedQuestionOptions);
        }

        private string SubstituteRosterTitle(string variableName, InterviewLevel currentInterviewLevel, List<InterviewLevel> upperInterviewLevels, InterviewInfoInternal interviewInfo)
        {
            if (!interviewInfo.VariableToRosterId.ContainsKey(variableName))
                return null;

            var rosterId = interviewInfo.VariableToRosterId[variableName];
            var questionRosterVector = currentInterviewLevel.RosterVector;
            var allInterviewLevelsToLookForTheVariable = upperInterviewLevels.Union(new[] {currentInterviewLevel}).ToArray();

            var rosterLevelDepth = currentInterviewLevel.RosterVector.Length;

            do
            {
                var variableRosterVector = questionRosterVector.Take(rosterLevelDepth).ToArray();
                var levelToLookForTheVariable =
                    allInterviewLevelsToLookForTheVariable.FirstOrDefault(
                        x => x.RosterVector.Length == rosterLevelDepth && x.RosterVector.SequenceEqual(variableRosterVector));

                if (levelToLookForTheVariable != null && levelToLookForTheVariable.RosterRowTitles.ContainsKey(rosterId))
                {
                    var rosterTitle = levelToLookForTheVariable.RosterRowTitles[rosterId];
                    return rosterTitle;
                }

                rosterLevelDepth--;

            } while (rosterLevelDepth > 0);

            return null;
        }

        private string SubstituteVariableValue(string variableName, InterviewLevel currentInterviewLevel, List<InterviewLevel> upperInterviewLevels, InterviewInfoInternal interviewInfo)
        {
            if (!interviewInfo.VariableToVariableId.ContainsKey(variableName))
                return null;

            var variableId = interviewInfo.VariableToVariableId[variableName];
            var questionRosterVector = currentInterviewLevel.RosterVector;
            var allInterviewLevelsToLookForTheVariable =
                upperInterviewLevels.Union(new[] {currentInterviewLevel}).ToArray();

            var rosterLevelDepth = currentInterviewLevel.RosterVector.Length;
            do
            {
                var variableRosterVector = questionRosterVector.Take(rosterLevelDepth).ToArray();
                var levelToLookForTheVariable =
                    allInterviewLevelsToLookForTheVariable.FirstOrDefault(
                        x => x.RosterVector.Length == rosterLevelDepth && x.RosterVector.SequenceEqual(variableRosterVector));

                if (levelToLookForTheVariable != null && levelToLookForTheVariable.Variables.ContainsKey(variableId))
                {
                    var variableValue = levelToLookForTheVariable.Variables[variableId];

                    return this.variableToUiStringService.VariableToUIString(variableValue);
                }

                rosterLevelDepth--;
            } while (rosterLevelDepth >= 0);

            return null;
        }

        private string GetFormattedAnswerForTitleSubstitution(InterviewQuestion interviewQuestion, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
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
                        ? this.GetAvailableOptions(question, selectedRosterVector, interviewInfo, interviewLinkedQuestionOptions)
                        : this.GetAvailableOptionsForQuestionLinkedOnRoster(question, selectedRosterVector, interviewInfo, interviewLinkedQuestionOptions);

                    KeyValuePair<decimal[], string> selectedOption = availableOptions.SingleOrDefault(option => option.Key.SequenceEqual(selectedRosterVector));

                    return selectedOption.Value;

                default:
                    return null;
            }
        }

        private InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels)
        {
            return this.GetQuestion(questionId, currentInterviewLevel)
                ?? upperInterviewLevels.Select(level => this.GetQuestion(questionId, level)).FirstOrDefault(q => q != null);
        }

        private InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel)
        {
            return currentInterviewLevel.QuestionsSearchCache.ContainsKey(questionId)? currentInterviewLevel.QuestionsSearchCache[questionId] : null;
        }

        private Dictionary<decimal[], string> GetAvailableOptions(IQuestion question, decimal[] questionRosterVector, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            return InterviewLevelUtils.GetAvailableOptionsForQuestionLinkedOnRoster(question, questionRosterVector,
                interviewInfo.Interview, interviewInfo.Questionnaire, interviewLinkedQuestionOptions)
                .ToDictionary(l => l.RosterVector,
                    l =>
                        this.CreateLinkedQuestionOption(
                            l.QuestionsSearchCache[question.LinkedToQuestionId.Value].Answer.ToString(),
                            l.RosterVector, questionRosterVector,
                            l.ScopeVectors.Keys.First(),
                            InterviewLevelUtils.GetRosterSizeSourcesForEntity(question), interviewInfo));
        }

        private Dictionary<decimal[], string> GetAvailableOptionsForQuestionLinkedOnRoster(IQuestion question, decimal[] questionRosterVector, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            return InterviewLevelUtils.GetAvailableOptionsForQuestionLinkedOnRoster(question, questionRosterVector,
                  interviewInfo.Interview, interviewInfo.Questionnaire, interviewLinkedQuestionOptions).ToDictionary(interviewLevel => interviewLevel.RosterVector,
                    interviewLevel => this.CreateLinkedQuestionOption(
                        interviewLevel.RosterRowTitles.ContainsKey(question.LinkedToRosterId.Value) ? interviewLevel.RosterRowTitles[question.LinkedToRosterId.Value] : null,
                        interviewLevel.RosterVector,
                        questionRosterVector,
                        interviewLevel.ScopeVectors.Keys.First(),
                        InterviewLevelUtils.GetRosterSizeSourcesForEntity(question),
                        interviewInfo));
        }

        private string CreateLinkedQuestionOption(
            string title, 
            decimal[] referencedRosterVector, 
            decimal[] linkedQuestionRosterVector,
            ValueVector<Guid> referencedRosterScope, 
            ValueVector<Guid> linkedQuestionRosterScope, 
            InterviewInfoInternal interviewInfo)
        {
            var combinedRosterTitles = new List<string>();

            for (int i = 0; i < referencedRosterScope.Length - 1; i++)
            {
                var scopeId = referencedRosterScope[i];
                var firstScreeninScopeRosterVector =
                    referencedRosterVector.Take(i + 1).ToArray();

                if (linkedQuestionRosterScope.Length > i && linkedQuestionRosterScope[i] == scopeId &&
                    linkedQuestionRosterVector.Length >= firstScreeninScopeRosterVector.Length)
                    continue;

                var levelFromScope =
                    interviewInfo.Interview.Levels.Values.FirstOrDefault(level => level.RosterVector.SequenceEqual(firstScreeninScopeRosterVector));

                if (levelFromScope != null)
                {
                    if (levelFromScope.RosterRowTitles.Any())
                    {
                        combinedRosterTitles.Add(levelFromScope.RosterRowTitles.First().Value);
                    }
                }
            }

            combinedRosterTitles.Add(title);

            return string.Join(": ", combinedRosterTitles.Where(rosterTitle => !string.IsNullOrEmpty(rosterTitle)));
        }

        private static bool IsRoster(IGroup currentGroup)
        {
            return currentGroup.IsRoster;
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

        private Guid GetNearestParentRosterId(IQuestionnaireDocument questionnaire, Guid groupId)
        {
            var currentGroup = questionnaire.Find<IGroup>(groupId);
            while (currentGroup != null)
            {
                if (currentGroup.IsRoster)
                    break;
                currentGroup = (IGroup)currentGroup.GetParent();
            }
            return currentGroup?.PublicKey ?? Guid.Empty;
        }
    }
}