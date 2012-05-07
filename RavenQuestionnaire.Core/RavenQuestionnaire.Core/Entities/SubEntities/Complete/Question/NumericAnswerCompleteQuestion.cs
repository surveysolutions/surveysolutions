using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class NumericAnswerCompleteQuestion: IAnswerStrategy
    {
        private ICompleteQuestion<ICompleteAnswer> document;
        public NumericAnswerCompleteQuestion(ICompleteQuestion<ICompleteAnswer> document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || currentAnswer.QuestionPublicKey != this.document.PublicKey)
                throw new CompositeException("answer wasn't found");
            
            double value;
          /*  string stringValue = currentAnswer.CustomAnswer.ToString();
            var array = currentAnswer.CustomAnswer as string[];
            if (array != null && array.Length > 0)
                stringValue = array[0];*/
            if (currentAnswer.AnswerValue ==null || !double.TryParse(currentAnswer.AnswerValue.ToString(), out value))
                value = 0;
              //  throw new InvalidCastException("answer is no numeric value");
            currentAnswer.Selected = true;
           // currentAnswer.AnswerType = AnswerType.Text;
            currentAnswer.AnswerValue = value;
            this.document.Answers.Clear();
            this.document.Answers.Add(currentAnswer);

        }
    }
}
