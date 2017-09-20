using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class ExpressionsGraphProvider : IExpressionsGraphProvider
    {
        private readonly IExpressionProcessor expressionProcessor;
        private readonly IMacrosSubstitutionService macrosSubstitutionService;

        public ExpressionsGraphProvider(
            IExpressionProcessor expressionProcessor,
            IMacrosSubstitutionService macrosSubstitutionService)
        {
            this.expressionProcessor = expressionProcessor;
            this.macrosSubstitutionService = macrosSubstitutionService;
        }

        public Dictionary<Guid, List<Guid>> BuildDependencyGraph(ReadOnlyQuestionnaireDocument questionnaire)
        {
            Dictionary<Guid, List<Guid>> conditionalDependencies = BuildConditionalDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> structuralDependencies = BuildStructuralDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> rosterDependencies = BuildRosterDependencies(questionnaire);
            Dictionary<Guid, Guid> linkedQuestionByRosterDependencies = BuildLinkedQuestionByRosterDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> susbtitutionDependencies = BuildSubstitutionDependencies(questionnaire);

            var mergedDependencies = new Dictionary<Guid, List<Guid>>();

            IEnumerable<Guid> allIdsInvolvedInExpressions =
                structuralDependencies.Keys
                    .Union(conditionalDependencies.Keys)
                    .Union(rosterDependencies.Keys)
                    .Union(linkedQuestionByRosterDependencies.Keys)
                    .Union(susbtitutionDependencies.Keys)
                    .Union(linkedQuestionByRosterDependencies.Values)
                    .Union(structuralDependencies.SelectMany(x => x.Value))
                    .Union(conditionalDependencies.SelectMany(x => x.Value))
                    .Union(rosterDependencies.SelectMany(x => x.Value))
                    .Union(susbtitutionDependencies.SelectMany(x => x.Value))
                    .Distinct();

            allIdsInvolvedInExpressions.ForEach(x => mergedDependencies.Add(x, new List<Guid>()));

            foreach (var x in structuralDependencies)
            {
                mergedDependencies[x.Key].AddRange(x.Value);
            }

            foreach (var conditionalDependency in conditionalDependencies)
            {
                foreach (var dependency in conditionalDependency.Value)
                {
                    if (!mergedDependencies[dependency].Contains(conditionalDependency.Key))
                    {
                        mergedDependencies[dependency].Add(conditionalDependency.Key);
                    }
                }
            }

            foreach (var rosterDependency in rosterDependencies)
            {
                foreach (var dependency in rosterDependency.Value)
                {
                    if (!mergedDependencies[rosterDependency.Key].Contains(dependency))
                    {
                        mergedDependencies[rosterDependency.Key].Add(dependency);
                    }
                }
            }

            foreach (var linkedDependency in linkedQuestionByRosterDependencies)
            {
                if (!mergedDependencies[linkedDependency.Value].Contains(linkedDependency.Key))
                {
                    mergedDependencies[linkedDependency.Value].Add(linkedDependency.Key);
                }
            }
            return mergedDependencies;
        }


        private Dictionary<Guid, Guid> BuildLinkedQuestionByRosterDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Find<IQuestion>(x => x.LinkedToQuestionId != null || x.LinkedToRosterId != null)
                .ToDictionary(x => x.PublicKey, x => x.LinkedToQuestionId ?? x.LinkedToRosterId.Value);
        }

        private Dictionary<Guid, List<Guid>> BuildStructuralDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Find<IGroup>() //GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());
        }

        private Dictionary<Guid, List<Guid>> BuildSubstitutionDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return new Dictionary<Guid, List<Guid>>();
        }

        private Dictionary<Guid, List<Guid>> BuildRosterDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var rosterDependencies =
                questionnaire.Find<Group>(x => x.IsRoster && x.RosterSizeSource == RosterSizeSourceType.Question)
                    .Where(x => x.RosterSizeQuestionId.HasValue)
                    .Select(x => new { Key = x.RosterSizeQuestionId.Value, Value = x.PublicKey })
                    .Union(questionnaire
                        .Find<Group>(x => x.IsRoster && x.RosterSizeSource == RosterSizeSourceType.Question)
                        .Where(x => x.RosterTitleQuestionId.HasValue)
                        .Select(x => new { Key = x.RosterTitleQuestionId.Value, Value = x.PublicKey }))
                    .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Select(r => r.Value).ToList());

            return rosterDependencies;
        }

        public Dictionary<Guid, List<Guid>> BuildConditionalDependencies(ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            var dependencies = new Dictionary<Guid, List<Guid>>();

            var allMacroses = questionnaireDocument.Macros.Values;
            var variableNamesByEntitiyIds = GetAllVariableNames(questionnaireDocument);

            foreach (var entity in questionnaireDocument.Find<IComposite>())
            {
                if (entity is IConditional conditionalEntity)
                {
                    FillDependencies(entity.PublicKey, conditionalEntity.ConditionExpression);
                }

                if (entity is IValidatable validatableEntity)
                {
                    foreach (var validationCondition in validatableEntity.ValidationConditions)
                    {
                        FillDependencies(entity.PublicKey, validationCondition.Expression);
                    }
                }

                if (entity is IQuestion question)
                {
                    FillDependencies(question.PublicKey, question.Properties.OptionsFilterExpression);

                    if (question.CascadeFromQuestionId != null)
                    {
                        if (dependencies.ContainsKey(entity.PublicKey))
                            dependencies[entity.PublicKey].Add(question.CascadeFromQuestionId.Value);
                        else
                            dependencies.Add(entity.PublicKey, new List<Guid> { question.CascadeFromQuestionId.Value });
                    }
                }

                if (entity is IVariable variable)
                {
                    FillDependencies(entity.PublicKey, variable.Expression);
                }
            }

            return dependencies;

            void FillDependencies(Guid entityId, string expression)
            {
                if (string.IsNullOrWhiteSpace(expression)) return;

                var conditionExpression = this.macrosSubstitutionService.InlineMacros(expression, allMacroses);
                var idsOfEntitesInvolvedInExpression = GetIdsOfEntitiesInvolvedInExpression(conditionExpression);

                if (!dependencies.TryGetValue(entityId, out List<Guid> depsList))
                {
                    depsList = new List<Guid>();
                }

                depsList = depsList.Union(idsOfEntitesInvolvedInExpression).Distinct().ToList();

                if (!dependencies.ContainsKey(entityId) && depsList.Count > 0)
                {
                    dependencies[entityId] = depsList;
                }
            }

            IEnumerable<Guid> GetIdsOfEntitiesInvolvedInExpression(string conditionExpression)
            {
                var identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(conditionExpression);

                foreach (var variable in identifiersUsedInExpression)
                {
                    if (variableNamesByEntitiyIds.TryGetValue(variable, out var value))
                    {
                        yield return value;
                    }
                }
            }
        }

        private static Dictionary<string, Guid> GetAllVariableNames(ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            var variablesOfQuestions = questionnaireDocument
                .Find<IQuestion>(x => !string.IsNullOrWhiteSpace(x.StataExportCaption))
                .Select(x => new { VariableName = x.StataExportCaption, EntityId = x.PublicKey });

            var variablesOfRosters = questionnaireDocument
                .Find<IGroup>(x => x.IsRoster && !string.IsNullOrWhiteSpace(x.VariableName))
                .Select(x => new { x.VariableName, EntityId = x.PublicKey });

            var variablesOfVariables = questionnaireDocument
                .Find<IVariable>()
                .Select(x => new { VariableName = x.Name, EntityId = x.PublicKey });

            return variablesOfQuestions.Union(variablesOfRosters).Union(variablesOfVariables).ToDictionary(x => x.VariableName, x => x.EntityId);
        }
    }
}