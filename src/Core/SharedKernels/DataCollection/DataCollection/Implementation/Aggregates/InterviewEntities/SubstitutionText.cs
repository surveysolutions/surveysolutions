using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class SubstitutionText
    {
        private readonly Identity identity;
        private readonly string selfVariable;
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        protected internal readonly List<SubstitutionVariable> substitutionVariables;

        public SubstitutionText()
        {
        }

        public SubstitutionText(
            Identity identity,
            string text,
            string selfVariable,
            List<SubstitutionVariable> variables,
            ISubstitutionService substitutionService,
            IVariableToUIStringService variableToUiStringService)
        {
            // this constructor is one of the most used constructors during interview progress
            // do not add any cpu/memory intensive operations
            this.Text = text;
            this.BrowserReadyText = text;
            this.originalText = text;
            this.identity = identity;
            this.selfVariable = selfVariable;
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
            this.substitutionVariables = variables;
        }

        private readonly string originalText;
        public string Text { get; private set; }
        public string BrowserReadyText { get; private set; }

        public bool HasSubstitutions => this.substitutionVariables?.Count > 0;

        public void ReplaceSubstitutions(InterviewTree tree)
        {
            if (!this.HasSubstitutions)
                return;

            var plainTextReplaced = this.originalText;
            var browserTextReplaced = this.originalText;

            foreach (var substitution in this.substitutionVariables)
            {
                string plainSubstitutionResult = null;
                string browserSubstitutionResult = null;
                bool shouldEncode = true;

                var treeEntity = tree.FindEntityInQuestionBranch(substitution.Id, this.identity);
                if (treeEntity != null && !treeEntity.IsDisabled())
                {
                    switch (treeEntity)
                    {
                        case InterviewTreeRoster roster:
                            plainSubstitutionResult = roster.RosterTitle;
                            browserSubstitutionResult = plainSubstitutionResult;
                            break;

                        case InterviewTreeVariable variable:
                            plainSubstitutionResult = this.variableToUiStringService.VariableToUIString(variable.Value);
                            browserSubstitutionResult = plainSubstitutionResult;
                            break;

                        case InterviewTreeQuestion question:
                            string answerString = question.GetAnswerAsString(CultureInfo.CurrentCulture);
                            shouldEncode = false;

                            plainSubstitutionResult = SubstitutionResult(false);
                            browserSubstitutionResult = SubstitutionResult(true);

                            string SubstitutionResult(bool shouldAddBrowserTags)
                            {
                                string result;

                                if (shouldAddBrowserTags && question.IsDateTime && question.IsAnswered())
                                {
                                    var asDateTime = question.GetAsInterviewTreeDateTimeQuestion();
                                    var dateTime = asDateTime.GetAnswer().Value;

                                    result = asDateTime.IsTimestamp
                                        ? $"<time datetime=\"{dateTime:O}\">{dateTime.ToString(asDateTime.UiFormatString)}</time>"
                                        : $"<time date=\"{dateTime:yyyy-MM-dd}\">{dateTime.ToString(asDateTime.UiFormatString)}</time>";
                                }
                                else
                                {
                                    result = shouldAddBrowserTags ? WebUtility.HtmlEncode(answerString ?? string.Empty) : answerString;
                                }

                                return result;
                            }

                            break;
                    }
                }
                
                plainSubstitutionResult = string.IsNullOrEmpty(plainSubstitutionResult)
                    ? this.substitutionService.DefaultSubstitutionText
                    : plainSubstitutionResult;

                browserSubstitutionResult = shouldEncode
                    ? WebUtility.HtmlEncode(browserSubstitutionResult) // DO NOT CHANGE TO HttpUtility.HtmlEncode: KP-10869
                    : browserSubstitutionResult;

                browserSubstitutionResult = string.IsNullOrEmpty(browserSubstitutionResult)
                    ? this.substitutionService.DefaultSubstitutionText
                    : browserSubstitutionResult;

                plainTextReplaced = this.substitutionService.ReplaceSubstitutionVariable(
                        plainTextReplaced, this.selfVariable, substitution.Name, plainSubstitutionResult);

                browserTextReplaced = this.substitutionService.ReplaceSubstitutionVariable(
                    browserTextReplaced, this.selfVariable, substitution.Name, browserSubstitutionResult);
            }

            Text = plainTextReplaced;
            BrowserReadyText = browserTextReplaced;
        }

        public override string ToString() => this.Text;

        public SubstitutionText Clone() => (SubstitutionText)MemberwiseClone();
    }
}
