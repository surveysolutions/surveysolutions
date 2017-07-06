using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.TopologicalSorter;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public interface IExpressionsPlayOrderProvider
    {
        List<Guid> GetExpressionsPlayOrder(ReadOnlyQuestionnaireDocument questionnaire);
    }

    public class ExpressionsPlayOrderProvider : IExpressionsPlayOrderProvider
    {
        private readonly IExpressionProcessor expressionProcessor;
        private readonly IMacrosSubstitutionService macrosSubstitutionService;

        public ExpressionsPlayOrderProvider(
            IExpressionProcessor expressionProcessor, 
            IMacrosSubstitutionService macrosSubstitutionService)
        {
            this.expressionProcessor = expressionProcessor;
            this.macrosSubstitutionService = macrosSubstitutionService;
        }

        public List<Guid> GetExpressionsPlayOrder(ReadOnlyQuestionnaireDocument questionnaire)
        {
            Dictionary<Guid, List<Guid>> conditionalDependencies = BuildConditionalDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> structuralDependencies = BuildStructuralDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> rosterDependencies = BuildRosterDependencies(questionnaire);
            Dictionary<Guid, Guid> linkedQuestionByRosterDependencies = BuildLinkedQuestionByRosterDependencies(questionnaire);

            var mergedDependencies = new Dictionary<Guid, List<Guid>>();

            IEnumerable<Guid> allIdsInvolvedInExpressions =
                structuralDependencies.Keys
                    .Union(conditionalDependencies.Keys)
                    .Union(rosterDependencies.Keys)
                    .Union(linkedQuestionByRosterDependencies.Keys)
                    .Union(linkedQuestionByRosterDependencies.Values)
                    .Union(structuralDependencies.SelectMany(x => x.Value))
                    .Union(conditionalDependencies.SelectMany(x => x.Value))
                    .Union(rosterDependencies.SelectMany(x => x.Value))
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

            var sorter = new TopologicalSorter<Guid>();
            IEnumerable<Guid> lisOsfOrderedConditions = sorter.Sort(mergedDependencies.ToDictionary(x => x.Key, x => x.Value.ToArray()));
            return lisOsfOrderedConditions.ToList();
        }

        private Dictionary<Guid, Guid> BuildLinkedQuestionByRosterDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Find<IQuestion>(x => x.LinkedToQuestionId != null || x.LinkedToRosterId!=null)
                .ToDictionary(x => x.PublicKey, x => x.LinkedToQuestionId ?? x.LinkedToRosterId.Value);
        }

        private Dictionary<Guid, List<Guid>> BuildStructuralDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Find<IGroup>() //GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());
        }

        private Dictionary<Guid, List<Guid>> BuildRosterDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var rosterDependencies = 
                (questionnaire.Find<Group>(x => x.IsRoster && x.RosterSizeSource == RosterSizeSourceType.Question)
                    .Where(x => x.RosterSizeQuestionId.HasValue)
                    .Select(x => new { Key = x.RosterSizeQuestionId.Value, Value = x.PublicKey }))
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
                var conditionalEntity = entity as IConditional;
                if (conditionalEntity != null)
                    this.FillDependencies(dependencies, entity.PublicKey, conditionalEntity.ConditionExpression, allMacroses, variableNamesByEntitiyIds);

                var validatableEntity = entity as IValidatable;
                if (validatableEntity != null)
                {
                    foreach (var validationCondition in validatableEntity.ValidationConditions)
                    {
                        this.FillDependencies(dependencies, entity.PublicKey, validationCondition.Expression, allMacroses, variableNamesByEntitiyIds);
                    }
                }
                var question = entity as IQuestion;
                if (question != null)
                {
                    this.FillDependencies(dependencies, question.PublicKey, question.Properties.OptionsFilterExpression, allMacroses, variableNamesByEntitiyIds);
                    if (question.CascadeFromQuestionId != null)
                    {
                        if (dependencies.ContainsKey(entity.PublicKey))
                            dependencies[entity.PublicKey].Add(question.CascadeFromQuestionId.Value);
                        else
                            dependencies.Add(entity.PublicKey, new List<Guid> { question.CascadeFromQuestionId.Value });
                    }
                }

                var variable = entity as IVariable;
                if (variable != null)
                {
                    this.FillDependencies(dependencies, entity.PublicKey, variable.Expression, allMacroses, variableNamesByEntitiyIds);
                }
            }

            return dependencies;
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

        private void FillDependencies(Dictionary<Guid, List<Guid>> dependencies, Guid entityId, string expression,
            IEnumerable<Macro> macroses, Dictionary<string, Guid> varById)
        {
            if (string.IsNullOrWhiteSpace(expression)) return;

            var idsOfEntitesInvolvedInExpression = this.GetIdsOfEntitiesInvolvedInExpression(
                this.macrosSubstitutionService.InlineMacros(expression, macroses), varById);

            if (!idsOfEntitesInvolvedInExpression.Any()) return;

            if (!dependencies.ContainsKey(entityId))
            {
                dependencies[entityId] = new List<Guid>();
            }

            foreach (var dependentEntityId in idsOfEntitesInvolvedInExpression)
            {
                if (!dependencies[entityId].Contains(dependentEntityId))
                    dependencies[entityId].Add(dependentEntityId);
            }
        }

        private List<Guid> GetIdsOfEntitiesInvolvedInExpression(string conditionExpression, Dictionary<string, Guid> variableNames)
        {
            var identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(conditionExpression);

            return new List<Guid>(from variable in identifiersUsedInExpression where variableNames.ContainsKey(variable) select variableNames[variable]);
        }
    }
}
