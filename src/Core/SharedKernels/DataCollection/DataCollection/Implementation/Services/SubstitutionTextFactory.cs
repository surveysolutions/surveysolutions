using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class SubstitutionTextFactory : ISubstitutionTextFactory
    {
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        public SubstitutionTextFactory() { }

        public SubstitutionTextFactory(
            ISubstitutionService substitutionService, 
            IVariableToUIStringService variableToUiStringService)
        {
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
        }

        public SubstitutionText CreateText(Identity identity, string text, IQuestionnaire questionnaire)
        {
            string[] variableNames = this.substitutionService.GetAllSubstitutionVariableNames(text);

            var substitutionVariables = new List<SubstitutionVariable>();

            foreach (var variable in variableNames)
            {
                if (questionnaire.HasQuestion(variable)) substitutionVariables.Add(new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetQuestionIdByVariable(variable).Value
                });

                if (questionnaire.HasVariable(variable)) substitutionVariables.Add(new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetVariableIdByVariableName(variable)
                });

                if (questionnaire.HasRoster(variable)) substitutionVariables.Add(new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetGroupIdByVariableName(variable)
                });
            }

            if (this.substitutionService.ContainsRosterTitle(text))
            {
                substitutionVariables.Add(new SubstitutionVariable
                {
                    Name = this.substitutionService.RosterTitleSubstitutionReference,
                    Id = questionnaire.GetRostersFromTopToSpecifiedEntity(identity.Id).Last()
                });
            }

            return new SubstitutionText(identity, text, substitutionVariables, this.substitutionService, this.variableToUiStringService);
        }
    }
}