using System.Diagnostics;
using System.Linq;
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

        public bool HasSubstitutions => this.substitutionVariables!= null &&
                (this.substitutionVariables.ByRosters.Any() ||
                this.substitutionVariables.ByVariables.Any() || 
                this.substitutionVariables.ByQuestions.Any());


        public void ReplaceSubstitutions()
        {
            if (!HasSubstitutions)
            {
                return;
            }

            var textWithReplacedSubstitutions = this.originalText;
            foreach (var substitution in this.substitutionVariables.ByRosters)
            {
                var roster = this.tree.FindEntityInQuestionBranch(substitution.Id, identity) as InterviewTreeRoster;
                var rosterTitle = string.IsNullOrEmpty(roster?.RosterTitle) 
                    ? this.substitutionService.DefaultSubstitutionText 
                    : roster.RosterTitle;
                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions, substitution.Name, rosterTitle);
            }

            foreach (var substitution in this.substitutionVariables.ByVariables)
            {
                var variable = this.tree.FindEntityInQuestionBranch(substitution.Id, identity) as InterviewTreeVariable;
                var variableValueAsString = this.variableToUiStringService.VariableToUIString(variable?.Value);
                variableValueAsString = string.IsNullOrEmpty(variableValueAsString)
                    ? this.substitutionService.DefaultSubstitutionText
                    : variableValueAsString;

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions, substitution.Name, variableValueAsString);
            }

            foreach (var substitution in this.substitutionVariables.ByQuestions)
            {
                var question = this.tree.FindEntityInQuestionBranch(substitution.Id, identity) as InterviewTreeQuestion;
                string answerString = question?.GetAnswerAsString();
                answerString = string.IsNullOrEmpty(answerString)
                    ? this.substitutionService.DefaultSubstitutionText
                    : answerString;

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions, substitution.Name, answerString);
            }

            this.Text = textWithReplacedSubstitutions;
        }

        public override string ToString()
        {
            return this.Text;
        }

        public SubstitionText Clone()
        {
            var clone = (SubstitionText)this.MemberwiseClone();
            return clone;
        }
    }
}