using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Mono.Cecil;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using Group = Main.Core.Entities.SubEntities.Group;
using ITypeDefinition = ICSharpCode.Decompiler.TypeSystem.ITypeDefinition;
using IVariable = WB.Core.SharedKernels.QuestionnaireEntities.IVariable;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Utils
{
    public static class Utils
    {
        public static bool IsExistsMacrosesInDocument(QuestionnaireDocument questionnaireDocument)
        {
            bool isExistsMacros = false;

            var entities = questionnaireDocument.Children.TreeToEnumerable(x => x.Children);

            foreach (var entity in entities)
            {
                if (entity is IConditional conditionalEntity)
                {
                    isExistsMacros |= IsExpressionContainsMacros(conditionalEntity.ConditionExpression);
                }

                if (entity is IValidatable validatable)
                {
                    foreach (var validationCondition in validatable.ValidationConditions)
                    {
                        isExistsMacros |= IsExpressionContainsMacros(validationCondition.Expression);
                    }
                }

                if (entity is IQuestion question)
                {
                    isExistsMacros |= IsExpressionContainsMacros(question.Properties.OptionsFilterExpression);
                    isExistsMacros |= IsExpressionContainsMacros(question.LinkedFilterExpression);
                }

                if (entity is IVariable variable)
                {
                    isExistsMacros |= IsExpressionContainsMacros(variable.Expression);
                }

                if (isExistsMacros)
                    return true;
            }

            return isExistsMacros;
        }

        public static bool IsSupportedDecompile(string pathToAssembly)
        {
            var module = UniversalAssemblyResolver.LoadMainModule(pathToAssembly, true, true);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            ((UniversalAssemblyResolver) module.AssemblyResolver).AddSearchDirectory(path);
            var decompiler = new CSharpDecompiler(module, new DecompilerSettings());

            var typeDefinition = new TypeDefinition("WB.Core.SharedKernels.DataCollection.Generated",
                CodeGeneratorV2.QuestionnaireLevel, TypeAttributes.Class);

            try
            {
                var classCode = decompiler.DecompileAsString(typeDefinition);
            }
            catch (InvalidOperationException e)
            {
                if (e.Message == "Could not find type definition in NR type system")
                {
                    var typeDefinitionForOldCode = new TypeDefinition("WB.Core.SharedKernels.DataCollection.Generated",
                        WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.CodeGenerator.QuestionnaireTypeName, 
                        TypeAttributes.Class);
                    var classCode = decompiler.DecompileAsString(typeDefinitionForOldCode);

                    return false;
                }

                throw;
            }

            return true;
        }

        public static void InlineMacrosesInDocument(QuestionnaireDocument questionnaireDocument, string pathToAssembly)
        {
            var levels = GenerateLevels(questionnaireDocument);

            var entities = questionnaireDocument.Children.TreeToEnumerable(x => x.Children);

            foreach (var entity in entities)
            {
                if (entity is IConditional conditionalEntity)
                {
                    var name = entity is Group group
                        ? group.IsRoster ? $"IsEnabled__{entity.VariableName}" : $"IsEnabled__subsection_{entity.PublicKey.FormatGuid()}"
                        : entity is StaticText ? $"IsEnabled__text_{entity.PublicKey.FormatGuid()}"
                        : $"IsEnabled__{entity.VariableName}";
                    string className = GetParentVariable(entity, questionnaireDocument, levels);
                    if (IsExpressionContainsMacros(conditionalEntity.ConditionExpression))
                        conditionalEntity.ConditionExpression = DecompileMacrosFromAssembly(name, className, pathToAssembly);
                }

                if (entity is IValidatable validatable)
                {
                    for (int i = 0; i < validatable.ValidationConditions.Count; i++)
                    {
                        var validationCondition = validatable.ValidationConditions[i];

                        var name = $"IsValid__{entity.VariableName}__{i}";
                        string className = GetParentVariable(entity, questionnaireDocument, levels, false);

                        if (IsExpressionContainsMacros(validationCondition.Expression))
                            validationCondition.Expression = DecompileMacrosFromAssembly(name, className, pathToAssembly);
                    }
                }

                if (entity is IQuestion question)
                {
                    var filterExpressionName = $"FilterOption__{entity.VariableName}";
                    string filterExpressionNameClassName = GetParentVariable(entity, questionnaireDocument, levels);
                    if (IsExpressionContainsMacros(question.Properties.OptionsFilterExpression))
                        question.Properties.OptionsFilterExpression = DecompileMacrosFromAssembly(filterExpressionName, filterExpressionNameClassName, pathToAssembly);

                    var linkedFilterExpressionName = $"FilterForLinkedQuestion__{entity.VariableName}";
                    string linkedFilterExpressionNameClassName = GetParentVariable(entity, questionnaireDocument, levels);
                    if (IsExpressionContainsMacros(question.LinkedFilterExpression))
                        question.LinkedFilterExpression = DecompileMacrosFromAssembly(linkedFilterExpressionName, linkedFilterExpressionNameClassName, pathToAssembly);
                }

                if (entity is IVariable variable)
                {
                    var variableName = $"Variable__{variable.VariableName}";
                    string className = GetParentVariable(entity, questionnaireDocument, levels);
                    if (IsExpressionContainsMacros(variable.Expression))
                        variable.Expression = DecompileMacrosFromAssembly(variableName, className, pathToAssembly);
                }
            }
        }

        private static Dictionary<string, string> GenerateLevels(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaire = questionnaireDocument.AsReadOnly();

            Dictionary<string, string> levels = new Dictionary<string, string>();

            Dictionary<RosterScope, Group[]> rosterScopes = questionnaire.Find<Group>()
                .GroupBy(questionnaire.GetRosterScope)
                .ToDictionary(x => x.Key, x => x.ToArray());

            foreach (var rosterScopePairs in rosterScopes)
            {
                var rosters = rosterScopePairs.Value;
                var firstRosterInScope = rosters.FirstOrDefault(x => x.IsRoster);

                string variable;
                string levelClassName;

                if (firstRosterInScope == null)
                {
                    variable = CodeGeneratorV2.QuestionnaireIdName;
                    levelClassName = CodeGeneratorV2.QuestionnaireLevel;
                }
                else
                {
                    variable = questionnaire.GetVariable(firstRosterInScope);
                    levelClassName = CodeGeneratorV2.LevelPrefix + variable;
                }

                levels.Add(variable, levelClassName);

                foreach (var roster in rosters.Skip(1))
                {
                    levels.Add(questionnaire.GetVariable(roster), levelClassName);
                }
            }

            return levels;
        }


        private static bool IsExpressionContainsMacros(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            var isExpressionContainsMacros = expression.Contains("$");
            return isExpressionContainsMacros;
        }

        private static ModuleDefinition mainModule;
        private static CSharpDecompiler decompiler;
        private static HashSet<string> classes;

        private static string DecompileMacrosFromAssembly(string methodName, string className, string assemblyPath)
        {
            if (mainModule?.FileName != assemblyPath)
            {
                mainModule = UniversalAssemblyResolver.LoadMainModule(assemblyPath, true, true);
                var path = AppDomain.CurrentDomain.BaseDirectory;//Directory.GetCurrentDirectory();
                (mainModule.AssemblyResolver as UniversalAssemblyResolver).AddSearchDirectory(path);
                decompiler = new CSharpDecompiler(mainModule, new DecompilerSettings());
                classes = new HashSet<string>();
            }

            var typeDefinition = new TypeDefinition("WB.Core.SharedKernels.DataCollection.Generated", className, TypeAttributes.Class);
            if (!classes.Contains(className))
            {
                var allClass = decompiler.DecompileAsString(typeDefinition); // for caching
                classes.Add(className);
            }

            ITypeDefinition typeDef = decompiler.TypeSystem.Resolve(typeDefinition).GetDefinition();
            var methods = typeDef.GetMethods(m => m.Name == methodName, GetMemberOptions.IgnoreInheritedMembers)
                  .Concat(typeDef.GetAccessors(m => m.Name == methodName, GetMemberOptions.IgnoreInheritedMembers));
            var methodDefinition = (MethodDefinition)decompiler.TypeSystem.GetCecil(methods.Single());

            methodDefinition.DeclaringType = typeDefinition;
            var methodBody = decompiler.DecompileAsString(methodDefinition);
            //var methodLines = methodBody.Split('\n');
            //methodBody = string.Concat(methodLines.Skip(2).Take(methodLines.Length - 4));

            return methodBody;
        }

        private static string GetParentVariable(IComposite entity, QuestionnaireDocument questionnaireDocument, Dictionary<string, string> levels, bool suppertLinkedRedirection = true)
        {
            if (suppertLinkedRedirection && entity is SingleQuestion singleQuestion && singleQuestion.LinkedToRosterId.HasValue)
            {
                entity = questionnaireDocument.Find<Group>(singleQuestion.LinkedToRosterId.Value);
            }
            else if (suppertLinkedRedirection && entity is MultyOptionsQuestion multiQuestion && multiQuestion.LinkedToRosterId.HasValue)
            {
                entity = questionnaireDocument.Find<Group>(multiQuestion.LinkedToRosterId.Value);
            }

            var variable = GetParentVariable(entity);
            if (variable != null && levels.ContainsKey(variable))
                return levels[variable];

            return CodeGeneratorV2.QuestionnaireLevel;
        }

        private static string GetParentVariable(IComposite entity)
        {
            if (entity is Group group && group.IsRoster)
                return group.VariableName;

            var parent = entity.GetParent();
            while (parent != null)
            {
                if (!string.IsNullOrWhiteSpace(parent.VariableName))
                    return parent.VariableName;

                parent = parent.GetParent();
            }

            return null;
        }
    }
}
