using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class CompleteQuestionFactory
    {
        public IAnswerStrategy Create(CompleteQuestion baseQuestion)
        {
            switch (baseQuestion.QuestionType)
            {
                case QuestionType.MultyOption:
                    return new MultyAnswerCompleteQuestion(baseQuestion);
                case QuestionType.DropDownList:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.SingleOption:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.Text:
                    return new TextAnswerCompleteQuestion(baseQuestion);
                case QuestionType.DateTime:
                    return new DateAnswerStrategy(baseQuestion);
                case QuestionType.Numeric:
                    return new NumericAnswerCompleteQuestion(baseQuestion);
                case QuestionType.GpsCoordinates:
                    return new GpsAnswerCompleteQuestion(baseQuestion);
            }
            return new TextAnswerCompleteQuestion(baseQuestion);
        }
    }
}
