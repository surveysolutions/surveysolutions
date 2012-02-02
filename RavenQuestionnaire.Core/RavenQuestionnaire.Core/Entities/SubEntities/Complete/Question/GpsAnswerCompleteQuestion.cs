using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class GpsAnswerCompleteQuestion: IAnswerStrategy
    {
        private CompleteQuestion document;
        public GpsAnswerCompleteQuestion(CompleteQuestion document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || currentAnswer.QuestionPublicKey != this.document.PublicKey)
                throw new CompositeException("answer wasn't found");

            
            /*  string stringValue = currentAnswer.CustomAnswer.ToString();
              var array = currentAnswer.CustomAnswer as string[];
              if (array != null && array.Length > 0)
                  stringValue = array[0];*/
            string[] coordinates = currentAnswer.AnswerValue.ToString().Split(';')  ?? new string[2];
            if (coordinates.Length != 2)
                throw new InvalidCastException("incorrect format");
            foreach (string coordinate in coordinates)
            {
                double value;
                if (!double.TryParse(coordinate, out value))
                    throw new InvalidCastException("incorrect format");
            }
            currentAnswer.Selected = true;
            currentAnswer.AnswerType = AnswerType.Text;
            currentAnswer.AnswerValue = currentAnswer.AnswerValue.ToString();
            this.document.Answers.Clear();
            this.document.Answers.Add(currentAnswer);

        }
    }
}
