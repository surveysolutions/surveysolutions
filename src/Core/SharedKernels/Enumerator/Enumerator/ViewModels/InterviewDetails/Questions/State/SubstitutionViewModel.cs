using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class SubstitutionViewModel
    {
        private SubstitutionVariables substitutionVariables;

        private class SubstitutionVariables
        {
            public IEnumerable<SubstitutionVariable> ByQuestions { get; set; }
            public IEnumerable<SubstitutionVariable> ByVariables { get; set; }
        }
        private class SubstitutionVariable
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        private readonly IRosterTitleSubstitutionService rosterTitleSubstitutionService;
        private string interviewId;
        private string textWithSubstitutions;
        private Identity entityIdentity;

        public SubstitutionViewModel(
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ISubstitutionService substitutionService,
            IVariableToUIStringService variableToUiStringService,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
            this.rosterTitleSubstitutionService = rosterTitleSubstitutionService;
        }

        public void Init(string interviewId, Identity entityIdentity, string textWithSubstitutions)
        {
            this.interviewId = interviewId;
            this.entityIdentity = entityIdentity;

            this.SetText(textWithSubstitutions ?? string.Empty);
        }

        private void SetText(string textWithSubstitutions)
        {
            this.textWithSubstitutions = textWithSubstitutions;

            this.substitutionVariables = this.GetSubstitutionVariables();
        }

        public void ChangeText(string textWithSubstitutions) => this.SetText(textWithSubstitutions);

        public bool HasVariablesInText(IEnumerable<Identity> variableIdentities)
        {
            var changedVariables = this.substitutionVariables.ByVariables.Where(
                substitution => variableIdentities.Any(variable => variable.Id == substitution.Id));

            return changedVariables.Any();
        }

        private SubstitutionVariables GetSubstitutionVariables()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            string[] variableNames = this.substitutionService.GetAllSubstitutionVariableNames(this.textWithSubstitutions);

            return new SubstitutionVariables
            {
                ByQuestions = variableNames.Where(questionnaire.HasQuestion).Select(variable => new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetQuestionIdByVariable(variable)
                }).ToList(),
                ByVariables = variableNames.Where(questionnaire.HasVariable).Select(x => new SubstitutionVariable
                {
                    Name = x,
                    Id = questionnaire.GetVariableIdByVariableName(x)
                }).ToList()
            };
        }


        public string ReplaceSubstitutions()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var textWithReplacedSubstitutions = this.textWithSubstitutions;

            if (this.substitutionService.ContainsRosterTitle(textWithReplacedSubstitutions))
            {
                textWithReplacedSubstitutions = this.rosterTitleSubstitutionService.Substitute(textWithReplacedSubstitutions,
                    this.entityIdentity, this.interviewId);
            }

            foreach (var substitution in this.substitutionVariables.ByVariables)
            {
                var variableValue = interview.GetVariableValueByOrDeeperRosterLevel(substitution.Id, this.entityIdentity.RosterVector);
                var variableValueAsString = this.variableToUiStringService.VariableToUIString(variableValue);

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(
                    textWithReplacedSubstitutions, substitution.Name,
                    string.IsNullOrEmpty(variableValueAsString) ? this.substitutionService.DefaultSubstitutionText : variableValueAsString);
            }

            foreach (var substitution in this.substitutionVariables.ByQuestions)
            {
                string answerString = interview.GetAnswerAsString(Identity.Create(substitution.Id, this.entityIdentity.RosterVector));

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(
                    textWithReplacedSubstitutions, substitution.Name, string.IsNullOrEmpty(answerString) ? this.substitutionService.DefaultSubstitutionText : answerString);
            }

            return textWithReplacedSubstitutions;
        }
    }
}