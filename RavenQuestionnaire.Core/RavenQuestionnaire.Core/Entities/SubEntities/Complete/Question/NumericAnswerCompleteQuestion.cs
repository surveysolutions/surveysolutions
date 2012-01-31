using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    class NumericAnswerCompleteQuestion: IAnswerStrategy
    {
        private CompleteQuestion document;
        public NumericAnswerCompleteQuestion(CompleteQuestion document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || currentAnswer.QuestionPublicKey != this.document.PublicKey)
                throw new CompositeException("answer wasn't found");
            
            double value;
            if (!double.TryParse( currentAnswer.CustomAnswer, out value))
                throw new InvalidCastException("answer is no numeric value");
            currentAnswer.Selected = true;
            currentAnswer.AnswerType = AnswerType.Text;
            this.document.Answers.Clear();
            this.document.Answers.Add(currentAnswer);

        }
    }
}
