using System;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class MoveQuestionnaireItemCommand: ICommand
    {
        
        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; set; }
        public Guid? GroupKey { get; set; }
        public Guid? AfterItemKey { get; set; }
        public UserLight Executor { get; set; }

        public MoveQuestionnaireItemCommand(string questionnaireId, Guid publicKey, Guid? group, Guid? afterItem, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.PublicKey = publicKey;
            this.AfterItemKey = afterItem;
            this.GroupKey = group;
            Executor = executor;
        }
    }
}
