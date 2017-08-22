using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeUpdater : IInterviewTreeUpdater, IDisposable
    {
        private readonly IInterviewExpressionStorage expressionStorage;
        private readonly IQuestionnaire questionnaire;
        private readonly Identity questionnaireIdentity;
        private readonly bool removeLinkedAnswers;

        readonly HashSet<Identity> disabledNodes = new HashSet<Identity>();
        private readonly ConcurrentDictionary<Identity, IInterviewLevel> memoryCache = new ConcurrentDictionary<Identity, IInterviewLevel>();

        public InterviewTreeUpdater(IInterviewExpressionStorage expressionStorage, IQuestionnaire questionnaire,
            bool removeLinkedAnswers)
        {
            this.expressionStorage = expressionStorage;
            this.questionnaire = questionnaire;
            this.questionnaireIdentity = new Identity(questionnaire.QuestionnaireId, RosterVector.Empty);
            this.removeLinkedAnswers = removeLinkedAnswers;
        }

        public void UpdateEnablement(IInterviewTreeNode entity)
        {
            if (this.disabledNodes.Contains(entity.Identity))
                return;

            var level = this.GetLevel(entity);
            var result = RunConditionExpression(level.GetConditionExpression(entity.Identity));
            if (result)
                entity.Enable();
            else
            {
                entity.Disable();
                var question = entity as InterviewTreeQuestion;
                if (question == null)
                    return;

                if (!IsRosterSizeQuestionType(question))
                    return;

                DisableDependingFormSourceQuestionRosters(question.Identity, question.Tree);
            }
        }

        private void DisableDependingFormSourceQuestionRosters(Identity questionIdentity, InterviewTree interviewTree)
        {
            var rosterIdsToBeDisabled = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionIdentity.Id);
            var rostersToBeDisabled = rosterIdsToBeDisabled
                .SelectMany(x => interviewTree.FindEntitiesFromSameOrDeeperLevel(x, questionIdentity))
                .Select(interviewTree.GetRoster)
                .Where(x => x != null)
                .ToList();

            rostersToBeDisabled.ForEach(x =>
            {
                x.Disable();
                this.disabledNodes.Add(x.Identity);
                List<Identity> disabledChildNodes = x.DisableChildNodes();
                disabledChildNodes.ForEach(d => this.disabledNodes.Add(d));
            });
        }

        private static bool IsRosterSizeQuestionType(InterviewTreeQuestion question)
        {
            return question.IsInteger || question.IsMultiFixedOption || question.IsTextList || question.IsYesNo;
        }

        public void UpdateEnablement(InterviewTreeGroup group)
        {
            if (this.disabledNodes.Contains(group.Identity))
                return;

            var level = this.GetLevel(group);

            var result = RunConditionExpression(level.GetConditionExpression(group.Identity));
            if (result)
                group.Enable();
            else
            {
                group.Disable();
                List<Identity> disabledChildNodes = group.DisableChildNodes();
                disabledChildNodes.ForEach(x => this.disabledNodes.Add(x));
            }
        }

        public void UpdateSingleOptionQuestion(InterviewTreeQuestion question)
        {
            if (this.disabledNodes.Contains(question.Identity))
                return;

            if (!(question.IsAnswered() && this.questionnaire.IsSupportFilteringForOptions(question.Identity.Id)))
                return;

            var level = this.GetLevel(question);
            var filter = level.GetCategoricalFilter(question.Identity);
            var filterResult = RunOptionFilter(filter,
                question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue);
            if (!filterResult)
                question.RemoveAnswer();
        }

        public void UpdateMultiOptionQuestion(InterviewTreeQuestion question)
        {
            if (this.disabledNodes.Contains(question.Identity))
                return;

            if (!(question.IsAnswered() && this.questionnaire.IsSupportFilteringForOptions(question.Identity.Id)))
                return;

            var level = this.GetLevel(question);
            var filter = level.GetCategoricalFilter(question.Identity);
            var selectedOptions =
                question.GetAsInterviewTreeMultiOptionQuestion().GetAnswer().CheckedValues.ToArray();
            var newSelectedOptions =
                selectedOptions.Where(x => RunOptionFilter(filter, x)).ToArray();
            if (newSelectedOptions.Length != selectedOptions.Length)
            {
                question.SetAnswer(
                    CategoricalFixedMultiOptionAnswer.FromInts(newSelectedOptions));
                // remove rosters, implement cheaper solutions
                question.Tree.ActualizeTree();
            }
        }

        public void UpdateYesNoQuestion(InterviewTreeQuestion question)
        {
            if (this.disabledNodes.Contains(question.Identity))
                return;

            if (!(question.IsAnswered() && this.questionnaire.IsSupportFilteringForOptions(question.Identity.Id)))
                return;

            var level = this.GetLevel(question);
            var filter = level.GetCategoricalFilter(question.Identity);
            var checkedOptions = question.GetAsInterviewTreeYesNoQuestion().GetAnswer().CheckedOptions;
            var newCheckedOptions =
                checkedOptions.Where(x => RunOptionFilter(filter, x.Value)).ToArray();

            if (newCheckedOptions.Length != checkedOptions.Count)
            {
                question.SetAnswer(YesNoAnswer.FromCheckedYesNoAnswerOptions(newCheckedOptions));
                // remove rosters, implement cheaper solutions
                question.Tree.ActualizeTree();
            }
        }

        public void UpdateCascadingQuestion(InterviewTreeQuestion question)
        {
            if (this.disabledNodes.Contains(question.Identity))
                return;

            //move to cascading
            var cascadingParent = (question.GetAsInterviewTreeCascadingQuestion()).GetCascadingParentTreeQuestion();
            if (cascadingParent.IsDisabled() || !cascadingParent.IsAnswered())
            {
                if (question.IsAnswered())
                    question.RemoveAnswer();
                question.Disable();
            }
            else
            {
                var selectedParentValue = cascadingParent.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue;
                if (!this.questionnaire.HasAnyCascadingOptionsForSelectedParentOption(question.Identity.Id,
                    cascadingParent.Identity.Id, selectedParentValue))
                {
                    question.Disable();
                }
                else
                {
                    question.Enable();
                }
            }
        }

        public void UpdateLinkedQuestion(InterviewTreeQuestion question)
        {
            if (this.disabledNodes.Contains(question.Identity))
                return;

            var level = this.GetLevel(question);
            var optionsAndParents = question.GetCalculatedLinkedOptions();
            var options = new List<RosterVector>();
            foreach (var optionAndParent in optionsAndParents)
            {
                var optionLevel = this.expressionStorage.GetLevel(optionAndParent.ParenRoster);
                Func<IInterviewLevel, bool> filter = optionLevel.GetLinkedQuestionFilter(question.Identity);
                if (filter == null)
                {
                    options.Add(optionAndParent.Option);
                }
                else
                {
                    if (RunLinkedFilter(filter, level))
                        options.Add(optionAndParent.Option);
                }
            }
            question.UpdateLinkedOptionsAndResetAnswerIfNeeded(options.ToArray(), this.removeLinkedAnswers);
        }

        public void UpdateLinkedToListQuestion(InterviewTreeQuestion question)
        {
            if (this.disabledNodes.Contains(question.Identity))
                return;

            question.CalculateLinkedToListOptions(this.removeLinkedAnswers);
        }

        public void UpdateRoster(InterviewTreeRoster roster)
        {
            if (this.disabledNodes.Contains(roster.Identity))
                return;

            roster.UpdateRosterTitle((questionId, answerOptionValue) => this.questionnaire
                .GetOptionForQuestionByOptionValue(questionId, answerOptionValue).Title);
        }

        public void UpdateVariable(InterviewTreeVariable variable)
        {
            if (this.disabledNodes.Contains(variable.Identity))
                return;

            var level = this.GetLevel(variable);

            Func<object> expression = level.GetVariableExpression(variable.Identity);
            variable.SetValue(GetVariableValue(expression));
        }

        public void UpdateValidations(InterviewTreeStaticText staticText)
        {
            IInterviewLevel level = this.GetLevel(staticText);
            var validationExpressions = level.GetValidationExpressions(staticText.Identity) ?? new Func<bool>[0];
            var validationResult = validationExpressions.Select(RunConditionExpression)
                .Select((x, i) => !x ? new FailedValidationCondition(i) : null)
                .Where(x => x != null)
                .ToArray();

            if (validationResult.Any())
                staticText.MarkInvalid(validationResult);
            else
                staticText.MarkValid();
        }

        public void UpdateValidations(InterviewTreeQuestion question)
        {
            if (!question.IsAnswered())
            {
                question.MarkValid();
                return;
            }

            IInterviewLevel level = this.GetLevel(question);
            var validationExpressions = level.GetValidationExpressions(question.Identity) ?? new Func<bool>[0];
            var validationResult = validationExpressions.Select(RunConditionExpression)
                .Select((x, i) => !x ? new FailedValidationCondition(i) : null)
                .Where(x => x != null)
                .ToArray();

            if (validationResult.Any())
                question.MarkInvalid(validationResult);
            else
                question.MarkValid();
        }

        private IInterviewLevel GetLevel(IInterviewTreeNode entity)
        {
            var nearestRoster = entity is InterviewTreeRoster
                ? entity.Identity
                : entity.Parents.OfType<InterviewTreeRoster>().LastOrDefault()?.Identity ?? this.questionnaireIdentity;

            var level = this.GetFromCache(nearestRoster) ?? this.expressionStorage.GetLevel(nearestRoster);

            if (level != null)
            {
                this.memoryCache[nearestRoster] = level;
            }

            return level;
        }

        private IInterviewLevel GetFromCache(Identity nearestRoster)
        {
            IInterviewLevel cachedLevel;

            bool foundInCache = this.memoryCache.TryGetValue(nearestRoster, out cachedLevel);
            if (!foundInCache) return null;

            return cachedLevel;
        }

        private static object GetVariableValue(Func<object> expression)
        {
            try
            {
                return expression();
            }
            catch
            {
                return null;
            }
        }

        private static bool RunLinkedFilter(Func<IInterviewLevel, bool> filter, IInterviewLevel level)
        {
            try
            {
                return filter(level);
            }
            catch
            {
                return false;
            }
        }


        private static bool RunOptionFilter(Func<int, bool> filter, int selectedValue)
        {
            try
            {
                return filter(selectedValue);
            }
            catch
            {
                return false;
            }
        }

        private static bool RunConditionExpression(Func<bool> expression)
        {
            try
            {
                return expression == null || expression();
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            this.memoryCache.Clear();
            this.disabledNodes.Clear();
        }
    }
}