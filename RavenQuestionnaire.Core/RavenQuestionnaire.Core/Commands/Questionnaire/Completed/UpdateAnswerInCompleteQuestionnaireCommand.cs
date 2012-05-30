using System;
using System.Linq;
using Newtonsoft.Json;
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
        protected UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId, UserLight executer)
        {
            this.CompleteQuestionnaireId = completeQuestionanireId;
            Executor = executer;
        }
        [JsonConstructor]
        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionnaireId/*, Guid? group*/,
                                                 CompleteAnswer[] completeAnswers, UserLight executor)
            : this(completeQuestionnaireId, executor)
        {
            this.CompleteAnswers = completeAnswers;

        }

        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId/*, Guid? group*/,
                                                  CompleteAnswerView[] answers, Guid? propogationPublicKey, UserLight executer)
            : this(completeQuestionanireId, executer)
        {
         
            if (answers != null)
            {
                this.CompleteAnswers = answers.Select(answer => new CompleteAnswer()
                                                                    {
                                                                        AnswerText = answer.AnswerText,
                                                                        AnswerType = answer.AnswerType,
                                                                        AnswerValue = answer.AnswerValue,
                                                                        Mandatory = answer.Mandatory,
                                                                        PublicKey = answer.PublicKey,
                                                                        Selected = answer.Selected
                                                                    }).ToArray();
                if (propogationPublicKey.HasValue)
                {
                    for (int i = 0; i < this.CompleteAnswers.Length; i++)
                    {
                        this.CompleteAnswers[i] = new CompleteAnswer(this.CompleteAnswers[i],
                                                                                 propogationPublicKey.Value);
                    }
                }
            }
           
        }
    }
}
