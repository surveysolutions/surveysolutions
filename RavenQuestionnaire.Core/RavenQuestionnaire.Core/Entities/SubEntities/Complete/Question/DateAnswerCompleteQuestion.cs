using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class DateAnswerStrategy : IAnswerStrategy
    {
        private ICompleteQuestion<ICompleteAnswer> document;
        public DateAnswerStrategy(ICompleteQuestion<ICompleteAnswer> document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || currentAnswer.QuestionPublicKey != this.document.PublicKey)
                throw new CompositeException("answer wasn't found");
            
            DateTime value;
            string stringValue = currentAnswer.AnswerValue.ToString();
            var array = currentAnswer.AnswerValue as string[];
            if (array != null && array.Length > 0)
                stringValue = array[0];
            //if (!DateTime.TryParse(stringValue, out value))
            if (!DateTime.TryParseExact(stringValue, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                throw new InvalidCastException("answer is no data value");
            currentAnswer.Selected = true;
            currentAnswer.AnswerType = AnswerType.Text;
            currentAnswer.AnswerValue = value;
            this.document.Answers.Clear();
            this.document.Answers.Add(currentAnswer);
        }
    }
}
