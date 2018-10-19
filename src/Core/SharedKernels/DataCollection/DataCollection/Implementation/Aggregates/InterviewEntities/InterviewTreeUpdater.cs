using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly bool updateLinkedAnswers;

        readonly HashSet<Identity> disabledNodes = new HashSet<Identity>();
        private readonly ConcurrentDictionary<Identity, IInterviewLevel> memoryCache = new ConcurrentDictionary<Identity, IInterviewLevel>();

        public InterviewTreeUpdater(IInterviewExpressionStorage expressionStorage, IQuestionnaire questionnaire,
            bool removeLinkedAnswers)
        {
            this.expressionStorage = expressionStorage;
            this.questionnaire = questionnaire;
            this.questionnaireIdentity = new Identity(questionnaire.QuestionnaireId, RosterVector.Empty);
            this.updateLinkedAnswers = removeLinkedAnswers;
        }

        public void UpdateEnablement(IInterviewTreeNode entity)
        {
            if (this.IsParentDiabled(entity))
                return;

            var level = this.GetLevel(entity);
            var result = RunConditionExpression(level.GetConditionExpression(entity.Identity));
            if (result)
                entity.Enable();
            else
            {
                entity.Disable();
                if (!(entity is InterviewTreeQuestion question))
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
                disabledNodes.UnionWith(disabledChildNodes);
            });
        }

        private static bool IsRosterSizeQuestionType(InterviewTreeQuestion question)
        {
            return question.IsInteger || question.IsMultiFixedOption || question.IsTextList || question.IsYesNo;
        }

        public void UpdateEnablement(InterviewTreeGroup group)
        {
            if (this.IsParentDiabled(group))
            {
                return;
            }

            var level = this.GetLevel(group);

            var result = RunConditionExpression(level.GetConditionExpression(group.Identity));
            if (result)
                group.Enable();
            else
            {
                group.Disable();
                List<Identity> disabledChildNodes = group.DisableChildNodes();
                disabledNodes.UnionWith(disabledChildNodes);
            }
        }

        public void UpdateEnablement(InterviewTreeRoster roster)
        {
            if (this.IsParentDiabled(roster))
            {
                var isRosterEnabledInDisabledParent = !roster.IsDisabledByOwnCondition();
                if (isRosterEnabledInDisabledParent)
                    DisableRoster();

                return;
            }

            var level = this.GetLevel(roster);

            var result = RunConditionExpression(level.GetConditionExpression(roster.Identity));
            if (result)
            {
                var rosterSizeQuestion = roster.GetQuestionFromThisOrUpperLevel(roster.RosterSizeId); 
                if (rosterSizeQuestion != null && rosterSizeQuestion.IsDisabledByOwnCondition())
                    DisableRoster();
                else
                    roster.Enable();
            }
            else
            {
                DisableRoster();
            }

            void DisableRoster()
            {
                roster.Disable();
                List<Identity> disabledChildNodes = roster.DisableChildNodes();
                disabledNodes.UnionWith(disabledChildNodes);
            }
        }

        public void UpdateSingleOptionQuestion(InterviewTreeQuestion question)
        {
            if (this.IsParentDiabled(question))
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
            if (this.IsParentDiabled(question))
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
                    CategoricalFixedMultiOptionAnswer.Convert(newSelectedOptions), DateTime.UtcNow);
                // remove rosters, implement cheaper solutions
                question.Tree.ActualizeTree();
            }
        }

        public void UpdateYesNoQuestion(InterviewTreeQuestion question)
        {
            if (this.IsParentDiabled(question))
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
                question.SetAnswer(YesNoAnswer.FromCheckedYesNoAnswerOptions(newCheckedOptions), DateTime.UtcNow);
                // remove rosters, implement cheaper solutions
                question.Tree.ActualizeTree();
            }
        }

        public void UpdateCascadingQuestion(InterviewTreeQuestion question)
        {
            if (this.IsParentDiabled(question))
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
            if (this.IsParentDiabled(question))
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
            question.UpdateLinkedOptionsAndUpdateAnswerIfNeeded(options.ToArray(), this.updateLinkedAnswers);
        }

        public void UpdateLinkedToListQuestion(InterviewTreeQuestion question)
        {
            if (this.IsParentDiabled(question))
                return;

            question.CalculateLinkedToListOptions(this.updateLinkedAnswers);
        }

        public void UpdateRoster(InterviewTreeRoster roster)
        {
            if (this.IsParentDiabled(roster))
                return;

            roster.UpdateRosterTitle((questionId, answerOptionValue) => this.questionnaire
                .GetOptionForQuestionByOptionValue(questionId, answerOptionValue).Title);
        }

        public void UpdateVariable(InterviewTreeVariable variable)
        {
            if (this.IsParentDiabled(variable))
                return;

            var level = this.GetLevel(variable);

            Func<object> expression = level.GetVariableExpression(variable.Identity);
            variable.SetValue(GetVariableValue(expression));
        }

        public void UpdateValidations(InterviewTreeStaticText staticText)
        {
            UpdateValidationsForEntity(staticText);
        }

        public void UpdateValidations(InterviewTreeQuestion question)
        {
            if (!question.IsAnswered())
            {
                question.MarkValid();
                question.MarkPlausible();
                return;
            }

            UpdateValidationsForEntity(question);
        }

        private void UpdateValidationsForEntity(InterviewTreeLeafNode entity)
        {
            if (!(entity is IInterviewTreeValidateable interviewTreeValidateable))
                return;

            IInterviewLevel level = this.GetLevel(entity);
            var validationExpressions = level.GetValidationExpressions(entity.Identity) ?? new Func<bool>[0];
            var validationResult = validationExpressions.Select(RunConditionExpression)
                .Select((x, i) => !x ? new FailedValidationCondition(i) : null)
                .Where(x => x != null)
                .ToArray();

            var warningIndexes = questionnaire.GetValidationWarningsIndexes(entity.Identity.Id).ToHashSet();

            if (validationResult.Any(v => warningIndexes.Contains(v.FailedConditionIndex)))
                interviewTreeValidateable.MarkImplausible(validationResult.Where(v => warningIndexes.Contains(v.FailedConditionIndex)));
            else
                interviewTreeValidateable.MarkPlausible();

            if (validationResult.Any(v => !warningIndexes.Contains(v.FailedConditionIndex)))
                interviewTreeValidateable.MarkInvalid(validationResult.Where(v => !warningIndexes.Contains(v.FailedConditionIndex)));
            else
                interviewTreeValidateable.MarkValid();
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
            bool foundInCache = this.memoryCache.TryGetValue(nearestRoster, out var cachedLevel);
            if (!foundInCache) return null;

            return cachedLevel;
        }

        private bool IsParentDiabled(IInterviewTreeNode node)
        {
            if (this.disabledNodes.Contains(node.Identity))
                return true;
            return node.Parent?.IsDisabled() ?? false;
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
            catch (TypeLoadException)
            {
                throw;
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
