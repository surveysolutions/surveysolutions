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
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        private readonly List<SubstitutionVariable> substitutionVariables;
        private InterviewTree tree;

        public SubstitutionText()
        {
        }

        public SubstitutionText(
            Identity identity,
            string text,
            List<SubstitutionVariable> variables,
            ISubstitutionService substitutionService,
            IVariableToUIStringService variableToUiStringService)
        {
            this.Text = text;
            this.BrowserReadyText = text;
            this.originalText = text;
            this.identity = identity;
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
            this.substitutionVariables = variables;
        }

        public void SetTree(InterviewTree interviewTree)
        {
            this.tree = interviewTree;
        }

        private readonly string originalText;
        public string Text { get; private set; }
        public string BrowserReadyText { get; private set; }

        public bool HasSubstitutions => this.substitutionVariables?.Count > 0;
        
        public void ReplaceSubstitutions()
        {
            if (!this.HasSubstitutions)
                return;

            this.Text = GetTextWithReplacedSubstitutions(false);
            this.BrowserReadyText = GetTextWithReplacedSubstitutions(shouldAddBrowserTags: true);
        }

        private string GetTextWithReplacedSubstitutions(bool shouldAddBrowserTags)
        {
            var textWithReplacedSubstitutions = this.originalText;

            foreach (var substitution in this.substitutionVariables)
            {
                string substitutionResult = null;
                bool shouldEncode = true;

                switch (this.tree.FindEntityInQuestionBranch(substitution.Id, this.identity))
                {
                    case InterviewTreeRoster roster:
                        substitutionResult = roster.RosterTitle;
                        break;

                    case InterviewTreeVariable variable:
                        substitutionResult = this.variableToUiStringService.VariableToUIString(variable.Value);
                        break;


                    case InterviewTreeQuestion question:
                        string answerString = question.GetAnswerAsString(CultureInfo.CurrentCulture);
                        shouldEncode = false;

                        substitutionResult = shouldAddBrowserTags ? WebUtility.HtmlEncode(answerString) : answerString;

                        if (shouldAddBrowserTags && question.IsDateTime && question.IsAnswered())
                        {
                            var asDateTime = question.GetAsInterviewTreeDateTimeQuestion();
                            var dateTime = asDateTime.GetAnswer().Value;

                            substitutionResult = asDateTime.IsTimestamp 
                                ? $"<time datetime=\"{dateTime:s}Z\">{dateTime.ToLocalTime().ToString(asDateTime.UiFormatString)}</time>" 
                                : $"<time date=\"{dateTime:yyyy-MM-dd}\">{dateTime.ToString(asDateTime.UiFormatString)}</time>";
                        }
                        
                        break;
                    case null:
                        break;
                }

                substitutionResult = shouldAddBrowserTags && shouldEncode 
                    ? WebUtility.HtmlEncode(substitutionResult) 
                    : substitutionResult;

                substitutionResult = string.IsNullOrEmpty(substitutionResult)
                    ? this.substitutionService.DefaultSubstitutionText
                    : substitutionResult;

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(
                        textWithReplacedSubstitutions, substitution.Name, substitutionResult);
            }

            return textWithReplacedSubstitutions;
        }

        public override string ToString() => this.Text;

        public SubstitutionText Clone() => (SubstitutionText) MemberwiseClone();
    }
}