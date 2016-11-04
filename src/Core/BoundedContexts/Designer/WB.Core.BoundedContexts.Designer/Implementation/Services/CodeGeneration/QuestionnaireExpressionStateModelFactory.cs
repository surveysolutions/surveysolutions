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
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class QuestionnaireExpressionStateModelFactory
    {
        private readonly IExpressionProcessor expressionProcessor;
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        private readonly ILookupTableService lookupTableService;

        public QuestionnaireExpressionStateModelFactory(
            IMacrosSubstitutionService macrosSubstitutionService,
            IExpressionProcessor expressionProcessor,
            ILookupTableService lookupTableService)
        {
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.expressionProcessor = expressionProcessor;
            this.lookupTableService = lookupTableService;
        }

        public QuestionnaireExpressionStateModel CreateQuestionnaireExecutorTemplateModel(
           QuestionnaireDocument questionnaire,
           CodeGenerationSettings codeGenerationSettings)
        {
            var expressionState = new QuestionnaireExpressionStateModel
            {
                AdditionalInterfaces = codeGenerationSettings.AdditionInterfaces,
                Namespaces = codeGenerationSettings.Namespaces,
                Id = questionnaire.PublicKey,
                ClassName = $"{CodeGenerator.InterviewExpressionStatePrefix}_{Guid.NewGuid().FormatGuid()}",
                QuestionnaireLevelModel = new QuestionnaireLevelTemplateModel(),
                LookupTables = BuildLookupTableModels(questionnaire).ToList(),
                StructuralDependencies = BuildStructuralDependencies(questionnaire),
                ConditionalDependencies = BuildConditionalDependencies(questionnaire),
                RosterDependencies = BuildRosterDependencies(questionnaire),
            };

            this.TraverseQuestionnaireAndUpdateExpressionStateWithBuiltModels(questionnaire, expressionState);

            expressionState.ConditionsPlayOrder = BuildConditionsPlayOrder(
                expressionState.ConditionalDependencies,
                expressionState.StructuralDependencies,
                expressionState.RosterDependencies,
                expressionState.LinkedQuestionByRosterDependencies);

            expressionState.QuestionnaireLevelModel.ConditionMethodsSortedByExecutionOrder = GetConditionMethodsSortedByExecutionOrder(
                expressionState.QuestionnaireLevelModel.Questions,
                expressionState.QuestionnaireLevelModel.StaticTexts,
                expressionState.QuestionnaireLevelModel.Groups,
                null,
                expressionState.QuestionnaireLevelModel.LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter,
                expressionState.ConditionsPlayOrder);

            var rosterGroupedByScope = expressionState.AllRosters.GroupBy(r => r.TypeName);

            expressionState.RostersGroupedByScope = rosterGroupedByScope
                .Select(x => this.BuildRosterScopeTemplateModel(x.Key, x.ToList(), expressionState))
                .ToDictionary(x => x.TypeName);

            UpdateReferencesOnParentRosters(expressionState.RostersGroupedByScope, expressionState.QuestionnaireLevelModel);

            UpdateReferencesOnParentQuestionsAndVariables(expressionState.RostersGroupedByScope, expressionState.QuestionnaireLevelModel);

            expressionState.MethodModels = BuildMethodModels(codeGenerationSettings, expressionState);

            expressionState.CategoricalOptionsFilterModels = BuildCategoricalOptionsFilterModels(codeGenerationSettings, expressionState);
            expressionState.LinkedFilterModels = BuildLinkedQuestionFilterModels(codeGenerationSettings, expressionState);


            return expressionState;
        }

        public static Dictionary<string, OptionsFilterConditionDescriptionModel> BuildCategoricalOptionsFilterModels(
            CodeGenerationSettings codeGenerationSettings,
            QuestionnaireExpressionStateModel questionnaireTemplate)
        {
            var methodModels = new Dictionary<string, OptionsFilterConditionDescriptionModel>();
            foreach (var question in questionnaireTemplate.AllQuestions)
            {
                if (question.HasOptionsFilter)
                {
                    methodModels.Add(ExpressionLocation.CategoricalQuestionFilter(question.Id).Key, new OptionsFilterConditionDescriptionModel(
                        question.ParentScopeTypeName,
                        question.OptionsFilterMethodName,
                        codeGenerationSettings.Namespaces,
                        question.OptionsFilterExpression,
                        question.VariableName));
                }
            }
            return methodModels;
        }

        public static Dictionary<string, LinkedFilterConditionDescriptionModel> BuildLinkedQuestionFilterModels(
            CodeGenerationSettings codeGenerationSettings,
            QuestionnaireExpressionStateModel questionnaireTemplate)
        {
            var methodModels = new Dictionary<string, LinkedFilterConditionDescriptionModel>();
            foreach (var filter in questionnaireTemplate.AllLinkedQuestionFilters)
            {
                var linkedQuestion = questionnaireTemplate.AllQuestions.Single(x => x.Id == filter.LinkedQuestionId);

                methodModels.Add(ExpressionLocation.LinkedQuestionFilter(filter.LinkedQuestionId).Key, new LinkedFilterConditionDescriptionModel(
                    filter.ParentScopeTypeName,
                    filter.FilterForLinkedQuestionMethodName,
                    codeGenerationSettings.Namespaces,
                    filter.FilterExpression,
                    linkedQuestion.ParentScopeTypeName));
            }
            return methodModels;
        }
        public static Dictionary<string, ConditionDescriptionModel> BuildMethodModels(
            CodeGenerationSettings codeGenerationSettings,
            QuestionnaireExpressionStateModel questionnaireTemplate)
        {
            var methodModels = new Dictionary<string, ConditionDescriptionModel>();

            foreach (var question in questionnaireTemplate.AllQuestions)
            {
                if (!string.IsNullOrWhiteSpace(question.Condition))
                {
                    methodModels.Add(ExpressionLocation.QuestionCondition(question.Id).Key, new ConditionDescriptionModel(
                        question.ParentScopeTypeName,
                        question.ConditionMethodName,
                        codeGenerationSettings.Namespaces,
                        question.Condition,
                        false,
                        question.VariableName));
                }
                foreach (var validation in question.ValidationExpressions)
                {
                    if (!string.IsNullOrWhiteSpace(validation.ValidationExpression))
                    {
                        methodModels.Add(ExpressionLocation.QuestionValidation(question.Id, validation.Order).Key,
                            new ConditionDescriptionModel(
                                question.ParentScopeTypeName,
                                validation.ValidationMethodName,
                                codeGenerationSettings.Namespaces,
                                validation.ValidationExpression,
                                true,
                                validation.VariableName));
                    }
                }
            }

            foreach (GroupTemplateModel group in questionnaireTemplate.AllGroups.Where(x => !string.IsNullOrWhiteSpace(x.Condition)))
            {
                methodModels.Add(ExpressionLocation.GroupCondition(group.Id).Key, new ConditionDescriptionModel(
                    group.ParentScopeTypeName,
                    group.ConditionMethodName,
                    codeGenerationSettings.Namespaces,
                    group.Condition,
                    false,
                    group.VariableName));
            }

            foreach (var staticText in questionnaireTemplate.AllStaticTexts)
            {
                if (!string.IsNullOrWhiteSpace(staticText.Condition))
                {
                    methodModels.Add(
                        ExpressionLocation.StaticTextCondition(staticText.Id).Key,
                        new ConditionDescriptionModel(
                            staticText.ParentScopeTypeName,
                            staticText.ConditionMethodName,
                            codeGenerationSettings.Namespaces,
                            staticText.Condition,
                            false,
                            staticText.VariableName));
                }

                foreach (var validation in staticText.ValidationExpressions.Where(x => !string.IsNullOrWhiteSpace(x.ValidationExpression)))
                {
                    methodModels.Add(
                        ExpressionLocation.StaticTextValidation(staticText.Id, validation.Order).Key,
                        new ConditionDescriptionModel(
                            staticText.ParentScopeTypeName,
                            validation.ValidationMethodName,
                            codeGenerationSettings.Namespaces,
                            validation.ValidationExpression,
                            false,
                            validation.VariableName));
                }
            }

            foreach (RosterTemplateModel roster in questionnaireTemplate.AllRosters.Where(x => !string.IsNullOrWhiteSpace(x.Conditions)))
            {
                methodModels.Add(ExpressionLocation.RosterCondition(roster.Id).Key,
                    new ConditionDescriptionModel(
                    roster.TypeName,
                    roster.ConditionsMethodName,
                    codeGenerationSettings.Namespaces,
                    roster.Conditions,
                    false,
                    roster.VariableName));
            }

            foreach (var variable in questionnaireTemplate.AllVariables)
            {
                methodModels.Add(ExpressionLocation.Variable(variable.Id).Key, new ConditionDescriptionModel(

                    variable.ParentScopeTypeName,
                    variable.ExpressionMethodName,
                    codeGenerationSettings.Namespaces,
                    variable.Expression,
                    false,
                    variable.VariableName,
                    returnType: variable.TypeName));
            }

            return methodModels;
        }

        public RosterScopeTemplateModel BuildRosterScopeTemplateModel(
            string rosterScopeType,
            List<RosterTemplateModel> rostersInScope,
            QuestionnaireExpressionStateModel template)
        {
            var groups = rostersInScope.SelectMany(r => r.Groups).ToList();
            var questions = rostersInScope.SelectMany(r => r.Questions).ToList();
            var staticTexts = rostersInScope.SelectMany(r => r.StaticTexts).ToList();
            var rosters = rostersInScope.SelectMany(r => r.Rosters).ToList();
            var variables = rostersInScope.SelectMany(r => r.Variables).ToList();
            var linkedQuestions = rostersInScope.SelectMany(x => x.LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter).ToList();
            var linkedQuestionsIdNames = rostersInScope.SelectMany(x => x.LinkedQuestionsIdNames).ToList();

            var linkedQuestionFilterExpressions = rostersInScope.SelectMany(x => x.LinkedQuestionFilterExpressions).ToList();

            var conditionMethodsSortedByExecutionOrder = GetConditionMethodsSortedByExecutionOrder(
                questions, staticTexts, groups, rostersInScope, linkedQuestions, template.ConditionsPlayOrder);

            return new RosterScopeTemplateModel(rosterScopeType, questions, staticTexts, groups, rosters, rostersInScope,
                conditionMethodsSortedByExecutionOrder, linkedQuestionFilterExpressions, variables, linkedQuestionsIdNames);
        }

        public static List<ConditionMethodAndState> GetConditionMethodsSortedByExecutionOrder(
            List<QuestionTemplateModel> questions,
            List<StaticTextTemplateModel> staticTexts,
            List<GroupTemplateModel> groups,
            List<RosterTemplateModel> rosters,
            List<LinkedQuestionVerifierModel> linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter,
            List<Guid> conditionsPlayOrder)
        {
            var groupsWithConditions = groups
                .Where(x => !string.IsNullOrWhiteSpace(x.Condition))
                .Reverse()
                .Select(x => new { x.Id, Condition = new ConditionMethodAndState(x.ConditionMethodName, x.StateName) });

            var rostersWithConditions = (rosters ?? new List<RosterTemplateModel>())
                .Where(x => !string.IsNullOrWhiteSpace(x.Conditions))
                .Reverse()
                .Select(x => new { x.Id, Condition = new ConditionMethodAndState(x.ConditionsMethodName, x.StateName) });

            var staticTextsWithConditions = staticTexts
                .Where(x => !string.IsNullOrWhiteSpace(x.Condition))
                .Reverse()
                .Select(x => new { x.Id, Condition = new ConditionMethodAndState(x.ConditionMethodName, x.StateName) });

            var questionsWithConditions = questions
                .Where(x => !string.IsNullOrWhiteSpace(x.Condition))
                .Select(x => new { x.Id, Condition = new ConditionMethodAndState(x.ConditionMethodName, x.StateName) });

            var questionsWithOptionsFilter = questions
                .Where(x => !string.IsNullOrWhiteSpace(x.OptionsFilterExpression))
                .Select(x => new
                {
                    x.Id,
                    Condition = new ConditionMethodAndState(x.OptionsFilterMethodName, x.StateName, true, x.MemberName, x.TypeName)
                });

            var linkedQuestionsWithConditions = linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter
                .Select(x => new
                {
                    x.Id,
                    Condition = new ConditionMethodAndState($"IdOf.{x.RosterScopeName}", x.StateName, true, x.MemberName, x.TypeName)
                });

            var itemsToSort = groupsWithConditions
                .Union(rostersWithConditions)
                .Union(questionsWithConditions)
                .Union(questionsWithOptionsFilter)
                .Union(linkedQuestionsWithConditions)
                .ToList();

            var itemsSorted = new List<ConditionMethodAndState>();

            foreach (Guid id in conditionsPlayOrder)
            {
                itemsSorted.AddRange(itemsToSort.Where(x => x.Id == id).Select(x => x.Condition));
            }

            itemsSorted.AddRange(staticTextsWithConditions.Select(st => st.Condition));

            return itemsSorted;
        }

        public static List<Guid> BuildConditionsPlayOrder(Dictionary<Guid, List<Guid>> conditionalDependencies,
            Dictionary<Guid, List<Guid>> structuralDependencies,
            Dictionary<Guid, List<Guid>> rosterDependencies,
            Dictionary<Guid, Guid> linkedQuestionByRosterDependencies)
        {
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

        public static Dictionary<Guid, List<Guid>> BuildStructuralDependencies(QuestionnaireDocument questionnaire)
        {
            return questionnaire
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());
        }

        public static void UpdateReferencesOnParentRosters(
            Dictionary<string, RosterScopeTemplateModel> rostersGroupedByScope,
            QuestionnaireLevelTemplateModel questionnaireLevelModel)
        {
            foreach (RosterScopeTemplateModel rosterScopeModel in rostersGroupedByScope.Values)
            {
                var parentRosters = GetParentRosters(rosterScopeModel, rostersGroupedByScope).ToList();

                var allParentsRostersToTop = parentRosters
                    .SelectMany(x => x.Rosters)
                    .Union(questionnaireLevelModel.Rosters)
                    .Select(x => new HierarchyReferenceModel { TypeName = x.TypeName, VariableName = x.VariableName }).ToList();

                rosterScopeModel.AllParentsRostersToTop = allParentsRostersToTop;
            }
        }

        public static void UpdateReferencesOnParentQuestionsAndVariables(
           Dictionary<string, RosterScopeTemplateModel> rostersGroupedByScope,
           QuestionnaireLevelTemplateModel questionnaireLevelModel)
        {
            foreach (RosterScopeTemplateModel rosterScopeModel in rostersGroupedByScope.Values)
            {
                var parentRosters = GetParentRosters(rosterScopeModel, rostersGroupedByScope).ToList();

                var allParentsQuestionsToTop = parentRosters
                    .SelectMany(x => x.Questions)
                    .Union(questionnaireLevelModel.Questions)
                    .Select(x => new HierarchyReferenceModel { TypeName = x.TypeName, VariableName = x.VariableName }).ToList();

                var allParentsVariablesToTop = parentRosters
                    .SelectMany(x => x.Variables)
                    .Union(questionnaireLevelModel.Variables)
                    .Select(x => new HierarchyReferenceModel { TypeName = x.TypeName, VariableName = x.VariableName })
                    .ToList();

                rosterScopeModel.AllParentsQuestionsToTop = allParentsQuestionsToTop;
                rosterScopeModel.AllParentsVariablesToTop = allParentsVariablesToTop;
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

        public IEnumerable<LookupTableTemplateModel> BuildLookupTableModels(QuestionnaireDocument questionnaire)
        {
            foreach (var table in questionnaire.LookupTables)
            {
                var lookupTableData = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, table.Key);
                var tableName = table.Value.TableName;
                var tableTemplateModel = new LookupTableTemplateModel
                {
                    TableName = tableName,
                    TypeName = CodeGenerator.LookupPrefix + tableName.ToPascalCase(),
                    TableNameField = CodeGenerator.PrivateFieldsPrefix + tableName.ToCamelCase(),
                    Rows = lookupTableData.Rows,
                    VariableNames = lookupTableData.VariableNames
                };
                yield return tableTemplateModel;
            }
        }

        public void TraverseQuestionnaireAndUpdateExpressionStateWithBuiltModels(QuestionnaireDocument questionnaireDoc, QuestionnaireExpressionStateModel expressionState)
        {
            var scopesTypeNames = new Dictionary<string, string>();

            var rostersToProcess = new Queue<Tuple<IGroup, RosterScopeBaseModel>>();
            rostersToProcess.Enqueue(new Tuple<IGroup, RosterScopeBaseModel>(questionnaireDoc, expressionState.QuestionnaireLevelModel));

            var linkedQuestions = CreateLinkedQuestionFilterExpressionModels(questionnaireDoc);
            var soursesOfLinkedQuestionsInMultiRosters = new List<Guid>();
            var likedQuestionToRosterMap = new Dictionary<Guid, Guid>();
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
                        var childAsQuestion = ((IQuestion)child);

                        var question = this.CreateQuestionTemplateModel(
                            questionnaireDoc, childAsQuestion,
                            currentScope.RosterScopeName,
                            currentScope.TypeName);

                        var linkedQuestionReferencedCurrentQuestion = questionnaireDoc
                            .Find<IQuestion>(x => x.LinkedToQuestionId == child.PublicKey)
                            .ToList();

                        var isQuestionIsSourceForLinkedQuestion = linkedQuestionReferencedCurrentQuestion.Any();
                        if (isQuestionIsSourceForLinkedQuestion)
                        {
                            foreach (var linkedQuestion in linkedQuestionReferencedCurrentQuestion)
                            {
                                likedQuestionToRosterMap.Add(linkedQuestion.PublicKey, rosterScope.Item1.PublicKey);
                            }

                            if (IsRosterTriggeredByQuestionWithFilter(questionnaireDoc, rosterScope.Item1.RosterSizeQuestionId))
                            {
                                soursesOfLinkedQuestionsInMultiRosters.Add(child.PublicKey);
                            }
                        }

                        currentScope.Questions.Add(question);

                        if (childAsQuestion.LinkedToQuestionId.HasValue || childAsQuestion.LinkedToRosterId.HasValue)
                        {
                            currentScope.LinkedQuestionsIdNames.Add(question.IdName);
                        }

                        expressionState.AllQuestions.Add(question);

                        continue;
                    }
                    if (child is IVariable)
                    {
                        var variable = this.CreateVariableTemplateModel((IVariable)child, currentScope, questionnaireDoc);

                        currentScope.Variables.Add(variable);

                        expressionState.AllVariables.Add(variable);

                        continue;
                    }
                    if (child is IStaticText)
                    {
                        var staticTextModel = this.CreateStaticTextTemplateModel((IStaticText)child, currentScope, questionnaireDoc);

                        currentScope.StaticTexts.Add(staticTextModel);

                        expressionState.AllStaticTexts.Add(staticTextModel);

                        continue;
                    }

                    if (!IsGroup(child))
                        continue;

                    var childAsGroup = (IGroup)child;

                    if (IsRoster(childAsGroup))
                    {
                        var roster = this.CreateRosterTemplateModel(questionnaireDoc, scopesTypeNames, childAsGroup, currentScope, linkedQuestions);

                        rostersToProcess.Enqueue(new Tuple<IGroup, RosterScopeBaseModel>(childAsGroup, roster));

                        expressionState.AllRosters.Add(roster);

                        expressionState.AllLinkedQuestionFilters.AddRange(roster.LinkedQuestionFilterExpressions);

                        currentScope.Rosters.Add(roster);
                    }
                    else
                    {
                        var group = this.CreateGroupTemplateModel(questionnaireDoc, childAsGroup, currentScope);

                        currentScope.Groups.Add(group);

                        expressionState.AllGroups.Add(group);

                        foreach (IComposite childGroup in childAsGroup.Children)
                        {
                            childrenOfCurrentRoster.Enqueue(childGroup);
                        }
                    }
                }
            }
            expressionState.LinkedQuestionByRosterDependencies = likedQuestionToRosterMap;
            var linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter = questionnaireDoc
                .Find<IQuestion>(x => x.LinkedToQuestionId.HasValue && soursesOfLinkedQuestionsInMultiRosters.Contains(x.LinkedToQuestionId.Value))
                .ToDictionary(x => x.PublicKey, x => x.LinkedToQuestionId);

            foreach (var roster in expressionState.AllRosters)
            {
                foreach (var question in roster.Questions)
                {
                    if (linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter.ContainsKey(question.Id))
                    {
                        roster.LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter
                            .Add(new LinkedQuestionVerifierModel
                            {
                                Id = question.Id,
                                MemberName = question.MemberName,
                                TypeName = question.TypeName,
                                StateName = question.StateName,
                                RosterScopeName = expressionState.AllQuestions.Single(q => q.Id == linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter[question.Id]).RosterScopeName
                            });
                    }
                }
            }

            foreach (var question in expressionState.QuestionnaireLevelModel.Questions)
            {
                if (linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter.ContainsKey(question.Id))
                {
                    expressionState.QuestionnaireLevelModel.LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter
                        .Add(new LinkedQuestionVerifierModel
                        {
                            Id = question.Id,
                            MemberName = question.MemberName,
                            TypeName = question.TypeName,
                            StateName = question.StateName,
                            RosterScopeName = expressionState.AllQuestions.Single(q => q.Id == linkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter[question.Id]).RosterScopeName
                        });
                }
            }
        }

        private static bool IsRosterTriggeredByQuestionWithFilter(
            QuestionnaireDocument questionnaireDoc,
            Guid? rosterSizeQuestionId)
        {
            var isRosterTriggeredByQuestion = rosterSizeQuestionId.HasValue;
            if (isRosterTriggeredByQuestion)
            {
                var rosterSizeQuestion = questionnaireDoc.Find<IQuestion>(rosterSizeQuestionId.Value);
                return !string.IsNullOrWhiteSpace(rosterSizeQuestion.Properties.OptionsFilterExpression);
            }

            return false;
        }

        private List<LinkedQuestionFilterExpressionModel> CreateLinkedQuestionFilterExpressionModels(QuestionnaireDocument questionnaireDoc)
        {
            var linkedQuestions = questionnaireDoc.Find<IQuestion>(q => q.LinkedToQuestionId.HasValue);
            var linkedQuestionsFilterModels = new List<LinkedQuestionFilterExpressionModel>();
            foreach (var linkedQuestion in linkedQuestions)
            {
                var linkedQuestionSource = questionnaireDoc.Find<IQuestion>(linkedQuestion.LinkedToQuestionId.Value);
                if (linkedQuestionSource == null)
                    continue;

                var parent = linkedQuestionSource.GetParent();
                while (parent != null)
                {
                    var parentGroup = parent as IGroup;
                    if (parentGroup != null && parentGroup.IsRoster)
                    {
                        var filterExpression = string.IsNullOrWhiteSpace(linkedQuestion.LinkedFilterExpression) ? "true" : this.macrosSubstitutionService.InlineMacros(
                            linkedQuestion.LinkedFilterExpression, questionnaireDoc.Macros.Values);

                        linkedQuestionsFilterModels.Add(
                            new LinkedQuestionFilterExpressionModel(
                                $"IsAnswered({linkedQuestionSource.StataExportCaption})&&({filterExpression})",
                                $"FilterForLinkedQuestion__{linkedQuestion.StataExportCaption}",
                                CodeGenerator.GetQuestionIdName(linkedQuestion.StataExportCaption), parentGroup.PublicKey, linkedQuestion.PublicKey));
                        break;
                    }
                    parent = parent.GetParent();
                }
            }

            var linkedToRosterQuestions = questionnaireDoc.Find<IQuestion>(q => q.LinkedToRosterId.HasValue).ToList();
            foreach (var linkedToRosterQuestion in linkedToRosterQuestions)
            {
                var sourceRoster = questionnaireDoc.Find<IGroup>(linkedToRosterQuestion.LinkedToRosterId.Value);
                if (sourceRoster == null)
                    continue;

                var filterExpression = string.IsNullOrWhiteSpace(linkedToRosterQuestion.LinkedFilterExpression)
                    ? "true"
                    : this.macrosSubstitutionService.InlineMacros(linkedToRosterQuestion.LinkedFilterExpression, questionnaireDoc.Macros.Values);

                var rosterState = CodeGenerator.PrivateFieldsPrefix + sourceRoster.VariableName +
                                  CodeGenerator.StateSuffix;

                string finalFilterExpression;
                if (sourceRoster.RosterTitleQuestionId != null)
                {

                    var titleQuestion = questionnaireDoc.Find<IQuestion>(sourceRoster.RosterTitleQuestionId.Value);
                    
                    finalFilterExpression = $"{rosterState}.State != State.Disabled && IsAnswered({titleQuestion.StataExportCaption}) && ({filterExpression})";
                }
                else
                {
                    finalFilterExpression = $"{rosterState}.State != State.Disabled && ({filterExpression})";
                }

                linkedQuestionsFilterModels.Add(
                        new LinkedQuestionFilterExpressionModel(
                            finalFilterExpression,
                            $"FilterForLinkedQuestion__{linkedToRosterQuestion.StataExportCaption}",
                            CodeGenerator.GetQuestionIdName(linkedToRosterQuestion.StataExportCaption),
                            sourceRoster.PublicKey, linkedToRosterQuestion.PublicKey));

            }

            return linkedQuestionsFilterModels.ToList();
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
            IQuestion question,
            string rosterScopeName,
            string parentScopeTypeName)
        {
            string varName = !String.IsNullOrEmpty(question.StataExportCaption)
                ? question.StataExportCaption
                : "__" + question.PublicKey.FormatGuid();

            var condition = question.CascadeFromQuestionId.HasValue
                ? this.GetConditionForCascadingQuestion(questionnaireDoc, question.PublicKey)
                : this.macrosSubstitutionService.InlineMacros(question.ConditionExpression, questionnaireDoc.Macros.Values);

            var validationExpressions = question
                .ValidationConditions
                .Select((validationCondition, index)
                    => new ValidationExpressionModel(
                        this.macrosSubstitutionService.InlineMacros(validationCondition.Expression, questionnaireDoc.Macros.Values),
                        varName,
                        index))
                .ToList();

            var optionsFilterExpression = this.macrosSubstitutionService.InlineMacros(question.Properties.OptionsFilterExpression, questionnaireDoc.Macros.Values);

            var questionModel = new QuestionTemplateModel
            {
                Id = question.PublicKey,
                VariableName = varName,
                Condition = condition,
                TypeName = GenerateQuestionTypeName(question),
                RosterScopeName = rosterScopeName,
                ParentScopeTypeName = parentScopeTypeName,
                ValidationExpressions = validationExpressions,
                OptionsFilterExpression = optionsFilterExpression
            };

            if (IsMultiQuestion(question))
            {
                var multyOptionsQuestion = question as IMultyOptionsQuestion;
                questionModel.IsMultiOptionYesNoQuestion = multyOptionsQuestion.YesNoView;
                if (questionModel.IsMultiOptionYesNoQuestion)
                {
                    questionModel.AllMultioptionYesNoCodes = multyOptionsQuestion.Answers.Select(x => x.AnswerValue).ToList();
                }
            }

            return questionModel;
        }

        private static bool IsMultiQuestion(IQuestion question)
        {
            return question.QuestionType == QuestionType.MultyOption && question is IMultyOptionsQuestion;
        }

        private RosterTemplateModel CreateRosterTemplateModel(
            QuestionnaireDocument questionnaireDoc,
            Dictionary<string, string> scopesTypeNames,
            IGroup childAsIGroup,
            RosterScopeBaseModel currentScope,
            List<LinkedQuestionFilterExpressionModel> linkedQuestions)
        {
            Guid currentScopeId = childAsIGroup.RosterSizeSource == RosterSizeSourceType.FixedTitles
                ? childAsIGroup.PublicKey
                : childAsIGroup.RosterSizeQuestionId.Value;

            List<Guid> currentRosterScope = currentScope.RosterScope.ToList();
            currentRosterScope.Add(currentScopeId);

            string varName = !String.IsNullOrWhiteSpace(childAsIGroup.VariableName)
                ? childAsIGroup.VariableName
                : "__" + childAsIGroup.PublicKey.FormatGuid();


            var typeName = this.GenerateTypeNameByScope($"{varName}_{childAsIGroup.PublicKey.FormatGuid()}_",
                currentRosterScope, scopesTypeNames);

            var linkedQuestionFilters = linkedQuestions.Where(q => q.RosterId == childAsIGroup.PublicKey).ToList();

            linkedQuestionFilters.ForEach(x => x.ParentScopeTypeName = typeName);

            var roster = new RosterTemplateModel(
                rosterScopeName: CodeGenerator.PrivateFieldsPrefix + varName + "_scope",
                typeName: typeName,
                rosterScope: currentRosterScope,
                parentTypeName: currentScope.TypeName,
                conditions: macrosSubstitutionService.InlineMacros(childAsIGroup.ConditionExpression, questionnaireDoc.Macros.Values),
                id: childAsIGroup.PublicKey,
                variableName: varName,
                parentScopeTypeName: currentScope.TypeName,
                linkedQuestionFilterExpressions: linkedQuestionFilters);

            return roster;
        }

        private GroupTemplateModel CreateGroupTemplateModel(
            QuestionnaireDocument questionnaireDoc,
            IGroup childAsIGroup,
            RosterScopeBaseModel currentScope)
            => new GroupTemplateModel
            {
                Id = childAsIGroup.PublicKey,
                Condition = this.macrosSubstitutionService.InlineMacros(childAsIGroup.ConditionExpression, questionnaireDoc.Macros.Values),
                VariableName = childAsIGroup.PublicKey.FormatGuid(),
                RosterScopeName = currentScope.RosterScopeName,
                ParentScopeTypeName = currentScope.TypeName,
            };

        private VariableTemplateModel CreateVariableTemplateModel(IVariable variable, RosterScopeBaseModel currentScope,
            QuestionnaireDocument questionnaire)
            =>
                new VariableTemplateModel(variable.PublicKey, variable.Name,
                    this.macrosSubstitutionService.InlineMacros(variable.Expression, questionnaire.Macros.Values),
                    CreateVariablesCSharpType(variable.Type), currentScope.RosterScopeName, currentScope.TypeName);

        private StaticTextTemplateModel CreateStaticTextTemplateModel(IStaticText staticText, RosterScopeBaseModel currentScope, QuestionnaireDocument questionnaire)
            => new StaticTextTemplateModel(
                staticText.PublicKey,
                this.macrosSubstitutionService.InlineMacros(staticText.ConditionExpression, questionnaire.Macros.Values),
                staticText
                    .ValidationConditions
                    .Select(validationCondition
                        => this.macrosSubstitutionService.InlineMacros(validationCondition.Expression, questionnaire.Macros.Values))
                    .ToList(),
                currentScope.RosterScopeName,
                currentScope.TypeName);

        private string CreateVariablesCSharpType(VariableType variableType)
        {
            switch (variableType)
            {
                case VariableType.LongInteger:
                    return "long?";
                case VariableType.Double:
                    return "double?";
                case VariableType.Boolean:
                    return "bool?";
                case VariableType.DateTime:
                    return "DateTime?";
                case VariableType.String:
                    return "string";
                default:
                    throw new ArgumentOutOfRangeException(nameof(variableType), variableType, $"unknown variable type {variableType}");
            }
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

            string childQuestionCondition = (string.IsNullOrWhiteSpace(conditionForChildCascadingQuestion) ? string.Empty : $" && {conditionForChildCascadingQuestion}");

            var valuesOfParentCascadingThatHaveChildOptions = childQuestion.Answers.Select(x => x.ParentValue).Distinct();
            var allValuesOfParentCascadingQuestion = parentQuestion.Answers.Select(x => x.AnswerValue);

            var parentOptionsThatHaveNoChildOptions = allValuesOfParentCascadingQuestion.Where(x => !valuesOfParentCascadingThatHaveChildOptions.Contains(x));

            var expressionToDisableChildThatHasNoOptionsForChosenParent = !parentOptionsThatHaveNoChildOptions.Any() ? string.Empty : GenerateExpressionToDisableChildThatHasNoOptionsForChosenParent(parentOptionsThatHaveNoChildOptions, parentQuestion.StataExportCaption);

            return $"!IsAnswerEmpty({parentQuestion.StataExportCaption})" + expressionToDisableChildThatHasNoOptionsForChosenParent + childQuestionCondition;
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

        static string GenerateExpressionToDisableChildThatHasNoOptionsForChosenParent(IEnumerable<string> parentOptionsThatHaveNoChildOptions, string stataExportCaption)
        {
            var joinedParentOptions = string.Join(",", parentOptionsThatHaveNoChildOptions.Select(x => $"{x}"));
            return $@" && !(new List<decimal?>(){{ {joinedParentOptions} }}.Contains({stataExportCaption}))";
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
                    return (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) ? "decimal[]" : "decimal[][]";

                case QuestionType.DateTime:
                    return "DateTime?";

                case QuestionType.SingleOption:
                    return (question.LinkedToQuestionId == null && question.LinkedToRosterId == null) ? "decimal?" : "decimal[]";

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

        private List<Guid> GetIdsOfEntitiesInvolvedInExpression(string conditionExpression, Dictionary<string, Guid> variableNames)
        {
            var identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(conditionExpression);

            return new List<Guid>(from variable in identifiersUsedInExpression where variableNames.ContainsKey(variable) select variableNames[variable]);
        }

        private string GenerateTypeNameByScope(string rosterClassNamePrefix, IEnumerable<Guid> currentRosterScope, Dictionary<string, string> scopesTypeNames)
        {
            string scopeStringKey = String.Join("$", currentRosterScope);
            if (!scopesTypeNames.ContainsKey(scopeStringKey))
                scopesTypeNames.Add(scopeStringKey, "@__" + rosterClassNamePrefix + Guid.NewGuid().FormatGuid());

            return scopesTypeNames[scopeStringKey];
        }
    }
}