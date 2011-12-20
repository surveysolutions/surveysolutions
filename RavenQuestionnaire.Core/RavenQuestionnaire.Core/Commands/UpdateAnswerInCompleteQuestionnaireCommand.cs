using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateAnswerInCompleteQuestionnaireCommand: ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public CompleteAnswer CompleteAnswer { get; private set; }



        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId, 
                                                  CompleteAnswer answer)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            this.CompleteAnswer = answer;
        }
    }
}
