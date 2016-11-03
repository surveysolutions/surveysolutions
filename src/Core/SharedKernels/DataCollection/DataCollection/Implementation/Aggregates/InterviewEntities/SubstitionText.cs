using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class SubstitionText
    {
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        private readonly SubstitutionVariables substitutionVariables;
        private InterviewTree tree;

        public SubstitionText() { }

        public SubstitionText(
            string text, 
            SubstitutionVariables variables, 
            ISubstitutionService substitutionService, 
            IVariableToUIStringService variableToUiStringService)
        {
            this.Text = text;
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
            this.substitutionVariables = variables;
        }

        public void SetTree(InterviewTree interviewTree)
        {
            this.tree = interviewTree;
        }

        public string Text { get; private set; }

        public void ReplaceSubstitutions()
        {
            if (this.substitutionVariables == null ||
                (!this.substitutionVariables.ByRosters.Any() && !this.substitutionVariables.ByVariables.Any() &&
                 !this.substitutionVariables.ByQuestions.Any()))
            {
                return;
            }

            var textWithReplacedSubstitutions = this.Text;
            foreach (var substitution in this.substitutionVariables.ByRosters)
            {
                var roster = this.tree.GetRoster(substitution.Id);
                var rosterTitle = string.IsNullOrEmpty(roster.RosterTitle) 
                    ? this.substitutionService.DefaultSubstitutionText 
                    : roster.RosterTitle;
                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions, substitution.Name, rosterTitle);
            }

            foreach (var substitution in this.substitutionVariables.ByVariables)
            {
                var variableValue = this.tree.GetVariable(substitution.Id);
                var variableValueAsString = this.variableToUiStringService.VariableToUIString(variableValue);
                variableValueAsString = string.IsNullOrEmpty(variableValueAsString)
                    ? this.substitutionService.DefaultSubstitutionText
                    : variableValueAsString;

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(textWithReplacedSubstitutions, substitution.Name, variableValueAsString);
            }

            foreach (var substitution in this.substitutionVariables.ByQuestions)
            {
                var baseInterviewAnswer = this.tree.GetQuestion(substitution.Id);
                string answerString = baseInterviewAnswer.GetAnswerAsString();
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

        public SubstitionText Clone(InterviewTree interviewTree)
        {
            var clone = (SubstitionText)this.MemberwiseClone();
            clone.SetTree(interviewTree);
            return clone;
        }
    }

    public class SubstitutionVariables
    {
        public IEnumerable<SubstitutionVariable> ByQuestions { get; set; }
        public IEnumerable<SubstitutionVariable> ByVariables { get; set; }
        public IEnumerable<SubstitutionVariable> ByRosters { get; set; }
    }


    public class SubstitutionVariable
    {
        public Identity Id { get; set; }
        public string Name { get; set; }
    }

}