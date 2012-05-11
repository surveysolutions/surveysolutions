using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class DateAnswerStrategy : IAnswerStrategy
    {
        private ICompleteQuestion document;
        public DateAnswerStrategy(ICompleteQuestion document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            ICompleteAnswer questionAnswer = this.document.Children[0] as ICompleteAnswer;
            if(questionAnswer==null)
                throw new CompositeException("document is corrapted");
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || !this.document.Children.Any(a => a.PublicKey == currentAnswer.PublicKey))
                throw new CompositeException("answer wasn't found");
            
            DateTime value;
            string stringValue = currentAnswer.AnswerValue.ToString();
            var array = currentAnswer.AnswerValue as string[];
            if (array != null && array.Length > 0)
                stringValue = array[0];
            //if (!DateTime.TryParse(stringValue, out value))
            var culture = CultureInfo.InvariantCulture;
            if (!DateTime.TryParse(stringValue, culture.DateTimeFormat, DateTimeStyles.None, out value))
                this.document.Valid = false;
            else
                this.document.Valid = true;
               // throw new InvalidCastException("answer is no data value");
            questionAnswer.Selected = true;
         //  currentAnswer.AnswerType = AnswerType.Text;
            questionAnswer.AnswerValue = stringValue;
          /*  this.document.Children[0];
            this.document.Children.Add(currentAnswer);*/
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
