using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class FindReplaceService : IFindReplaceService
    {
        private readonly IDesignerQuestionnaireStorage questionnaires;

        public FindReplaceService(IDesignerQuestionnaireStorage questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public IEnumerable<QuestionnaireEntityReference> FindAll(QuestionnaireRevision questionnaireId, string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            var questionnaire = this.questionnaires.Get(questionnaireId);
            if(questionnaire == null)
                throw new InvalidOperationException("Questionnaire was not found.");
            return FindAll(questionnaire, searchFor, matchCase, matchWholeWord, useRegex);
        }

        public IEnumerable<QuestionnaireEntityReference> FindAll(QuestionnaireDocument questionnaire, string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            Regex searchRegex = BuildSearchRegex(searchFor, matchCase, matchWholeWord, useRegex);
            IEnumerable<IComposite> allEntries = questionnaire.Children.TreeToEnumerableDepthFirst(x => x.Children);
            foreach (var questionnaireItem in allEntries)
            {
                var title = questionnaireItem.GetTitle();
                var variable = questionnaireItem.GetVariable();

                if (MatchesSearchTerm(variable, searchRegex) || questionnaireItem.PublicKey.ToString().Equals(searchFor, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.VariableName);
                }
                if (MatchesSearchTerm(title, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.Title);
                }

                if (questionnaireItem is IQuestion question)
                {
                    if (question.Answers != null && !(question.IsFilteredCombobox.GetValueOrDefault() || question.CascadeFromQuestionId.HasValue))
                    {
                        for (int i = 0; i < question.Answers.Count; i++)
                        {
                            if (MatchesSearchTerm(question.Answers[i].AnswerText, searchRegex))
                            {
                                yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.Option, i);
                            }
                        }
                    }

                    if (MatchesSearchTerm(question.Properties?.OptionsFilterExpression, searchRegex) || MatchesSearchTerm(question.LinkedFilterExpression, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.OptionsFilter);
                    }

                    if (MatchesSearchTerm(question.Instructions, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.Instructions);
                    }
                }

                if (questionnaireItem is IGroup @group)
                {
                    if (group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    {
                        for (int i = 0; i < group.FixedRosterTitles.Length; i++)
                        {
                            if (MatchesSearchTerm(group.FixedRosterTitles[i].Title, searchRegex))
                            {
                                yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.FixedRosterItem, i);
                            }
                        }
                    }
                }

                var conditional = questionnaireItem as IConditional;
                if (MatchesSearchTerm(conditional?.ConditionExpression, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.EnablingCondition);
                }

                if (questionnaireItem is IValidatable validatable)
                {
                    for (int i = 0; i < validatable.ValidationConditions.Count; i++)
                    {
                        if (MatchesSearchTerm(validatable.ValidationConditions[i].Expression, searchRegex))
                        {
                            yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.ValidationExpression, i);
                        }
                        if (MatchesSearchTerm(validatable.ValidationConditions[i].Message, searchRegex))
                        {
                            yield return QuestionnaireEntityReference.CreateFrom(questionnaireItem, QuestionnaireVerificationReferenceProperty.ValidationMessage, i);
                        }
                    }
                }

                if (questionnaireItem is IVariable questionnaireVariable)
                {
                    if (MatchesSearchTerm(questionnaireVariable.Label, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireVariable, QuestionnaireVerificationReferenceProperty.VariableLabel);
                    }

                    if (MatchesSearchTerm(questionnaireVariable.Expression, searchRegex))
                    {
                        yield return QuestionnaireEntityReference.CreateFrom(questionnaireVariable, QuestionnaireVerificationReferenceProperty.VariableContent);
                    }
                }

                if (questionnaireItem is IStaticText staticText && MatchesSearchTerm(staticText.AttachmentName, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateFrom(staticText, QuestionnaireVerificationReferenceProperty.AttachmentName);
                }
            }

            foreach (var macro in questionnaire.Macros.OrderBy(x => x.Value.Name))
            {
                if (MatchesSearchTerm(macro.Value.Content, searchRegex))
                {
                    yield return QuestionnaireEntityReference.CreateForMacro(macro.Key);
                }
            }
        }

        private static Regex BuildSearchRegex(string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (!matchCase)
            {
                options |= RegexOptions.IgnoreCase;
            }
            string encodedSearchPattern = useRegex ? searchFor : Regex.Escape(searchFor);
            string pattern = matchWholeWord ? $@"\b{encodedSearchPattern}\b" : encodedSearchPattern;

            Regex searchRegex = new Regex(pattern, options);
            return searchRegex;
        }

        private static bool MatchesSearchTerm(string? target, Regex searchRegex)
        {
            if (target.IsNullOrEmpty()) return false;

            return searchRegex.IsMatch(target);
        }

        private static string ReplaceUsingSearchTerm(string? target, Regex searchFor, string? replaceWith)
        {
            return searchFor.Replace(target ?? String.Empty, replaceWith?? string.Empty);
        }

        public int ReplaceTexts(QuestionnaireRevision questionnaireId, Guid responsibleId, string searchFor, string replaceWith,
            bool matchCase, bool matchWholeWord, bool useRegex)
        {
            var questionnaire = this.questionnaires.Get(questionnaireId);
            if(questionnaire == null)
                throw new InvalidOperationException("Questionnaire was not found.");
            return ReplaceTexts(questionnaire, responsibleId, searchFor, replaceWith, matchCase, matchWholeWord, useRegex);
        }

        public int ReplaceTexts(QuestionnaireDocument questionnaire, Guid responsibleId, string searchFor, string replaceWith, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            int affectedByReplaceEntries = 0;

            var allEntries = questionnaire.Children.TreeToEnumerable(x => x.Children);
            var searchRegex = BuildSearchRegex(searchFor, matchCase, matchWholeWord, useRegex);
            foreach (var questionnaireItem in allEntries)
            {
                bool replacedAny = false;
                var title = questionnaireItem.GetTitle();
                if (MatchesSearchTerm(title, searchRegex))
                {
                    replacedAny = true;
                    questionnaireItem.SetTitle(ReplaceUsingSearchTerm(title, searchRegex, replaceWith));
                }

                var variableName = questionnaireItem.GetVariable();
                if (MatchesSearchTerm(variableName, searchRegex))
                {
                    replacedAny = true;
                    questionnaireItem.SetVariable(ReplaceUsingSearchTerm(variableName, searchRegex, replaceWith));
                }

                if (questionnaireItem is IConditional conditional && MatchesSearchTerm(conditional.ConditionExpression, searchRegex))
                {
                    replacedAny = true;
                    string newCondition = ReplaceUsingSearchTerm(conditional.ConditionExpression ?? string.Empty, searchRegex, replaceWith);
                    conditional.ConditionExpression = newCondition;
                }

                if (questionnaireItem is IValidatable validatable)
                {
                    foreach (var validationCondition in validatable.ValidationConditions)
                    {
                        if (MatchesSearchTerm(validationCondition.Expression, searchRegex))
                        {
                            replacedAny = true;
                            string newValidationCondition = ReplaceUsingSearchTerm(validationCondition.Expression, searchRegex, replaceWith);
                            validationCondition.Expression = newValidationCondition;
                        }
                        if (MatchesSearchTerm(validationCondition.Message, searchRegex))
                        {
                            replacedAny = true;
                            string newMessage = ReplaceUsingSearchTerm(validationCondition.Message, searchRegex, replaceWith);
                            validationCondition.Message = newMessage;
                        }
                    }
                }

                if (questionnaireItem is IVariable questionnaireVariable)
                {
                    if (MatchesSearchTerm(questionnaireVariable.Label, searchRegex))
                    {
                        replacedAny = true;
                        questionnaireVariable.Label = ReplaceUsingSearchTerm(questionnaireVariable.Label, searchRegex, replaceWith);
                    }

                    if (MatchesSearchTerm(questionnaireVariable.Expression, searchRegex))
                    {
                        replacedAny = true;
                        questionnaireVariable.Expression = ReplaceUsingSearchTerm(questionnaireVariable.Expression, searchRegex, replaceWith);
                    }
                }

                if (questionnaireItem is IQuestion question)
                {
                    if (question.Answers != null && !(question.IsFilteredCombobox.GetValueOrDefault() || question.CascadeFromQuestionId.HasValue))
                    {
                        foreach (var questionAnswer in question.Answers)
                        {
                            if (MatchesSearchTerm(questionAnswer.AnswerText, searchRegex))
                            {
                                replacedAny = true;
                                questionAnswer.AnswerText = ReplaceUsingSearchTerm(questionAnswer.AnswerText, searchRegex, replaceWith);
                            }
                        }
                    }

                    if (question.Properties?.OptionsFilterExpression != null && MatchesSearchTerm(question.Properties?.OptionsFilterExpression, searchRegex))
                    {
                        replacedAny = true;
                        question.Properties!.OptionsFilterExpression = ReplaceUsingSearchTerm(question.Properties.OptionsFilterExpression, searchRegex, replaceWith);
                    }

                    if (MatchesSearchTerm(question.LinkedFilterExpression, searchRegex))
                    {
                        replacedAny = true;
                        question.LinkedFilterExpression = ReplaceUsingSearchTerm(question.LinkedFilterExpression, searchRegex, replaceWith);
                    }

                    if (MatchesSearchTerm(question.Instructions, searchRegex))
                    {
                        replacedAny = true;
                        question.Instructions = ReplaceUsingSearchTerm(question.Instructions, searchRegex, replaceWith);
                    }
                }

                if (questionnaireItem is IGroup @group)
                {
                    if (group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    {
                        foreach (var fixedRosterTitle in group.FixedRosterTitles)
                        {
                            if (MatchesSearchTerm(fixedRosterTitle.Title, searchRegex))
                            {
                                replacedAny = true;
                                fixedRosterTitle.Title = ReplaceUsingSearchTerm(fixedRosterTitle.Title, searchRegex, replaceWith);
                            }
                        }
                    }
                }

                if (questionnaireItem is IStaticText staticText)
                {
                    if (MatchesSearchTerm(staticText.AttachmentName, searchRegex))
                    {
                        replacedAny = true;
                        staticText.AttachmentName = ReplaceUsingSearchTerm(staticText.AttachmentName, searchRegex, replaceWith);
                    }
                }

                if (replacedAny)
                {
                    affectedByReplaceEntries++;
                }
            }

            foreach (var macro in questionnaire.Macros.Values)
            {
                if (MatchesSearchTerm(macro.Content, searchRegex))
                {
                    affectedByReplaceEntries++;
                    macro.Content = ReplaceUsingSearchTerm(macro.Content, searchRegex, replaceWith);
                }
            }

            return affectedByReplaceEntries;
        }
    }
}
