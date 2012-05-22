using System;
using System.Linq;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class UpdateAnswerInCompleteQuestionnaireCommand: ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public CompleteAnswer[] CompleteAnswers { get; private set; }
     //   public Guid? Group { get; private set; }
		public UserLight Executor { get; set; }

        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId/*, Guid? group*/,
                                                  CompleteAnswerView[] answers, Guid? propogationPublicKey, UserLight executer)
        {
            this.CompleteQuestionnaireId = completeQuestionanireId;
            /*   if (group != Guid.Empty)
                   this.Group = group;*/
            this.CompleteAnswers = answers.Select(answer => new CompleteAnswer()
                                                                {
                                                                    AnswerText = answer.AnswerText,
                                                                    AnswerType = answer.AnswerType,
                                                                    AnswerValue = answer.AnswerValue,
                                                                    Mandatory = answer.Mandatory,
                                                                    PublicKey = answer.PublicKey,
                                                                    Selected =  answer.Selected
                                                                }).ToArray();
            if (propogationPublicKey.HasValue)
            {
                for (int i = 0; i < this.CompleteAnswers.Length; i++)
                {
                    this.CompleteAnswers[i] = new PropagatableCompleteAnswer(this.CompleteAnswers[i],
                                                                             propogationPublicKey.Value);
                }
            }
            Executor = executer;
        }
    }
}
