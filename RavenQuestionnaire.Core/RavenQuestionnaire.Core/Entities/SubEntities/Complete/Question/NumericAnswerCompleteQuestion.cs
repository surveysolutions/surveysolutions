using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class NumericAnswerCompleteQuestion: IAnswerStrategy
    {
        private ICompleteQuestion document;
        public NumericAnswerCompleteQuestion(ICompleteQuestion document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            ICompleteAnswer questionAnswer = this.document.Children[0] as ICompleteAnswer;
            if (questionAnswer == null)
                throw new CompositeException("document is corrapted");
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || !this.document.Children.Any(a => a.PublicKey == currentAnswer.PublicKey))
                  throw new CompositeException("answer wasn't found");
                
            double value;
          /*  string stringValue = currentAnswer.CustomAnswer.ToString();
            var array = currentAnswer.CustomAnswer as string[];
            if (array != null && array.Length > 0)
                stringValue = array[0];*/
            if (currentAnswer.AnswerValue ==null || !double.TryParse(currentAnswer.AnswerValue.ToString(), out value)|| value<0)
              //  value = 0;
              //  throw new InvalidCastException("answer is no numeric value");
                this.document.Valid = false;
            else
                this.document.Valid = true;
            questionAnswer.Selected = true;
           // currentAnswer.AnswerType = AnswerType.Text;
            questionAnswer.AnswerValue = currentAnswer.AnswerValue;
           

        }
        public void Remove()
        {
            ICompleteAnswer questionAnswer = this.document.Children[0] as ICompleteAnswer;
            if (questionAnswer == null)
                throw new CompositeException("document is corrapted");
            questionAnswer.Selected = false;
            questionAnswer.AnswerValue = null;
        }

        public void Create(IEnumerable<IComposite> answers)
        {
            document.Children.Add(new CompleteAnswerFactory().ConvertToCompleteAnswer(new Answer()));
          //  document.OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(result), newanswer));
        }
    }
}
