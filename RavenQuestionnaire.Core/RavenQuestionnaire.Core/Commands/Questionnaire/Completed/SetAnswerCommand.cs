using System;
using System.Collections.Generic;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "SetAnswer")]
    public class SetAnswerCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public Guid QuestionPublickey { get; set; }
        public Guid? PropogationPublicKey { get; set; }
        public object CompleteAnswer { get; private set; }
        public List<object> CompleteAnswers { get; private set; }

        
        public SetAnswerCommand(Guid completeQuestionnaireId, CompleteQuestionView question, Guid? propogationPublicKey)
        {
            CompleteQuestionnaireId = completeQuestionnaireId;
            PropogationPublicKey = propogationPublicKey;
            this.QuestionPublickey = question.PublicKey;

            if (question.QuestionType == QuestionType.ExtendedDropDownList || question.QuestionType == QuestionType.DropDownList ||
                question.QuestionType == QuestionType.SingleOption || question.QuestionType == QuestionType.YesNo)
            {
                if (question.Answers != null)
                {

                    this.CompleteAnswer = question.Answers[0].PublicKey;

                }
            }
            else if (question.QuestionType == QuestionType.MultyOption)
            {
                var answers = new List<object>();
                for (int i = 0; i < question.Answers.Length; i++)
                {
                    if (question.Answers[i].Selected)
                        answers.Add(question.Answers[i].PublicKey);
                }
                this.CompleteAnswers = answers;
            }
            else
            {
                this.CompleteAnswer = question.Answers[0].AnswerValue;
            }
        }

    }
}