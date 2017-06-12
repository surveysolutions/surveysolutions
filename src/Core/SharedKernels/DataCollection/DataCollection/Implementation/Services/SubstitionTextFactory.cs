using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class SubstitionTextFactory : ISubstitionTextFactory
    {
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        public SubstitionTextFactory() { }

        public SubstitionTextFactory(
            ISubstitutionService substitutionService, 
            IVariableToUIStringService variableToUiStringService)
        {
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
        }

        public SubstitionText CreateText(Identity identity, string text, IQuestionnaire questionnaire)
        {
            string[] variableNames = this.substitutionService.GetAllSubstitutionVariableNames(text);

            var substitutionVariables = new SubstitutionVariables();

            foreach (var variable in variableNames)
            {
                if (questionnaire.HasQuestion(variable)) substitutionVariables.ByQuestions.Add(new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetQuestionIdByVariable(variable).Value
                });

                if (questionnaire.HasVariable(variable)) substitutionVariables.ByVariables.Add(new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetVariableIdByVariableName(variable)
                });

                if (questionnaire.HasRoster(variable)) substitutionVariables.ByRosters.Add(new SubstitutionVariable
                {
                    Name = variable,
                    Id = questionnaire.GetGroupIdByVariableName(variable)
                });
            }

            if (this.substitutionService.ContainsRosterTitle(text))
            {
                substitutionVariables.ByRosters.Add(new SubstitutionVariable
                {
                    Name = this.substitutionService.RosterTitleSubstitutionReference,
                    Id = questionnaire.GetRostersFromTopToSpecifiedEntity(identity.Id).Last()
                });
            }

            return new SubstitionText(identity, text, substitutionVariables, this.substitutionService, this.variableToUiStringService);
        }
    }
}