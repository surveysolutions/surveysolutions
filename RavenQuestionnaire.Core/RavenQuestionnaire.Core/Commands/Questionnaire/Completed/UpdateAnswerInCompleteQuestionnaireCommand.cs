using System;
using System.Linq;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class UpdateAnswerInCompleteQuestionnaireCommand: ICommand
    {
        public string CompleteQuestionnaireId { get; private set; }
        public Guid QuestionPublickey { get; private set; }
        public Guid? Propagationkey { get; private set; }
        public object CompleteAnswers { get; private set; }
		public UserLight Executor { get; set; }
       
        [JsonConstructor]
        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionnaireId,Guid questionPublickey,Guid? propagationkey,
                                                 object completeAnswers, UserLight executor)
           
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            Executor = executor;
            this.CompleteAnswers = completeAnswers;
            this.QuestionPublickey = questionPublickey;
            this.Propagationkey = propagationkey;
        }

        public UpdateAnswerInCompleteQuestionnaireCommand(string completeQuestionanireId,
                                                  CompleteQuestionView question, Guid? propogationPublicKey, UserLight executer)
          
        {
            this.CompleteQuestionnaireId = completeQuestionanireId;
            Executor = executer;
            this.QuestionPublickey = question.PublicKey;
            this.Propagationkey = propogationPublicKey;
            if (question.QuestionType == QuestionType.ExtendedDropDownList || question.QuestionType == QuestionType.DropDownList ||
                question.QuestionType == QuestionType.MultyOption || question.QuestionType == QuestionType.SingleOption)
            {
                if (question.Answers != null)
                {

                    for (int i = 0; i < question.Answers.Length; i++)
                    {
                        this.CompleteAnswers = question.Answers[i].PublicKey;
                    }
                }
            }
            else
            {
                this.CompleteAnswers = question.Answers[0].AnswerValue;
            }

        }
    }
}
