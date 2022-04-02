using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

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
            Dictionary<Guid, List<Guid>> linkedQuestionByRosterDependencies = BuildLinkedQuestionByRosterDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> substitutionDependencies = BuildSubstitutionDependencies(questionnaire);
            Dictionary<Guid, List<Guid>> staticTextDependencies = BuildStaticTextDependencies(questionnaire);

            Dictionary<Guid, List<Guid>> sectionsFromChildrenDependencies = BuildSectionFromChildrenDependencies(questionnaire);

            var mergedDependencies = new Dictionary<Guid, List<Guid>>();

            IEnumerable<Guid> allIdsInvolvedInExpressions =
                structuralDependencies.Keys
                    .Union(conditionalDependencies.Keys)
                    .Union(rosterDependencies.Keys)
                    .Union(linkedQuestionByRosterDependencies.Keys)
                    .Union(substitutionDependencies.Keys)
                    .Union(staticTextDependencies.Keys)
                    .Union(sectionsFromChildrenDependencies.Keys)
                    .Union(linkedQuestionByRosterDependencies.SelectMany(x => x.Value))
                    .Union(structuralDependencies.SelectMany(x => x.Value))
                    .Union(conditionalDependencies.SelectMany(x => x.Value))
                    .Union(rosterDependencies.SelectMany(x => x.Value))
                    .Union(substitutionDependencies.SelectMany(x => x.Value))
                    .Union(sectionsFromChildrenDependencies.SelectMany(x => x.Value))
                    .Union(staticTextDependencies.SelectMany(x => x.Value))
                    .Distinct();

            allIdsInvolvedInExpressions.ForEach(x => mergedDependencies.Add(x, new List<Guid>()));

            foreach (var x in structuralDependencies)
            {
                mergedDependencies[x.Key].AddRange(x.Value);
            }

            foreach (var staticTextDependency in staticTextDependencies)
            {
                mergedDependencies[staticTextDependency.Key].AddRange(staticTextDependency.Value);
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
                foreach (var entityId in linkedDependency.Value)
                {
                    if (!mergedDependencies[entityId].Contains(linkedDependency.Key))
                    {
                        mergedDependencies[entityId].Add(linkedDependency.Key);
                    }                    
                }
            }
            return mergedDependencies;
        }

        private Dictionary<Guid, List<Guid>> BuildStaticTextDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var allStaticTextsWithAttachedVariables = questionnaire.Find<IStaticText>(s =>
                !string.IsNullOrWhiteSpace(s.AttachmentName) &&
                questionnaire.FirstOrDefault<IVariable>(v => v.VariableName == s.AttachmentName) != null);

            return allStaticTextsWithAttachedVariables.ToDictionary(s => s.PublicKey, s => new List<Guid>
            {
                questionnaire.FirstOrDefault<IVariable>(v => v.VariableName == s.AttachmentName)?.PublicKey 
                    ?? throw new Exception("Variable nor found")
            });
        }

        private Dictionary<Guid, List<Guid>> BuildLinkedQuestionByRosterDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                    .Find<IQuestion>(x => x.LinkedToQuestionId.HasValue || x.LinkedToRosterId.HasValue)
                    .Select(x => new { Key = x.PublicKey, Value = (Guid?)(x.LinkedToQuestionId ?? x.LinkedToRosterId) })
                .Union(questionnaire
                    .Find<IQuestion>(x => x.LinkedToRosterId.HasValue)
                    .Select(x => new { Id = x.PublicKey, RosterTitleQuestionId = (x.LinkedToRosterId.HasValue ? questionnaire.GetRoster(x.LinkedToRosterId.Value)?.RosterTitleQuestionId:(Guid?)null) })
                    .Where(x => x.RosterTitleQuestionId.HasValue)
                    .Select(x => new { Key = x.Id, Value = x.RosterTitleQuestionId }))
                .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Select(s => s.Value!.Value).ToList());
        }

        private Dictionary<Guid, List<Guid>> BuildStructuralDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Find<IGroup>() //GetAllGroups()
                .ToDictionary(
                    group => @group.PublicKey, 
                    group => @group.Children.Select(x => x.PublicKey).ToList()
                );
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
                        .Select(x => new { Key = x.RosterSizeQuestionId!.Value, Value = x.PublicKey })
                    .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Select(r => r.Value).ToList());

            return rosterDependencies;
        }

        private Dictionary<Guid, List<Guid>> BuildSectionFromChildrenDependencies(ReadOnlyQuestionnaireDocument questionnaire)
        {
            var sectionsHavingVariableNamesDependencies =
                questionnaire.Find<Group>(x => !x.IsRoster && !String.IsNullOrWhiteSpace(x.VariableName))
                .SelectMany(group =>
                        group.Children.TreeToEnumerableDepthFirst(x => x.Children)
                            .OfType<IQuestion>(), //questions only 
                    (g, c) => new {Key = g.PublicKey, Value = c.PublicKey})
                    .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.Select(r => r.Value).ToList());

            return sectionsHavingVariableNamesDependencies;
        }

        private Dictionary<Guid, List<Guid>> BuildConditionalDependencies(ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            var dependencies = new Dictionary<Guid, List<Guid>>();

            var allMacros = questionnaireDocument.Macros.Values;
            var variableNamesByEntityIds = GetAllVariableNames(questionnaireDocument);

            Dictionary<Guid, List<Guid>> sectionsFromChildrenDependencies = BuildSectionFromChildrenDependencies(questionnaireDocument);

            foreach (var entity in questionnaireDocument.Find<IComposite>())
            {
                if (entity is IConditional conditionalEntity)
                {
                    FillDependencies(dependencies, variableNamesByEntityIds, allMacros, entity.PublicKey, conditionalEntity.ConditionExpression, sectionsFromChildrenDependencies);
                }

                if (entity is IQuestion question)
                {
                    FillDependencies(dependencies, variableNamesByEntityIds, allMacros, question.PublicKey, question.Properties?.OptionsFilterExpression, sectionsFromChildrenDependencies);
                    FillDependencies(dependencies, variableNamesByEntityIds, allMacros, question.PublicKey, question.LinkedFilterExpression, sectionsFromChildrenDependencies);

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
                    FillDependencies(dependencies, variableNamesByEntityIds, allMacros, entity.PublicKey, variable.Expression, sectionsFromChildrenDependencies);
                }
            }

            return dependencies;
        }

        public Dictionary<Guid, List<Guid>> BuildValidationDependencyGraph(ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            Dictionary<Guid, List<Guid>> validationDependencies = BuildAllValidationDependencies(questionnaireDocument);

            var mergedDependencies = new Dictionary<Guid, List<Guid>>();

            IEnumerable<Guid> allIdsInvolvedInExpressions = validationDependencies.SelectMany(x => x.Value).Distinct();

            allIdsInvolvedInExpressions.ForEach(x => mergedDependencies.Add(x, new List<Guid>()));

            foreach (var validationDependency in validationDependencies)
            {
                foreach (var dependency in validationDependency.Value)
                {
                    if (!mergedDependencies[dependency].Contains(validationDependency.Key))
                    {
                        mergedDependencies[dependency].Add(validationDependency.Key);
                    }
                }
            }

            return mergedDependencies;
        }

        private Dictionary<Guid, List<Guid>> BuildAllValidationDependencies(ReadOnlyQuestionnaireDocument questionnaireDocument)
        {
            var dependencies = new Dictionary<Guid, List<Guid>>();

            var allMacros = questionnaireDocument.Macros.Values;
            var variableNamesByEntityIds = GetAllVariableNames(questionnaireDocument);

            Dictionary<Guid, List<Guid>> sectionsFromChildrenDependencies = BuildSectionFromChildrenDependencies(questionnaireDocument);

            foreach (var entity in questionnaireDocument.Find<IComposite>())
            {
                if (entity is IValidatable validatableEntity)
                {
                    foreach (var validationCondition in validatableEntity.ValidationConditions)
                    {
                        FillDependencies(dependencies, variableNamesByEntityIds, allMacros, 
                            entity.PublicKey, 
                            validationCondition.Expression, 
                            sectionsFromChildrenDependencies, 
                            ignoreReferenceOnSelf: true);
                    }
                }
            }

            return dependencies;
        }

        private void FillDependencies(Dictionary<Guid, List<Guid>> dependencies, 
            Dictionary<string, Guid> variableNamesByEntitiyIds, 
            Dictionary<Guid, Macro>.ValueCollection allMacros, 
            Guid entityId, 
            string? expression,
            Dictionary<Guid, List<Guid>> sectionsFromChildrenDependencies,
            bool ignoreReferenceOnSelf = false)
        {
            if (string.IsNullOrWhiteSpace(expression)) return;

            var conditionExpression = this.macrosSubstitutionService.InlineMacros(expression, allMacros);
            var idsOfEntitiesInvolvedInExpression = GetIdsOfEntitiesInvolvedInExpression(variableNamesByEntitiyIds, conditionExpression, entityId, ignoreReferenceOnSelf).ToList();

            if (!dependencies.TryGetValue(entityId, out List<Guid>? depsList))
            {
                depsList = new List<Guid>();
            }

            depsList = depsList.Union(idsOfEntitiesInvolvedInExpression).Distinct().ToList();

            //add all questions that affects group state if it was used in expression
            foreach (var pair in sectionsFromChildrenDependencies.Where(x => idsOfEntitiesInvolvedInExpression.Contains(x.Key)))
            {
                depsList = depsList.Union(pair.Value).Distinct().ToList();
            }

            if (depsList.Count > 0)
            {
                dependencies[entityId] = depsList;
            }

        }

        IEnumerable<Guid> GetIdsOfEntitiesInvolvedInExpression(Dictionary<string, Guid> variableNamesByEntityIds, 
            string conditionExpression,
            Guid entityId,
            bool ignoreReferenceOnSelf = false)
        {
            var identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(conditionExpression);

            if (!ignoreReferenceOnSelf && identifiersUsedInExpression.Contains("self"))
                yield return entityId;

            foreach (var variable in identifiersUsedInExpression)
            {
                if (variableNamesByEntityIds.TryGetValue(variable, out var value))
                {
                    if (ignoreReferenceOnSelf && value == entityId)
                        continue;
                    yield return value;
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
                .Find<IVariable>(x => !string.IsNullOrWhiteSpace(x.VariableName))
                .Select(x => new { VariableName = x.Name, EntityId = x.PublicKey });

            var variablesOfSections = questionnaireDocument
                .Find<IGroup>(x => !x.IsRoster && !string.IsNullOrWhiteSpace(x.VariableName))
                .Select(x => new { x.VariableName, EntityId = x.PublicKey });

            return variablesOfQuestions.Union(variablesOfRosters)
                .Union(variablesOfVariables)
                .Union(variablesOfSections)
                .ToDictionary(x => x.VariableName, x => x.EntityId);
        }
    }
}
