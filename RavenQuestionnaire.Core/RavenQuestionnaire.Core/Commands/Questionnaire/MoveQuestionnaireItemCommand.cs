using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    public class MoveQuestionnaireItemCommand: ICommand
    {
        
        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; set; }
        public Guid? AfterItemKey { get; set; }
        public UserLight Executor { get; set; }

        public MoveQuestionnaireItemCommand(string questionnaireId, Guid publicKey, Guid? afterItem, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.PublicKey = publicKey;
            this.AfterItemKey = afterItem;
            Executor = executor;
        }
    }
}
