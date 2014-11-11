using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private const string InterviewExpressionStatePrefix = "InterviewExpressionState";

        private IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        public string Generate(QuestionnaireDocument questionnaire)
        {
            QuestionnaireExecutorTemplateModel questionnaireTemplateStructure =
                CreateQuestionnaireExecutorTemplateModel(questionnaire, true);
            var template = new InterviewExpressionStateTemplate(questionnaireTemplateStructure);

            return template.TransformText();
        }

        public Dictionary<string, string> GenerateEvaluator(QuestionnaireDocument questionnaire)
        {
            var generatedClasses = new Dictionary<string, string>();

            QuestionnaireExecutorTemplateModel questionnaireTemplateStructure =
                CreateQuestionnaireExecutorTemplateModel(questionnaire, false);
            var template = new InterviewExpressionStateTemplate(questionnaireTemplateStructure);

            generatedClasses.Add(new ExpressionLocation
            {
                ItemType = ItemType.Questionnaire,
                ExpressionType = ExpressionType.General,
                Id = questionnaire.PublicKey
            }.ToString(), template.TransformText());

            //generating partial classes
            GenerateQuestionnaryLevelExpressionClasses(questionnaireTemplateStructure, generatedClasses);
            GenerateRostersPartialClasses(questionnaireTemplateStructure, generatedClasses);

            return generatedClasses;
        }

        private static void GenerateRostersPartialClasses(QuestionnaireExecutorTemplateModel questionnaireTemplateStructure,
            Dictionary<string, string> generatedClasses)
        {
            foreach (var grouppedRosters in questionnaireTemplateStructure.RostersGroupedByScope)
            {
                foreach (
                    QuestionTemplateModel questionTemplateModel in grouppedRosters.Value.SelectMany(r => r.Questions))
                {
                    if (!string.IsNullOrWhiteSpace(questionTemplateModel.Conditions))
                    {
                        var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                        {
                            ExpressionString = questionTemplateModel.Conditions,
                            GeneratedClassName = grouppedRosters.Key,
                            GeneratedMethodName = questionTemplateModel.GeneratedConditionsMethodName
                        });

                        generatedClasses.Add(
                            new ExpressionLocation
                            {
                                ItemType = ItemType.Question,
                                ExpressionType = ExpressionType.Conditions,
                                Id = questionTemplateModel.Id
                            }.ToString(), methodTemplate.TransformText());
                    }

                    if (!string.IsNullOrWhiteSpace(questionTemplateModel.Validations))
                    {
                        var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                        {
                            ExpressionString = questionTemplateModel.Validations,
                            GeneratedClassName = grouppedRosters.Key,
                            GeneratedMethodName = questionTemplateModel.GeneratedValidationsMethodName
                        });

                        generatedClasses.Add(new ExpressionLocation
                        {
                            ItemType = ItemType.Question,
                            ExpressionType = ExpressionType.Validations,
                            Id = questionTemplateModel.Id
                        }.ToString(), methodTemplate.TransformText());
                    }
                }

                foreach (GroupTemplateModel groupTemplateModel in grouppedRosters.Value.SelectMany(r => r.Groups))
                {
                    if (!string.IsNullOrWhiteSpace(groupTemplateModel.Conditions))
                    {
                        var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                        {
                            ExpressionString = groupTemplateModel.Conditions,
                            GeneratedClassName = grouppedRosters.Key,
                            GeneratedMethodName = groupTemplateModel.GeneratedConditionsMethodName
                        });

                        generatedClasses.Add(
                            new ExpressionLocation
                            {
                                ItemType = ItemType.Group,
                                ExpressionType = ExpressionType.Conditions,
                                Id = groupTemplateModel.Id
                            }.ToString(), methodTemplate.TransformText());
                    }
                }

                foreach (RosterTemplateModel rosterTemplateModel in grouppedRosters.Value)
                {
                    if (!string.IsNullOrWhiteSpace(rosterTemplateModel.Conditions))
                    {
                        var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                        {
                            ExpressionString = rosterTemplateModel.Conditions,
                            GeneratedClassName = grouppedRosters.Key,
                            GeneratedMethodName = rosterTemplateModel.GeneratedConditionsMethodName
                        });

                        generatedClasses.Add(
                            new ExpressionLocation
                            {
                                ItemType = ItemType.Roster,
                                ExpressionType = ExpressionType.Conditions,
                                Id = rosterTemplateModel.Id
                            }.ToString(), methodTemplate.TransformText());
                    }
                }
            }
        }

        private static void GenerateQuestionnaryLevelExpressionClasses(
            QuestionnaireExecutorTemplateModel questionnaireTemplateStructure, Dictionary<string, string> generatedClasses)
        {
            foreach (QuestionTemplateModel questionTemplateModel in questionnaireTemplateStructure.QuestionnaireLevelModel.Questions)
            {
                if (!string.IsNullOrWhiteSpace(questionTemplateModel.Conditions))
                {
                    var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                    {
                        ExpressionString = questionTemplateModel.Conditions,
                        GeneratedClassName = questionnaireTemplateStructure.QuestionnaireLevelModel.GeneratedTypeName,
                        GeneratedMethodName = questionTemplateModel.GeneratedConditionsMethodName
                    });

                    generatedClasses.Add(
                        new ExpressionLocation
                        {
                            ItemType = ItemType.Question,
                            ExpressionType = ExpressionType.Conditions,
                            Id = questionTemplateModel.Id
                        }.ToString(), methodTemplate.TransformText());
                }

                if (!string.IsNullOrWhiteSpace(questionTemplateModel.Validations))
                {
                    var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                    {
                        ExpressionString = questionTemplateModel.Validations,
                        GeneratedClassName = questionnaireTemplateStructure.QuestionnaireLevelModel.GeneratedTypeName,
                        GeneratedMethodName = questionTemplateModel.GeneratedValidationsMethodName
                    });

                    generatedClasses.Add(
                        new ExpressionLocation
                        {
                            ItemType = ItemType.Question,
                            ExpressionType = ExpressionType.Validations,
                            Id = questionTemplateModel.Id
                        }.ToString(), methodTemplate.TransformText());
                }
            }

            foreach (GroupTemplateModel groupTemplateModel in questionnaireTemplateStructure.QuestionnaireLevelModel.Groups)
            {
                if (!string.IsNullOrWhiteSpace(groupTemplateModel.Conditions))
                {
                    var methodTemplate = new ExpressionMethodTemplate(new ExpressionMethodModel
                    {
                        ExpressionString = groupTemplateModel.Conditions,
                        GeneratedClassName = questionnaireTemplateStructure.QuestionnaireLevelModel.GeneratedTypeName,
                        GeneratedMethodName = groupTemplateModel.GeneratedConditionsMethodName
                    });

                    generatedClasses.Add(
                        new ExpressionLocation
                        {
                            ItemType = ItemType.Group,
                            ExpressionType = ExpressionType.Conditions,
                            Id = groupTemplateModel.Id
                        }.ToString(), methodTemplate.TransformText());
                }
            }
        }

        public QuestionnaireExecutorTemplateModel CreateQuestionnaireExecutorTemplateModel(
            QuestionnaireDocument questionnaire, bool generateExpressionMethods)
        {
            var template = new QuestionnaireExecutorTemplateModel();
            template.GenerateEmbeddedExpressionMethods = generateExpressionMethods;
            var questionnaireLevelModel = new QuestionnaireLevelTemplateModel(template, generateExpressionMethods);
            string generatedClassName = string.Format("{0}_{1}", InterviewExpressionStatePrefix,
                Guid.NewGuid().FormatGuid());

            Dictionary<string, string> generatedScopesTypeNames;
            List<QuestionTemplateModel> allQuestions;
            List<GroupTemplateModel> allGroups;
            List<RosterTemplateModel> allRosters;

            BuildStructures(questionnaire, questionnaireLevelModel, out generatedScopesTypeNames, out allQuestions,
                out allGroups, out allRosters);
            Dictionary<string, List<RosterTemplateModel>> rostersGroupedByScope =
                allRosters.GroupBy(r => r.GeneratedTypeName).ToDictionary(g => g.Key, g => g.ToList());

            Dictionary<Guid, List<Guid>> structuralDependencies = questionnaire
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());

            Dictionary<string, Guid> variableNames = allQuestions.ToDictionary(q => q.VariableName, q => q.Id);
            foreach (RosterTemplateModel roster in allRosters)
            {
                if (!variableNames.ContainsKey(roster.VariableName))
                {
                    variableNames.Add(roster.VariableName, questionnaire.PublicKey);
                }
            }

            Dictionary<Guid, List<Guid>> conditionalDependencies = BuildConditionalDependencies(questionnaire,
                variableNames);

            template.Id = questionnaire.PublicKey;
            template.AllQuestions = allQuestions;
            template.AllGroups = allGroups;
            template.AllRosters = allRosters;
            template.GeneratedClassName = generatedClassName;
            template.GeneratedScopesTypeNames = generatedScopesTypeNames;
            template.RostersGroupedByScope = rostersGroupedByScope;
            template.ConditionalDependencies = conditionalDependencies;
            template.StructuralDependencies = structuralDependencies;
            template.QuestionnaireLevelModel = questionnaireLevelModel;
            template.VariableNames = variableNames;

            return template;
        }

        private void BuildStructures(QuestionnaireDocument questionnaireDoc,
            QuestionnaireLevelTemplateModel questionnaireLevelModel,
            out Dictionary<string, string> generatedScopesTypeNames,
            out List<QuestionTemplateModel> allQuestions, out List<GroupTemplateModel> allGroups,
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

                    var childAsIQuestion = child as IQuestion;
                    if (childAsIQuestion != null)
                    {
                        string varName = !String.IsNullOrEmpty(childAsIQuestion.StataExportCaption)
                            ? childAsIQuestion.StataExportCaption
                            : "__" + childAsIQuestion.PublicKey.FormatGuid();

                        var question = new QuestionTemplateModel
                        {
                            Id = childAsIQuestion.PublicKey,
                            VariableName = varName,
                            Conditions = childAsIQuestion.ConditionExpression,
                            Validations = childAsIQuestion.ValidationExpression,
                            QuestionType = childAsIQuestion.QuestionType,
                            GeneratedTypeName = GenerateQuestionTypeName(childAsIQuestion),
                            GeneratedMemberName = "@__" + varName,
                            GeneratedStateName = "@__" + varName + "_state",
                            GeneratedIdName = "@__" + varName + "_id",
                            GeneratedConditionsMethodName = "IsEnabled_" + varName,
                            GeneratedValidationsMethodName = "IsValid_" + varName,
                            GeneratedMandatoryMethodName = "IsManadatoryValid_" + varName,
                            IsMandatory = childAsIQuestion.Mandatory,
                            RosterScopeName = currentScope.GeneratedRosterScopeName
                        };

                        currentScope.Questions.Add(question);

                        allQuestions.Add(question);

                        continue;
                    }

                    var childAsIGroup = child as IGroup;
                    if (childAsIGroup != null)
                    {
                        if (IsRosterGroup(childAsIGroup))
                        {
                            Guid currentScopeId = childAsIGroup.RosterSizeSource == RosterSizeSourceType.FixedTitles
                                ? childAsIGroup.PublicKey
                                : childAsIGroup.RosterSizeQuestionId.Value;

                            List<Guid> currentRosterScope = currentScope.RosterScope.Select(t => t).ToList();
                            currentRosterScope.Add(currentScopeId);

                            string varName = !String.IsNullOrWhiteSpace(childAsIGroup.VariableName)
                                ? childAsIGroup.VariableName
                                : "__" + childAsIGroup.PublicKey.FormatGuid();

                            var roster = new RosterTemplateModel
                            {
                                Id = childAsIGroup.PublicKey,
                                Conditions = childAsIGroup.ConditionExpression,
                                VariableName = varName,
                                GeneratedTypeName =
                                    GenerateTypeNameByScope(currentRosterScope, generatedScopesTypeNames),
                                GeneratedStateName = "@__" + varName + "_state",
                                ParentScope = currentScope,
                                GeneratedIdName = "@__" + varName + "_id",
                                GeneratedConditionsMethodName = "IsEnabled_" + varName,
                                RosterScope = currentRosterScope,
                                GeneratedRosterScopeName = "@__" + varName + "_scope",
                            };

                            rostersToProcess.Enqueue(new Tuple<IGroup, RosterScopeBaseModel>(childAsIGroup, roster));
                            allRosters.Add(roster);
                            currentScope.Rosters.Add(roster);
                        }
                        else
                        {
                            string varName = childAsIGroup.PublicKey.FormatGuid();
                            var group =
                                new GroupTemplateModel
                                {
                                    Id = childAsIGroup.PublicKey,
                                    Conditions = childAsIGroup.ConditionExpression,
                                    VariableName = "@__" + varName, //generating variable name by publicKey
                                    GeneratedStateName = "@__" + varName + "_state",
                                    GeneratedIdName = "@__" + varName + "_id",
                                    GeneratedConditionsMethodName = "IsEnabled_" + varName,
                                    RosterScopeName = currentScope.GeneratedRosterScopeName
                                };

                            currentScope.Groups.Add(group);
                            allGroups.Add(group);
                            foreach (IComposite childGroup in childAsIGroup.Children)
                            {
                                childrenOfCurrentRoster.Enqueue(childGroup);
                            }
                        }
                    }
                }
            }
        }

        private string GenerateQuestionTypeName(IQuestion question)
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

        private Dictionary<Guid, List<Guid>> BuildConditionalDependencies(QuestionnaireDocument questionnaireDocument,
            Dictionary<string, Guid> variableNames)
        {
            Dictionary<Guid, List<Guid>> dependencies = questionnaireDocument.GetAllGroups()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, variableNames));

            questionnaireDocument.GetEntitiesByType<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, variableNames))
                .ToList()
                .ForEach(x => dependencies.Add(x.Key, x.Value));

            return dependencies;
        }

        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression,
            Dictionary<string, Guid> variableNames)
        {
            return new List<Guid>(
                from variable in ExpressionProcessor.GetIdentifiersUsedInExpression(conditionExpression)
                where variableNames.ContainsKey(variable)
                select variableNames[variable]);
        }

        private static bool IsRosterGroup(IGroup group)
        {
            return group.IsRoster || group.Propagated == Propagate.AutoPropagated;
        }

        private string GenerateTypeNameByScope(IEnumerable<Guid> currentRosterScope,
            Dictionary<string, string> generatedScopesTypeNames)
        {
            string scopeStringKey = String.Join("$", currentRosterScope);
            if (!generatedScopesTypeNames.ContainsKey(scopeStringKey))
                generatedScopesTypeNames.Add(scopeStringKey, "@__" + Guid.NewGuid().FormatGuid());

            return generatedScopesTypeNames[scopeStringKey];
        }
    }
}