using System;
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

        public SubstitutionTextFactory(
            ISubstitutionService substitutionService, 
            IVariableToUIStringService variableToUiStringService)
        {
            this.substitutionService = substitutionService ?? throw new ArgumentNullException(nameof(substitutionService));
            this.variableToUiStringService = variableToUiStringService ?? throw new ArgumentNullException(nameof(variableToUiStringService));
        }

        public SubstitutionText CreateText(Identity identity, string text, IQuestionnaire questionnaire)
        {
            var entityVariable = questionnaire.GetEntityVariableOrThrow(identity.Id);
            string[] variableNames = this.substitutionService.GetAllSubstitutionVariableNames(text, entityVariable);

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
                    Id = questionnaire.GetRosterIdByVariableName(variable).Value
                });
            }

            if (this.substitutionService.ContainsRosterTitle(text))
            {
                if (questionnaire.IsRosterGroup(identity.Id))
                {
                    substitutionVariables.Add(new SubstitutionVariable
                    {
                        Name = this.substitutionService.RosterTitleSubstitutionReference,
                        Id = identity.Id
                    });
                }
                else
                {
                    var rostersFromTop = questionnaire.GetRostersFromTopToSpecifiedEntity(identity.Id).ToList();
                    substitutionVariables.Add(new SubstitutionVariable
                    {
                        Name = this.substitutionService.RosterTitleSubstitutionReference,
                        Id = rostersFromTop.Count > 0 ? rostersFromTop[rostersFromTop.Count - 1] : identity.Id
                    });
                }
            }
            
            return new SubstitutionText(identity, 
                text == null ? null : questionnaire.ApplyMarkDownTransformation(text) ?? text, 
                entityVariable,
                substitutionVariables, 
                this.substitutionService, 
                this.variableToUiStringService);
        }
    }
}
