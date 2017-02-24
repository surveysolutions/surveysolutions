using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class SubstitionText
    {
        private readonly Identity identity;
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        private readonly SubstitutionVariables substitutionVariables;
        private InterviewTree tree;

        public SubstitionText()
        {
        }

        public SubstitionText(
            Identity identity,
            string text,
            SubstitutionVariables variables,
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

        public bool HasSubstitutions => this.substitutionVariables!= null &&
                (this.substitutionVariables.ByRosters.Any() ||
                this.substitutionVariables.ByVariables.Any() || 
                this.substitutionVariables.ByQuestions.Any());


        public void ReplaceSubstitutions()
        {
            if (!this.HasSubstitutions)
                return;

            this.Text = this.GetTextWithReplacedSubstitutions(shouldAddBrowserTags: false);
            this.BrowserReadyText = this.GetTextWithReplacedSubstitutions(shouldAddBrowserTags: true);
        }

        private string GetTextWithReplacedSubstitutions(bool shouldAddBrowserTags)
        {
            var textWithReplacedSubstitutions = this.originalText;

            foreach (var substitution in this.substitutionVariables.ByRosters)
            {
                var roster = this.tree.FindEntityInQuestionBranch(substitution.Id, this.identity) as InterviewTreeRoster;
                var rosterTitle = string.IsNullOrEmpty(roster?.RosterTitle)
                    ? this.substitutionService.DefaultSubstitutionText
                    : roster.RosterTitle;
                textWithReplacedSubstitutions =
                    this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions,
                        substitution.Name,
                        WebUtility.HtmlEncode(rosterTitle));
            }

            foreach (var substitution in this.substitutionVariables.ByVariables)
            {
                var variable = this.tree.FindEntityInQuestionBranch(substitution.Id, this.identity) as InterviewTreeVariable;
                var variableValueAsString = this.variableToUiStringService.VariableToUIString(variable?.Value);
                variableValueAsString = string.IsNullOrEmpty(variableValueAsString)
                    ? this.substitutionService.DefaultSubstitutionText
                    : variableValueAsString;

                textWithReplacedSubstitutions =
                    this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions,
                        substitution.Name,
                        WebUtility.HtmlEncode(variableValueAsString));
            }

            foreach (var substitution in this.substitutionVariables.ByQuestions)
            {
                var question = this.tree.FindEntityInQuestionBranch(substitution.Id, this.identity) as InterviewTreeQuestion;
                string answerString = question?.GetAnswerAsString(CultureInfo.CurrentCulture);

                answerString = string.IsNullOrEmpty(answerString)
                    ? this.substitutionService.DefaultSubstitutionText
                    : answerString;

                var htmlReadyAnswer = WebUtility.HtmlEncode(answerString);

                if (shouldAddBrowserTags && question?.AsDateTime?.IsAnswered == true)
                {
                    var dateTime = question.AsDateTime.GetAnswer().Value;
                    htmlReadyAnswer = $"<time datetime=\"{dateTime:o}\">{htmlReadyAnswer}</time>";
                }

                textWithReplacedSubstitutions =
                    this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions,
                        substitution.Name,
                        htmlReadyAnswer);
            }

            return textWithReplacedSubstitutions;
        }

        public override string ToString() => this.Text;

        public SubstitionText Clone() => (SubstitionText) this.MemberwiseClone();
    }
}