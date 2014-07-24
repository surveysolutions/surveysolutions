using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExecutorTemplateModel
    {
        public Guid Id { private set; get; }

        public List<QuestionTemplateModel> Questions { private set; get; }

        public List<RosterTemplateModel> Rosters { private set; get; }
        public List<GroupTemplateModel> Groups { private set; get; }

        public Dictionary<Guid, Guid[]> ParentsMap { private set; get; }
        public Dictionary<Guid, Guid[]> RostersIdToScopeMap { private set; get; }
        
        public Dictionary<Guid, List<Guid>> ConditionalDependencies { private set; get; }

        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel = new QuestionnaireLevelTemplateModel();

        public QuestionnaireExecutorTemplateModel(QuestionnaireDocument questionnaireDocument)
        {
            ConditionalDependencies = this.BuildDependencyTree(questionnaireDocument);

            this.Id = questionnaireDocument.PublicKey;

            this.Questions =
                questionnaireDocument.GetAllQuestions<AbstractQuestion>()
                    .Select(
                        qestion =>
                            new QuestionTemplateModel
                            {
                                Id = qestion.PublicKey,
                                VariableName = qestion.StataExportCaption,
                                Conditions = qestion.ConditionExpression,
                                Validations = qestion.ValidationExpression,
                                QuestionType = qestion.QuestionType,

                                GeneratedQuestionTypeName = this.GenerateQuestionTypeName(qestion),
                                GeneratedQuestionMemberName = "@__" + qestion.StataExportCaption,
                                GeneratedQuestionStateName = qestion.StataExportCaption + "_state"
                            }).ToList();

            this.Rosters =
                questionnaireDocument.Find<IGroup>(x => x.IsRoster)
                    .Select(
                        roster =>
                            new RosterTemplateModel()
                            {
                                Id = roster.PublicKey,
                                Conditions = roster.ConditionExpression,
                                VariableName = roster.PublicKey.ToString(), //waiting for merge roster name from default
                                RosterGeneratedTypeName = roster.PublicKey.ToString() + "_type"
                            })
                    .ToList();

            this.Groups =
                questionnaireDocument.Find<IGroup>(x => x.IsRoster != true)
                    .Select(
                        group =>
                            new GroupTemplateModel()
                            {
                                Id = group.PublicKey,
                                Conditions = group.ConditionExpression,
                                VariableName =  "@" + group.PublicKey.ToString(), //generating variable name by publicKey
                                GeneratedGroupStateName = group.PublicKey.ToString() + "_state"
                            })
                    .ToList();


        }

        private Dictionary<Guid, List<Guid>> BuildDependencyTree(QuestionnaireDocument questionnaireDocument)
        {
            Dictionary<Guid, List<Guid>> dependencies = questionnaireDocument
                .GetAllGroups()
                .ToDictionary(group => @group.PublicKey, group => @group.Children.Select(x => x.PublicKey).ToList());

            questionnaireDocument
                .GetAllQuestions<IQuestion>()
                .ToDictionary(group => @group.PublicKey, group => new List<Guid>())
                .ToList()
                .ForEach(x => dependencies.Add(x.Key, x.Value));

            Dictionary<Guid, List<Guid>> invertedDependencies = questionnaireDocument.GetAllGroups()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, questionnaireDocument));

            questionnaireDocument.GetAllQuestions<IQuestion>()
                .Where(x => !string.IsNullOrWhiteSpace(x.ConditionExpression))
                .ToDictionary(x => x.PublicKey,
                    x => this.GetIdsOfQuestionsInvolvedInExpression(x.ConditionExpression, questionnaireDocument))
                .ToList()
                .ForEach(x => invertedDependencies.Add(x.Key, x.Value));

            foreach (KeyValuePair<Guid, List<Guid>> dependency in invertedDependencies)
            {
                dependency.Value.ForEach(x => dependencies[x].Add(dependency.Key));
            }

            return dependencies;
        }

        private List<Guid> GetIdsOfQuestionsInvolvedInExpression(string conditionExpression, QuestionnaireDocument questionnaireDocument)
        {
            return new List<Guid>();
        }

        private string GenerateQuestionTypeName(AbstractQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    return "string";

                case QuestionType.AutoPropagate:
                    return "long?";

                case QuestionType.Numeric:
                    return (question as NumericQuestion).IsInteger ? "long?" : "decimal?";

                case QuestionType.QRBarcode:
                    return "string";

                // does linked multioption have scence?
                case QuestionType.MultyOption:
                    return (question.LinkedToQuestionId == null) ? "decimal[]" : "decimal[][]";

                case QuestionType.DateTime:
                    return "DateTime?";

                // does linked singleoption have scence in conditions?
                case QuestionType.SingleOption:
                    return (question.LinkedToQuestionId == null) ? "decimal?" : "decimal[]";

                // does text list have scence?
                case QuestionType.TextList:
                    return "Tuple<decimal, string>[]";

                //TODO: should be fixed with custom type
                case QuestionType.GpsCoordinates:
                    return "decimal?";
                default:
                    throw new ArgumentException("Unknown question type.");
            }
        }
    }


    public interface IParent
    {
        IParent GetParent();
        string GetTypeName();
        IEnumerable<QuestionTemplateModel> GetQuestions();
    }
}
