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
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public class InterviewDataAndQuestionnaireMerger : IInterviewDataAndQuestionnaireMerger
    {
        private readonly ISubstitutionService substitutionService;

        public InterviewDataAndQuestionnaireMerger(
            ISubstitutionService substitutionService)
        {
            this.substitutionService = substitutionService;
        }

        private class InterviewInfoInternal
        {
            public InterviewData Interview { get; }
            public IQuestionnaireDocument Questionnaire { get; }
            public Dictionary<string, Guid> VariableToQuestionId { get; }
            public Dictionary<string, AttachmentInfoView> Attachments { get; }

            public InterviewInfoInternal(
                InterviewData interview,
                IQuestionnaireDocument questionnaire,
                Dictionary<string, Guid> variableToQuestionId,
                Dictionary<string, AttachmentInfoView> attachments)
            {
                this.Interview = interview;
                this.Questionnaire = questionnaire;
                this.VariableToQuestionId = variableToQuestionId;
                this.Attachments = attachments;
            }
        }

        public InterviewDetailsView Merge(InterviewData interview, IQuestionnaireDocument questionnaire, UserLight responsible, 
            InterviewLinkedQuestionOptions interviewLinkedQuestionOptions, IEnumerable<AttachmentInfoView> attachmentInfoViews)
        {
            questionnaire.ConnectChildrenWithParent();
            Dictionary<string, AttachmentInfoView> attachmentInfos = new Dictionary<string, AttachmentInfoView>();

            var questionnaireDocument = questionnaire as QuestionnaireDocument;
            if (questionnaireDocument != null && questionnaireDocument.Attachments != null && attachmentInfoViews != null)
            {
                foreach (var attachment in questionnaireDocument.Attachments)
                {
                    AttachmentInfoView attachmentInfoView = attachmentInfoViews.FirstOrDefault(x => x.ContentHash == attachment.ContentId);
                    attachmentInfos.Add(attachment.Name, attachmentInfoView);
                }
            }

            var interviewInfo = new InterviewInfoInternal(
                interview: interview,
                questionnaire: questionnaire,
                variableToQuestionId: questionnaire.GetAllQuestions().ToDictionary(x => x.StataExportCaption, x => x.PublicKey),
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
                ReceivedByInterviewer = interview.ReceivedByInterviewer
            };
        }

        private InterviewLevel GetRootLevel(InterviewData interview)
        {
            return interview.Levels.FirstOrDefault(w => w.Value.ScopeVectors.Count==1 && w.Value.ScopeVectors.ContainsKey(new ValueVector<Guid>())).Value;
        }

        private IEnumerable<KeyValuePair<string, InterviewLevel>> GetRosterLevels(IGroup group, InterviewInfoInternal interviewInfo)
        {
            var groupScope = GetRosterSizeSourcesForEntity(group);

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
                    result.AddRange(GetCompletedRosterGroups(nestedGroup, depth + 1, interviewLevel, upperInterviewLevels, interviewInfo, interviewLinkedQuestionOptions));    
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

                    var answersForTitleSubstitution = GetAnswersForTitleSubstitution(question, interviewLevel, upperInterviewLevels, rosterTitleFromLevel, interviewInfo, interviewLinkedQuestionOptions);

                    bool isQuestionsParentGroupDisabled = interviewLevel.DisabledGroups != null && IsQuestionParentGroupDisabled(disabledGroups, currentGroup);

                    if (question.LinkedToQuestionId.HasValue)
                        interviewEntity = new InterviewLinkedQuestionView(question, answeredQuestion,
                            answersForTitleSubstitution,
                            GetAvailableOptions(question, interviewLevel.RosterVector, interviewInfo, interviewLinkedQuestionOptions),
                            isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                    else if (question.LinkedToRosterId.HasValue)
                        interviewEntity = new InterviewLinkedQuestionView(question, answeredQuestion,
                            answersForTitleSubstitution,
                            GetAvailableOptionsForQuestionLinkedOnRoster(question, interviewLevel.RosterVector, interviewInfo, interviewLinkedQuestionOptions),
                            isQuestionsParentGroupDisabled, interviewLevel.RosterVector, interviewInfo.Interview.Status);
                    else
                        interviewEntity = new InterviewQuestionView(question,
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

                    interviewEntity = new InterviewStaticTextView(staticText, attachment == null ? null : new InterviewAttachmentViewModel(attachment.ContentHash, attachment.ContentType, staticText.AttachmentName));

                    completedGroup.Entities.Add(interviewEntity);
                }
            }

            return completedGroup;
        }

        private Dictionary<string, string> GetAnswersForTitleSubstitution(
            IQuestion question, 
            InterviewLevel currentInterviewLevel, 
            List<InterviewLevel> upperInterviewLevels, 
            string rosterTitle, 
            InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            return question.GetVariablesUsedInTitle()
                .Select(variableName => new
                {
                    Variable = variableName,
                    Answer = this.GetAnswerForTitleSubstitution(variableName, currentInterviewLevel, upperInterviewLevels, rosterTitle, interviewInfo, interviewLinkedQuestionOptions),
                })
                .Where(x => x.Answer != null)
                .ToDictionary(x => x.Variable, x => x.Answer);
        }

        private string GetAnswerForTitleSubstitution(string variableName, 
            InterviewLevel currentInterviewLevel, List<InterviewLevel> upperInterviewLevels, string rosterTitle, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
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

            return GetFormattedAnswerForTitleSubstitution(interviewQuestion, interviewInfo, interviewLinkedQuestionOptions);
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
                        ? GetAvailableOptions(question, selectedRosterVector, interviewInfo, interviewLinkedQuestionOptions)
                        : GetAvailableOptionsForQuestionLinkedOnRoster(question, selectedRosterVector, interviewInfo, interviewLinkedQuestionOptions);

                    KeyValuePair<decimal[], string> selectedOption = availableOptions.SingleOrDefault(option => option.Key.SequenceEqual(selectedRosterVector));

                    return selectedOption.Value;

                default:
                    return null;
            }
        }

        private InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel, IEnumerable<InterviewLevel> upperInterviewLevels)
        {
            return GetQuestion(questionId, currentInterviewLevel)
                ?? upperInterviewLevels.Select(level => GetQuestion(questionId, level)).FirstOrDefault(q => q != null);
        }

        private InterviewQuestion GetQuestion(Guid questionId, InterviewLevel currentInterviewLevel)
        {
            return currentInterviewLevel.QuestionsSearchCache.ContainsKey(questionId)? currentInterviewLevel.QuestionsSearchCache[questionId] : null;
        }

        private Dictionary<decimal[], string> GetAvailableOptions(IQuestion questionId, decimal[] questionRosterVector, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            var referencedQuestion = interviewInfo.Questionnaire.Find<IQuestion>(questionId.LinkedToQuestionId.Value);
            var referencedRosterScope = GetRosterSizeSourcesForEntity(referencedQuestion);
            var linkedQuestionRosterScope = GetRosterSizeSourcesForEntity(questionId);

            var questionStringKey=new Identity(questionId.PublicKey, questionRosterVector).ToString();

            IDictionary<RosterVector, InterviewQuestion> allLinkedQuestions = null;

            if (interviewLinkedQuestionOptions!=null && interviewLinkedQuestionOptions.LinkedQuestionOptions.ContainsKey(questionStringKey))
            {
                var options = interviewLinkedQuestionOptions.LinkedQuestionOptions[questionStringKey];

                allLinkedQuestions = options.ToDictionary(option => option,
                    option =>
                    {
                        var interviewLevel= interviewInfo.Interview.Levels.Values.FirstOrDefault(
                            x =>
                                x.RosterVector.SequenceEqual(option) &&
                                x.ScopeVectors.ContainsKey(referencedRosterScope));
                        if (interviewLevel == null)
                            return null;
                        return interviewLevel.QuestionsSearchCache.ContainsKey(referencedQuestion.PublicKey)
                            ? interviewLevel.QuestionsSearchCache[referencedQuestion.PublicKey]
                            : null;

                    });
            }
            else
            {

                IEnumerable<InterviewLevel> allAvailableLevelsByScope =
                    GetAllAvailableLevelsByScope(interviewInfo.Interview, questionRosterVector, referencedRosterScope,
                        linkedQuestionRosterScope);

                allLinkedQuestions =
                    allAvailableLevelsByScope.ToDictionary(interviewLevel => new RosterVector(interviewLevel.RosterVector),
                        interviewLevel =>
                            interviewLevel.QuestionsSearchCache.ContainsKey(referencedQuestion.PublicKey)
                                ? interviewLevel.QuestionsSearchCache[referencedQuestion.PublicKey]
                                : null);
            }

            return allLinkedQuestions.Where(question => question.Value != null && !question.Value.IsDisabled() && question.Value.Answer != null)
                .ToDictionary(question => question.Key.ToArray(),
                    question => CreateLinkedQuestionOption(question.Value.Answer.ToString(), question.Key, questionRosterVector, referencedRosterScope, linkedQuestionRosterScope, interviewInfo));
        }

        private Dictionary<decimal[], string> GetAvailableOptionsForQuestionLinkedOnRoster(IQuestion question, decimal[] questionRosterVector, InterviewInfoInternal interviewInfo, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions)
        {
            var referencedRoster = interviewInfo.Questionnaire.Find<IGroup>(question.LinkedToRosterId.Value);
            var referencedRosterScope = GetRosterSizeSourcesForEntity(referencedRoster);
            var linkedQuestionRosterScope = GetRosterSizeSourcesForEntity(question);

            var questionStringKey = new Identity(question.PublicKey, questionRosterVector).ToString();

            IEnumerable<InterviewLevel> allAvailableLevelsByScope = null;
            if (interviewLinkedQuestionOptions != null &&  interviewLinkedQuestionOptions.LinkedQuestionOptions.ContainsKey(questionStringKey))
            {
                var options = interviewLinkedQuestionOptions.LinkedQuestionOptions[questionStringKey];

                allAvailableLevelsByScope = options.Select(option =>
                {
                    return interviewInfo.Interview.Levels.Values.FirstOrDefault(
                        x =>
                            x.RosterVector.SequenceEqual(option) &&
                            x.ScopeVectors.ContainsKey(referencedRosterScope));
                });
            }
            else
            {
                allAvailableLevelsByScope= GetAllAvailableLevelsByScope(
                    interviewInfo.Interview, questionRosterVector, referencedRosterScope, linkedQuestionRosterScope);
            }

            return
                allAvailableLevelsByScope.ToDictionary(interviewLevel => interviewLevel.RosterVector,
                    interviewLevel => CreateLinkedQuestionOption(
                        interviewLevel.RosterRowTitles.ContainsKey(referencedRoster.PublicKey) ? interviewLevel.RosterRowTitles[referencedRoster.PublicKey] : null,
                        interviewLevel.RosterVector, 
                        questionRosterVector, 
                        referencedRosterScope, 
                        linkedQuestionRosterScope, 
                        interviewInfo));
        }

        private IEnumerable<InterviewLevel> GetAllAvailableLevelsByScope(InterviewData interview, decimal[] questionRosterVector, ValueVector<Guid> referencedRosterScope, ValueVector<Guid> linkedQuestionRosterScope)
        {
            return
                interview.Levels.Values.Where(
                    level =>
                        level.ScopeVectors.ContainsKey(referencedRosterScope) 
                        && IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(level.RosterVector, questionRosterVector, referencedRosterScope, linkedQuestionRosterScope));
        }

        private bool IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(
            decimal[] referencedLevelRosterVector,
            decimal[] linkedQuestionRosterVector,
            ValueVector<Guid> referencedLevelRosterScopeVector,
            ValueVector<Guid> linkedQuestionRosterScopeVector)
        {
            for (int i = 0;i < Math.Min(referencedLevelRosterVector.Length - 1, linkedQuestionRosterVector.Length);i++)
            {
                if (referencedLevelRosterScopeVector[i] != linkedQuestionRosterScopeVector[i])
                    continue;
                if (referencedLevelRosterVector[i] != linkedQuestionRosterVector[i])
                    return false;
            }
            return true;
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

        public  ValueVector<Guid> GetRosterSizeSourcesForEntity(IComposite entity)
        {
            var rosterSizes = new List<Guid>();
            while (!(entity is IQuestionnaireDocument))
            {
                var group = entity as IGroup;
                if (group != null)
                {
                    if (IsRoster(group))
                        rosterSizes.Add(group.RosterSizeQuestionId ?? group.PublicKey);

                }
                entity = entity.GetParent();
            }
            rosterSizes.Reverse();
            return rosterSizes.ToArray();
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