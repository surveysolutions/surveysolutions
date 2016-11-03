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

        public SubstitionText CreateText(Identity identity, string text, IQuestionnaire questionnaire, InterviewTree tree)
        {
            string[] variableNames = this.substitutionService.GetAllSubstitutionVariableNames(text);

            var substitutionVariables = new SubstitutionVariables
            {
                ByQuestions = variableNames.Where(questionnaire.HasQuestion).Select(variable => new SubstitutionVariable
                {
                    Name = variable,
                    Id = new Identity(questionnaire.GetQuestionIdByVariable(variable), RosterVector.Empty)
                }).ToList(),
                ByVariables = variableNames.Where(questionnaire.HasVariable).Select(x => new SubstitutionVariable
                {
                    Name = x,
                    Id = new Identity(questionnaire.GetVariableIdByVariableName(x), RosterVector.Empty)
                }).ToList(),
                ByRosters = new List<SubstitutionVariable> { new SubstitutionVariable
                {
                    Name = this.substitutionService.RosterTitleSubstitutionReference,
                    Id = new Identity(Guid.Empty, RosterVector.Empty)
                }}
            };
            return new SubstitionText(text, substitutionVariables, this.substitutionService, this.variableToUiStringService);
        }
    }
}