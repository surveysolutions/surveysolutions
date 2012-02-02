using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateAnswerInCompleteQuestionnaireCommand: ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public CompleteAnswer CompleteAnswer { get; private set; }
     //   public Guid? Group { get; private set; }
		public UserLight Executor { get; set; }

        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId/*, Guid? group*/,
                                                  CompleteAnswerView answer, Guid? propogationPublicKey, UserLight executer)
        {
            this.CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(completeQuestionanireId);
            /*   if (group != Guid.Empty)
                   this.Group = group;*/
            this.CompleteAnswer = new CompleteAnswer()
                                      {
                                          AnswerText = answer.AnswerText,
                                          AnswerType = answer.AnswerType,
                                          AnswerValue = answer.AnswerValue,
                                          Mandatory = answer.Mandatory,
                                          PublicKey = answer.PublicKey,
                                          QuestionPublicKey = answer.QuestionId,
                                          Selected = answer.Selected
                                      };
            if (propogationPublicKey.HasValue)
            {
                this.CompleteAnswer = new PropagatableCompleteAnswer(this.CompleteAnswer, propogationPublicKey.Value);
            }
            Executor = executer;
        }
    }
}
