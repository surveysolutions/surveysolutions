using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.Infrastructure.Compilation
{
    public class QuestionnaireExecutorTemplateModel
    {
        public Guid Id { private set; get; }

        public List<QuestionTemplateModel> Questions { private set; get; }
        public List<RosterTemplateModel> Rosters { private set; get; }
        public List<GroupTemplateModel> Groups { private set; get; }
        public Dictionary<Guid, Guid[]> ParentsMap { private set; get; }
        public Dictionary<Guid, Guid[]> ConditionalDependencies { private set; get; }

        public QuestionnaireExecutorTemplateModel(QuestionnaireDocument questionnaireDocument)
        {
            this.Id = questionnaireDocument.PublicKey;

            this.Questions =
                questionnaireDocument.GetAllQuestions<AbstractQuestion>()
                    .Select(
                        qestion =>
                            new QuestionTemplateModel()
                            {
                                Id = qestion.PublicKey,
                                VariableName = qestion.StataExportCaption,
                                Conditions = qestion.ConditionExpression,
                                Validations = qestion.ValidationExpression,
                                QuestionType = qestion.QuestionType,

                                GeneratedQuestionTypeName = GenerateQuestionTypeName(qestion),
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


    public class QuestionTemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }
        public string Validations { set; get; }

        public QuestionType QuestionType { set; get; }

        public string GeneratedQuestionTypeName { set; get; }
        public string GeneratedQuestionMemberName { set; get; }
        public string GeneratedQuestionStateName { set; get; }
    }

    public class GroupTemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }

        public string GeneratedGroupStateName { set; get; }
    }

    public class RosterTemplateModel : IParent
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }
        public string RosterGeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { private set; get; }
        public List<GroupTemplateModel> Groups { private set; get; }

        public IParent ParentRoster { set; get; }

        public IParent GetParent()
        {
            return ParentRoster;
        }

        public string GetTypeName()
        {
            return RosterGeneratedTypeName;
        }

        public IEnumerable<QuestionTemplateModel> GetQuestions()
        {
            return Questions;
        }
    }

    public interface IParent
    {
        IParent GetParent();
        string GetTypeName();
        IEnumerable<QuestionTemplateModel> GetQuestions();
    }
}
