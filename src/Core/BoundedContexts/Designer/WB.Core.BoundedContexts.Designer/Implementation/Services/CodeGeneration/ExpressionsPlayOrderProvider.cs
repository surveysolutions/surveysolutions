using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
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
            Dictionary<Guid, List<Guid>> conditionalDependencies = BuildConditionalDependencies(questionnaire.Questionnaire);
            Dictionary<Guid, List<Guid>> structuralDependencies = BuildStructuralDependencies(questionnaire.Questionnaire);
            Dictionary<Guid, List<Guid>> rosterDependencies = BuildRosterDependencies(questionnaire.Questionnaire);
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
            return new Dictionary<Guid, Guid>();
        }

        private Dictionary<Guid, List<Guid>> BuildStructuralDependencies(QuestionnaireDocument questionnaire)
        {
            return questionnaire
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());
        }

        private Dictionary<Guid, List<Guid>> BuildRosterDependencies(QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<Group>(x => x.IsRoster && x.RosterSizeSource == RosterSizeSourceType.Question)
                .Where(x => x.RosterSizeQuestionId.HasValue)
                .GroupBy(x => x.RosterSizeQuestionId.Value)
                .ToDictionary(x => x.Key, x => x.Select(r => r.PublicKey).ToList());
        }

        public Dictionary<Guid, List<Guid>> BuildConditionalDependencies(QuestionnaireDocument questionnaireDocument)
        {
            var dependencies = new Dictionary<Guid, List<Guid>>();

            var allMacroses = questionnaireDocument.Macros.Values;
            var variableNamesByEntitiyIds = GetAllVariableNames(questionnaireDocument);

            foreach (var entity in questionnaireDocument.Find<IComposite>())
            {
                var conditionalEntity = entity as IConditional;
                if (conditionalEntity != null)
                    this.FillDependencies(dependencies, entity.PublicKey, conditionalEntity.ConditionExpression, allMacroses, variableNamesByEntitiyIds);

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
            }

            return dependencies;
        }

        private static Dictionary<string, Guid> GetAllVariableNames(QuestionnaireDocument questionnaireDocument)
        {
            var variablesOfQuestions = questionnaireDocument
                .Find<IQuestion>(x => !string.IsNullOrWhiteSpace(x.StataExportCaption))
                .Select(x => new { VariableName = x.StataExportCaption, EntityId = x.PublicKey });

            var variablesOfRosters = questionnaireDocument
                .Find<IGroup>(x => x.IsRoster && !string.IsNullOrWhiteSpace(x.VariableName))
                .Select(x => new { x.VariableName, EntityId = x.PublicKey });

            return variablesOfQuestions.Union(variablesOfRosters).ToDictionary(x => x.VariableName, x => x.EntityId);
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
