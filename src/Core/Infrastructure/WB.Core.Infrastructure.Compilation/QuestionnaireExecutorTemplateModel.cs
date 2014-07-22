using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.Infrastructure.Compilation
{
    public class QuestionnaireExecutorTemplateModel
    {
        public Guid Id { private set; get; }

        public List<QuestionTemplateModel> Questions { private set; get; }

        public List<RosterTemplateModel> Rosters { private set; get; }

        public List<GroupTemplateModel> Groups { private set; get; }

        public Dictionary<Guid, Guid[]> ParentsMap { private set; get; }

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
                                QuestionType = qestion.QuestionType
                            }).ToList();

            this.Rosters =
                questionnaireDocument.Find<IGroup>(x => x.IsRoster)
                    .Select(
                        roster =>
                            new RosterTemplateModel()
                            {
                                Id = roster.PublicKey,
                                Conditions = roster.ConditionExpression,
                                VariableName = roster.PublicKey.ToString() //waiting for merge from default
                            })
                    .ToList();
        }
    }


    public class QuestionTemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }
        public string Validations { set; get; }
        public QuestionType QuestionType { set; get; }

    }

    public class GroupTemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }
    }

    public class RosterTemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }

        public List<QuestionTemplateModel> Questions { private set; get; }
    }
}
