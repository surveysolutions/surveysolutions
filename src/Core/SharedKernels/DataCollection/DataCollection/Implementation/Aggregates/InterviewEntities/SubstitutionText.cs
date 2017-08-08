using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        private readonly SubstitutionVariables substitutionVariables;
        private InterviewTree tree;

        public SubstitutionText()
        {
        }

        public SubstitutionText(
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
                (this.substitutionVariables.ByRosters.Count > 0 ||
                this.substitutionVariables.ByVariables.Count > 0 ||
                this.substitutionVariables.ByQuestions.Count > 0);
        
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
                        shouldAddBrowserTags ? WebUtility.HtmlEncode(rosterTitle) : rosterTitle);
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
                        shouldAddBrowserTags ? WebUtility.HtmlEncode(variableValueAsString) : variableValueAsString);
            }

            foreach (var substitution in this.substitutionVariables.ByQuestions)
            {
                var question = this.tree.FindEntityInQuestionBranch(substitution.Id, this.identity) as InterviewTreeQuestion;
                string answerString = question?.GetAnswerAsString(CultureInfo.CurrentCulture);

                answerString = string.IsNullOrEmpty(answerString)
                    ? this.substitutionService.DefaultSubstitutionText
                    : answerString;

                var htmlReadyAnswer = shouldAddBrowserTags ? WebUtility.HtmlEncode(answerString) : answerString;

                if (shouldAddBrowserTags && question?.AsDateTime?.IsAnswered == true)
                {
                    var asDateTime = question.AsDateTime;
                    var dateTime = asDateTime.GetAnswer().Value;
                    if (asDateTime.IsTimestamp)
                    {
                        htmlReadyAnswer = $"<time datetime=\"{dateTime:s}Z\">{dateTime.ToLocalTime().ToString(asDateTime.UiFormatString)}</time>";
                    }
                    else
                    {
                        htmlReadyAnswer = $"<time date=\"{dateTime:yyyy-MM-dd}\">{dateTime.ToString(asDateTime.UiFormatString)}</time>";
                    }
                }

                textWithReplacedSubstitutions =
                    this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions,
                        substitution.Name,
                        htmlReadyAnswer);
            }

            return textWithReplacedSubstitutions;
        }

        public override string ToString() => this.Text;

        public SubstitutionText Clone() => (SubstitutionText) this.MemberwiseClone();
    }
}