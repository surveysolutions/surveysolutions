using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public class CompleteQuestionFactory : ICompleteQuestionFactory
    {
        /*   private CompleteQuestionnaireDocument document;
           public CompleteQuestionFactory(CompleteQuestionnaireDocument document)
           {
               this.document = document;
           }*/

        #region Implementation of ICompleteQuestionFactory

        public IAnswerStrategy Create(ICompleteQuestion<ICompleteAnswer> baseQuestion)
        {
            switch (baseQuestion.QuestionType)
            {
                case QuestionType.MultyOption:
                    return new MultyAnswerCompleteQuestion(baseQuestion);
                case QuestionType.DropDownList:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.SingleOption:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.YesNo:
                    return new YesNoAnswerCompleteQuestion(baseQuestion);
                case QuestionType.Text:
                    return new TextAnswerCompleteQuestion(baseQuestion);
                case QuestionType.DateTime:
                    return new DateAnswerStrategy(baseQuestion);
                case QuestionType.Numeric:
                    return new NumericAnswerCompleteQuestion(baseQuestion);
                case QuestionType.AutoPropagate:
                    return new NumericAnswerCompleteQuestion(baseQuestion);
                case QuestionType.GpsCoordinates:
                    return new GpsAnswerCompleteQuestion(baseQuestion);
                case QuestionType.Percentage:
                    return new PercentageAnswerCompleteQuestion(baseQuestion);
                case QuestionType.ExtendedDropDownList:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
            }
            return new TextAnswerCompleteQuestion(baseQuestion);
        }

        public object GetAnswerValue(ICompleteQuestion baseQuestion)
        {
            var answer =
                baseQuestion.FirstOrDefault<ICompleteAnswer>(a => a.Selected);
            object retval;
            if (answer == null)
            {
                retval = string.Empty;
            }
            else
            {
                retval = answer.AnswerValue ?? answer.AnswerText;
            }
            switch (baseQuestion.QuestionType)
            {
               /* case QuestionType.MultyOption:
                    return new MultyAnswerCompleteQuestion(baseQuestion);
                case QuestionType.DropDownList:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.SingleOption:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.YesNo:
                    return new YesNoAnswerCompleteQuestion(baseQuestion);
                case QuestionType.Text:
                    return new TextAnswerCompleteQuestion(baseQuestion);*/
          /*      case QuestionType.DateTime:
                    
                    break;*/
                case QuestionType.Numeric:
                    if (retval == null || string.IsNullOrEmpty(retval.ToString()) || !baseQuestion.Valid)
                        retval = 0;
                    break;
               /* case QuestionType.GpsCoordinates:
                    return new GpsAnswerCompleteQuestion(baseQuestion);*/
            }
            return retval;
        }

        public string GetAnswerString(QuestionType type, object answer)
        {
            if (answer == null)
                return null;
            switch (type)
            {
               /* case QuestionType.MultyOption:
                    return new MultyAnswerCompleteQuestion(baseQuestion);
                case QuestionType.DropDownList:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.SingleOption:
                    return new SingleAnswerCompleteQuestion(baseQuestion);
                case QuestionType.YesNo:
                    return new YesNoAnswerCompleteQuestion(baseQuestion);
                case QuestionType.Text:
                    return new TextAnswerCompleteQuestion(baseQuestion);*/
                case QuestionType.DateTime:
                    if (answer is string)
                    {
                        DateTime date;
                        if (DateTime.TryParse((string) answer, out date))
                            return date.ToShortDateString();
                    }
                    if (answer is DateTime)
                        return ((DateTime) answer).ToShortTimeString();
                    break;
                    /*case QuestionType.Numeric:
                    return new NumericAnswerCompleteQuestion(baseQuestion);
                case QuestionType.GpsCoordinates:
                    return new GpsAnswerCompleteQuestion(baseQuestion);
                case QuestionType.Percentage:
                    return new PercentageAnswerCompleteQuestion(baseQuestion);
                case QuestionType.ExtendedDropDownList:
                    return new SingleAnswerCompleteQuestion(baseQuestion);*/
            }
            return answer.ToString();
        }

        public CompleteQuestionView CreateQuestion(CompleteQuestionnaireDocument doc,ICompleteGroup group, ICompleteQuestion question)
        {
        /*    BindedCompleteQuestion bindedQuestion = question as BindedCompleteQuestion;
            if (bindedQuestion != null)
                return new BindedCompleteQuestionView(doc,group, bindedQuestion);*/
            return new CompleteQuestionView(doc, question);
        }

        public ICompleteQuestion ConvertToCompleteQuestion(IQuestion question)
        {
            var simpleQuestion = question as Question;
            if (simpleQuestion != null)
                return (CompleteQuestion) simpleQuestion;
            var bindedQuestion = question as BindedQuestion;
            if (bindedQuestion != null)
                return (BindedCompleteQuestion)bindedQuestion;
            throw new ArgumentException();
        }
        #endregion
    }
}
