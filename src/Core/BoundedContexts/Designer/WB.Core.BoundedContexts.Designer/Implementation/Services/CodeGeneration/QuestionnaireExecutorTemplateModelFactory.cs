using System;
using System.Collections.Generic;
using System.Linq;

using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class QuestionnaireExecutorTemplateModelFactory
    {
        private readonly IExpressionProcessor expressionProcessor;

        private const string InterviewExpressionStatePrefix = "InterviewExpressionState";
        private const string privateFieldsPrefix = "@__";
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        private readonly ILookupTableService lookupTableService;

        public QuestionnaireExecutorTemplateModelFactory(
            IMacrosSubstitutionService macrosSubstitutionService, 
            IExpressionProcessor expressionProcessor, 
            ILookupTableService lookupTableService)
        {
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.expressionProcessor = expressionProcessor;
            this.lookupTableService = lookupTableService;
        }

        public QuestionnaireExecutorTemplateModel CreateQuestionnaireExecutorTemplateModel(
            QuestionnaireDocument questionnaire, 
            CodeGenerationSettings codeGenerationSettings)
        {
            var template = new QuestionnaireExecutorTemplateModel
            {
                AdditionInterfaces = codeGenerationSettings.AdditionInterfaces,
                Namespaces = codeGenerationSettings.Namespaces,
                Id = questionnaire.PublicKey,
                GeneratedClassName = $"{InterviewExpressionStatePrefix}_{Guid.NewGuid().FormatGuid()}"
            };

            var questionnaireLevelModel = new QuestionnaireLevelTemplateModel();

            template.QuestionnaireLevelModel = questionnaireLevelModel;

            template.LookupTables = BuildLookupTableModels(questionnaire).ToList();

            template.StructuralDependencies = BuildStructuralDependencies(questionnaire);

            template.ConditionalDependencies = BuildConditionalDependencies(questionnaire);

            template.ConditionsPlayOrder = BuildConditionsPlayOrder(template.ConditionalDependencies, template.StructuralDependencies);

            Dictionary<string, string> generatedScopesTypeNames;
            List<QuestionTemplateModel> allQuestions;
            List<GroupTemplateModel> allGroups;
            List<RosterTemplateModel> allRosters;

            // creates rosters model and fills questionnaireLevelModel and roster models with questions, groups and nested rosters.
            this.BuildStructures(questionnaire, questionnaireLevelModel, out generatedScopesTypeNames, out allQuestions, out allGroups, out allRosters);

            questionnaireLevelModel.ConditionMethodsSortedByExecutionOrder = GetConditionMethodsSortedByExecutionOrder(questionnaireLevelModel.Questions, questionnaireLevelModel.Groups, null, template.ConditionsPlayOrder);

            var rosterGroupedByScope = allRosters.GroupBy(r => r.GeneratedTypeName);

            template.RostersGroupedByScope = rosterGroupedByScope
                .Select(x => BuildRosterScopeTemplateModel(x.Key, x.ToList(), template))
                .ToDictionary(x => x.GeneratedTypeName);

            BuildReferencesOnParentRosters(template.RostersGroupedByScope, questionnaireLevelModel);

            BuildReferencesOnParentQuestions(template.RostersGroupedByScope, questionnaireLevelModel);
            
            template.AllQuestions = allQuestions;
            template.AllGroups = allGroups;
            template.AllRosters = allRosters;
            template.GeneratedScopesTypeNames = generatedScopesTypeNames;
            return template;
        }

        private RosterScopeTemplateModel BuildRosterScopeTemplateModel(
            string rosterScopeType, 
            List<RosterTemplateModel> rostersInScope, 
            QuestionnaireExecutorTemplateModel template)
        {
            var groups = rostersInScope.SelectMany(r => r.Groups).ToList();
            var questions = rostersInScope.SelectMany(r => r.Questions).ToList();
            var rosters = rostersInScope.SelectMany(r => r.Rosters).ToList();

            var conditionMethodsSortedByExecutionOrder = GetConditionMethodsSortedByExecutionOrder(questions, groups, rostersInScope, template.ConditionsPlayOrder);

            return new RosterScopeTemplateModel(rosterScopeType, questions, groups, rosters, rostersInScope, conditionMethodsSortedByExecutionOrder);
        }

        private static List<ConditionMethodAndState> GetConditionMethodsSortedByExecutionOrder(
            List<QuestionTemplateModel> questions, 
            List<GroupTemplateModel> groups, 
            List<RosterTemplateModel> rosters, 
            List<Guid> conditionsPlayOrder)
        {
            List<GroupTemplateModel> groupsWithConditions = groups.Where(g => !string.IsNullOrWhiteSpace(g.Conditions)).Reverse().ToList();
            List<QuestionTemplateModel> questionsWithConditions = questions.Where(q => !string.IsNullOrWhiteSpace(q.Conditions)).ToList();
            List<RosterTemplateModel> rostersWithConditions = (rosters ?? new List<RosterTemplateModel>()).Where(r => !string.IsNullOrWhiteSpace(r.Conditions)).Reverse().ToList();

            Dictionary<Guid, ConditionMethodAndState> itemsToSort = new Dictionary<Guid, ConditionMethodAndState>();

            groupsWithConditions.ForEach(g => itemsToSort.Add(g.Id, new ConditionMethodAndState(g.GeneratedConditionsMethodName, g.GeneratedStateName)));
            rostersWithConditions.ForEach(r => itemsToSort.Add(r.Id, new ConditionMethodAndState(r.GeneratedConditionsMethodName, r.GeneratedStateName)));
            questionsWithConditions.ForEach(q => itemsToSort.Add(q.Id, new ConditionMethodAndState(q.GeneratedConditionsMethodName, q.GeneratedStateName)));

            var itemsSorted = new List<ConditionMethodAndState>();

            foreach (Guid id in conditionsPlayOrder)
            {
                if (itemsToSort.ContainsKey(id))
                    itemsSorted.Add(itemsToSort[id]);
            }

            return itemsSorted;
        }

        private static List<Guid> BuildConditionsPlayOrder(
            Dictionary<Guid, List<Guid>> conditionalDependencies, 
            Dictionary<Guid, List<Guid>> structuralDependencies)
        {
            var mergedDependencies = new Dictionary<Guid, List<Guid>>();

            IEnumerable<Guid> allIdsInvolvedInExpressions =
                structuralDependencies.Keys.Union(conditionalDependencies.Keys)
                    .Union(structuralDependencies.SelectMany(x => x.Value))
                    .Union(conditionalDependencies.SelectMany(x => x.Value))
                    .Distinct();

            allIdsInvolvedInExpressions.ForEach(x => mergedDependencies.Add(x, new List<Guid>()));

            structuralDependencies.ForEach(x => mergedDependencies[x.Key].AddRange(x.Value));

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

            var sorter = new TopologicalSorter<Guid>();
            IEnumerable<Guid> listOfOrderedContitions = sorter.Sort(mergedDependencies.ToDictionary(x => x.Key, x => x.Value.ToArray()));
            return listOfOrderedContitions.ToList();
        }

        private static Dictionary<Guid, List<Guid>> BuildStructuralDependencies(QuestionnaireDocument questionnaire)
        {
            return questionnaire
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());
        }

        private static void BuildReferencesOnParentRosters(
            Dictionary<string, RosterScopeTemplateModel> rostersGroupedByScope,
            QuestionnaireLevelTemplateModel questionnaireLevelModel)
        {
            foreach (RosterScopeTemplateModel rosterScopeModel in rostersGroupedByScope.Values)
            {
                var parentRosters = GetParentRosters(rosterScopeModel, rostersGroupedByScope).ToList();

                var allParentsRostersToTop = parentRosters
                    .SelectMany(x => x.Rosters)
                    .Union(questionnaireLevelModel.Rosters)
                    .Select(x => new TypeAndNameModel { GeneratedTypeName = x.GeneratedTypeName, VariableName = x.VariableName }).ToList();

               rosterScopeModel.AllParentsRostersToTop = allParentsRostersToTop;
            }
        }

        private static void BuildReferencesOnParentQuestions(
           Dictionary<string, RosterScopeTemplateModel> rostersGroupedByScope,
           QuestionnaireLevelTemplateModel questionnaireLevelModel)
        {
            foreach (RosterScopeTemplateModel rosterScopeModel in rostersGroupedByScope.Values)
            {
                var parentRosters = GetParentRosters(rosterScopeModel, rostersGroupedByScope).ToList();

                var allParentsQuestionsToTop = parentRosters
                    .SelectMany(x => x.Questions)
                    .Union(questionnaireLevelModel.Questions)
                    .Select(x => new TypeAndNameModel { GeneratedTypeName = x.GeneratedTypeName, VariableName = x.VariableName }).ToList();

                rosterScopeModel.AllParentsQuestionsToTop = allParentsQuestionsToTop;
            }
        }

        private static IEnumerable<RosterScopeTemplateModel> GetParentRosters(RosterScopeTemplateModel rosterModel, Dictionary<string, RosterScopeTemplateModel> rostersGroupedByScope)
        {
            var parentScopeTypeName = rosterModel.ParentTypeName ?? "";

            while (rostersGroupedByScope.ContainsKey(parentScopeTypeName))
            {
                RosterScopeTemplateModel parentScope = rostersGroupedByScope[parentScopeTypeName];

                yield return parentScope;

                parentScopeTypeName = parentScope.ParentTypeName;
            }
        }

        private IEnumerable<LookupTableTemplateModel> BuildLookupTableModels(QuestionnaireDocument questionnaire)
        {
            foreach (var table in questionnaire.LookupTables)
            {
                var lookupTableData = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, table.Key);
                var tableName = table.Value.TableName;
                var tableTemplateModel = new LookupTableTemplateModel
                {
                    TableName = tableName.ToCamelCase(),
                    TypeName = tableName.ToPascalCase(),
                    TableNameField = privateFieldsPrefix + tableName.ToCamelCase(),
                    Rows = lookupTableData.Rows,
                    VariableNames = lookupTableData.VariableNames
                };
                yield return tableTemplateModel;
            }
        }

        private void BuildStructures(QuestionnaireDocument questionnaireDoc,
            QuestionnaireLevelTemplateModel questionnaireLevelModel,
            out Dictionary<string, string> generatedScopesTypeNames,
            out List<QuestionTemplateModel> allQuestions, 
            out List<GroupTemplateModel> allGroups,
            out List<RosterTemplateModel> allRosters)
        {
            generatedScopesTypeNames = new Dictionary<string, string>();
            allQuestions = new List<QuestionTemplateModel>();
            allGroups = new List<GroupTemplateModel>();
            allRosters = new List<RosterTemplateModel>();

            var rostersToProcess = new Queue<Tuple<IGroup, RosterScopeBaseModel>>();
            rostersToProcess.Enqueue(new Tuple<IGroup, RosterScopeBaseModel>(questionnaireDoc, questionnaireLevelModel));

            while (rostersToProcess.Count != 0)
            {
                Tuple<IGroup, RosterScopeBaseModel> rosterScope = rostersToProcess.Dequeue();
                RosterScopeBaseModel currentScope = rosterScope.Item2;

                var childrenOfCurrentRoster = new Queue<IComposite>();

                foreach (IComposite childGroup in rosterScope.Item1.Children)
                {
                    childrenOfCurrentRoster.Enqueue(childGroup);
                }

                while (childrenOfCurrentRoster.Count != 0)
                {
                    IComposite child = childrenOfCurrentRoster.Dequeue();

                    if (IsQuestion(child))
                    {
                        var question = this.CreateQuestionTemplateModel(questionnaireDoc, (IQuestion)child, currentScope.GeneratedRosterScopeName);

                        currentScope.Questions.Add(question);

                        allQuestions.Add(question);

                        continue;
                    }

                    if (!IsGroup(child))
                        continue;

                    var childAsGroup = (IGroup)child;

                    if (IsRoster(childAsGroup))
                    {
                        var roster = this.CreateRosterTemplateModel(questionnaireDoc, generatedScopesTypeNames, childAsGroup, currentScope);

                        rostersToProcess.Enqueue(new Tuple<IGroup, RosterScopeBaseModel>(childAsGroup, roster));

                        allRosters.Add(roster);

                        currentScope.Rosters.Add(roster);
                    }
                    else
                    {
                        var group = this.CreateGroupTemplateModel(questionnaireDoc, childAsGroup, currentScope);

                        currentScope.Groups.Add(group);

                        allGroups.Add(group);

                        foreach (IComposite childGroup in childAsGroup.Children)
                        {
                            childrenOfCurrentRoster.Enqueue(childGroup);
                        }
                    }
                }
            }
        }

        private static bool IsRoster(IGroup item)
        {
            return item.IsRoster;
        }

        private static bool IsGroup(IComposite item)
        {
            return item is IGroup;
        }

        private static bool IsQuestion(IComposite item)
        {
            return item is IQuestion;
        }

        private QuestionTemplateModel CreateQuestionTemplateModel(
            QuestionnaireDocument questionnaireDoc,
            IQuestion childAsIQuestion,
            string generatedRosterScopeName)
        {
            string varName = !String.IsNullOrEmpty(childAsIQuestion.StataExportCaption)
                ? childAsIQuestion.StataExportCaption
                : "__" + childAsIQuestion.PublicKey.FormatGuid();

            var validation = this.macrosSubstitutionService.InlineMacros(childAsIQuestion.ValidationExpression, questionnaireDoc.Macros.Values);
            var condition = childAsIQuestion.CascadeFromQuestionId.HasValue 
                ? this.GetConditionForCascadingQuestion(questionnaireDoc, childAsIQuestion.PublicKey)
                : this.macrosSubstitutionService.InlineMacros(childAsIQuestion.ConditionExpression, questionnaireDoc.Macros.Values);

            var question = new QuestionTemplateModel
            {
                Id = childAsIQuestion.PublicKey,
                VariableName = varName,
                Conditions = condition,
                Validations = validation,
                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                GeneratedValidationsMethodName = "IsValid_" + varName,
                QuestionType = childAsIQuestion.QuestionType,
                GeneratedTypeName = GenerateQuestionTypeName(childAsIQuestion),
                GeneratedMemberName = "@__" + varName,
                GeneratedStateName = "@__" + varName + "_state",
                GeneratedIdName = "@__" + varName + "_id",
                RosterScopeName = generatedRosterScopeName
            };

            if (childAsIQuestion.QuestionType == QuestionType.MultyOption && childAsIQuestion is IMultyOptionsQuestion)
            {
                var multyOptionsQuestion = childAsIQuestion as IMultyOptionsQuestion;
                question.IsMultiOptionYesNoQuestion = multyOptionsQuestion.YesNoView;
                if (question.IsMultiOptionYesNoQuestion)
                {
                    question.AllMultioptionYesNoCodes = multyOptionsQuestion.Answers.Select(x => x.AnswerValue).ToList();
                }
            }
            return question;
        }

        private RosterTemplateModel CreateRosterTemplateModel(
            QuestionnaireDocument questionnaireDoc,
            Dictionary<string, string> generatedScopesTypeNames,
            IGroup childAsIGroup,
            RosterScopeBaseModel currentScope)
        {
            Guid currentScopeId = childAsIGroup.RosterSizeSource == RosterSizeSourceType.FixedTitles
                ? childAsIGroup.PublicKey
                : childAsIGroup.RosterSizeQuestionId.Value;

            List<Guid> currentRosterScope = currentScope.RosterScope.ToList();
            currentRosterScope.Add(currentScopeId);

            string varName = !String.IsNullOrWhiteSpace(childAsIGroup.VariableName)
                ? childAsIGroup.VariableName
                : "__" + childAsIGroup.PublicKey.FormatGuid();

            var roster = new RosterTemplateModel
            {
                Id = childAsIGroup.PublicKey,
                Conditions = this.macrosSubstitutionService.InlineMacros(childAsIGroup.ConditionExpression, questionnaireDoc.Macros.Values),
                VariableName = varName,
                GeneratedTypeName = this.GenerateTypeNameByScope($"{varName}_{childAsIGroup.PublicKey.FormatGuid()}_", currentRosterScope, generatedScopesTypeNames),
                GeneratedStateName = "@__" + varName + "_state",
                GeneratedIdName = "@__" + varName + "_id",
                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                RosterScope = currentRosterScope,
                ParentGeneratedTypeName = currentScope.GeneratedTypeName,
                GeneratedRosterScopeName = "@__" + varName + "_scope",
            };
            return roster;
        }

        private GroupTemplateModel CreateGroupTemplateModel(
            QuestionnaireDocument questionnaireDoc,
            IGroup childAsIGroup,
            RosterScopeBaseModel currentScope)
        {
            string varName = childAsIGroup.PublicKey.FormatGuid();
            var group = new GroupTemplateModel
            {
                Id = childAsIGroup.PublicKey,
                Conditions =
                    this.macrosSubstitutionService.InlineMacros(
                        childAsIGroup.ConditionExpression,
                        questionnaireDoc.Macros.Values),
                VariableName = "@__" + varName, //generating variable name by publicKey
                GeneratedStateName = "@__" + varName + "_state",
                GeneratedIdName = "@__" + varName + "_id",
                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                RosterScopeName = currentScope.GeneratedRosterScopeName
            };
            return @group;
        }

        private string GetConditionForCascadingQuestion(QuestionnaireDocument questionnaireDocument, Guid cascadingQuestionId)
        {
            var childQuestion = questionnaireDocument.Find<SingleQuestion>(cascadingQuestionId);
            var parentQuestion = questionnaireDocument.Find<SingleQuestion>(childQuestion.CascadeFromQuestionId.Value);

            var conditionForChildCascadingQuestion = this.macrosSubstitutionService.InlineMacros(childQuestion.ConditionExpression, questionnaireDocument.Macros.Values);

            if (parentQuestion == null)
            {
                return conditionForChildCascadingQuestion;
            }

            string childQuestionCondition = (string.IsNullOrWhiteSpace(conditionForChildCascadingQuestion)
                ? string.Empty
                : $" && {conditionForChildCascadingQuestion}");

            var valuesOfParentCascadingThatHaveChildOptions = childQuestion.Answers.Select(x => x.ParentValue).Distinct();
            var allValuesOfParentCascadingQuestion = parentQuestion.Answers.Select(x => x.AnswerValue);

            var parentOptionsThatHaveNoChildOptions = allValuesOfParentCascadingQuestion.Where(x => !valuesOfParentCascadingThatHaveChildOptions.Contains(x));

            var expressionToDisableChildThatHasNoOptionsForChosenParent = !parentOptionsThatHaveNoChildOptions.Any()
                ? string.Empty
                : GenerateExpressionToDisableChildThatHasNoOptionsForChosenParent(parentOptionsThatHaveNoChildOptions, parentQuestion.StataExportCaption);

            return string.Format("!IsAnswerEmpty({0})", parentQuestion.StataExportCaption) + expressionToDisableChildThatHasNoOptionsForChosenParent + childQuestionCondition;
        }

        private Dictionary<Guid, List<Guid>> BuildConditionalDependencies(QuestionnaireDocument questionnaireDocument)
        {
            var allGroups = questionnaireDocument.GetAllGroups().ToList();
            Dictionary<string, Guid> variableNames = questionnaireDocument
                .GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.StataExportCaption))
                .ToDictionary(q => q.StataExportCaption, q => q.PublicKey);

            foreach (var roster in allGroups.Where(x => x.IsRoster && !string.IsNullOrWhiteSpace(x.VariableName)))
            {
                variableNames.Add(roster.VariableName, questionnaireDocument.PublicKey);
            }
            
            var groupsWithConditions = allGroups.Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression));
            var questionsWithCondition = questionnaireDocument
                .GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression));

            Dictionary<Guid, List<Guid>> dependencies = groupsWithConditions.ToDictionary(
                x => x.PublicKey,
                x => this.GetIdsOfQuestionsInvolvedInExpression(
                    this.macrosSubstitutionService.InlineMacros(x.ConditionExpression, questionnaireDocument.Macros.Values),
                    variableNames));

            questionsWithCondition.ToDictionary(
                x => x.PublicKey,
                x => this.GetIdsOfQuestionsInvolvedInExpression(
                    this.macrosSubstitutionService.InlineMacros(x.ConditionExpression, questionnaireDocument.Macros.Values), variableNames))
                .ToList()
                .ForEach(x => dependencies.Add(x.Key, x.Value));

            var cascadingQuestions = questionnaireDocument
                .GetEntitiesByType<SingleQuestion>()
                .Where(x => x.CascadeFromQuestionId.HasValue);

            foreach (var cascadingQuestion in cascadingQuestions)
            {
                if (dependencies.ContainsKey(cascadingQuestion.PublicKey))
                {
                    dependencies[cascadingQuestion.PublicKey].Add(cascadingQuestion.CascadeFromQuestionId.Value);
                }
                else
                {
                    dependencies.Add(cascadingQuestion.PublicKey, new List<Guid> { cascadingQuestion.CascadeFromQuestionId.Value });
                }
            }

            return dependencies;
        }


        static string GenerateExpressionToDisableChildThatHasNoOptionsForChosenParent(IEnumerable<string> parentOptionsThatHaveNoChildOptions, string stataExportCaption)
        {
            var joinedParentOptions = string.Join(",", parentOptionsThatHaveNoChildOptions.Select(x => string.Format("{0}", x)));
            return string.Format(@" && !(new List<decimal?>(){{ {0} }}.Contains({1}))", joinedParentOptions, stataExportCaption);
        }

        private static string GenerateQuestionTypeName(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    return "string";

                case QuestionType.AutoPropagate:
                    return "long?";

                case QuestionType.Numeric:
                    return (question as NumericQuestion).IsInteger ? "long?" : "double?";

                case QuestionType.QRBarcode:
                    return "string";

                case QuestionType.MultyOption:
                    var multiOtion = question as MultyOptionsQuestion;
                    if (multiOtion != null && multiOtion.YesNoView)
                        return typeof(YesNoAnswers).Name;
                    return (question.LinkedToQuestionId == null) ? "decimal[]" : "decimal[][]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    return (question.LinkedToQuestionId == null) ? "decimal?" : "decimal[]";

                case QuestionType.TextList:
                    return "Tuple<decimal, string>[]";

                case QuestionType.GpsCoordinates:
                    return "GeoLocation";

                case QuestionType.Multimedia:
                    return "string";

                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }

        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression,
            Dictionary<string, Guid> variableNames)
        {
            var identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(conditionExpression);

            return new List<Guid>(
                from variable in identifiersUsedInExpression
                where variableNames.ContainsKey(variable)
                select variableNames[variable]);
        }

        private string GenerateTypeNameByScope(string rosterClassNamePrefix, IEnumerable<Guid> currentRosterScope, Dictionary<string, string> generatedScopesTypeNames)
        {
            string scopeStringKey = String.Join("$", currentRosterScope);
            if (!generatedScopesTypeNames.ContainsKey(scopeStringKey))
                generatedScopesTypeNames.Add(scopeStringKey, "@__" + rosterClassNamePrefix + Guid.NewGuid().FormatGuid());

            return generatedScopesTypeNames[scopeStringKey];
        }
    }
}