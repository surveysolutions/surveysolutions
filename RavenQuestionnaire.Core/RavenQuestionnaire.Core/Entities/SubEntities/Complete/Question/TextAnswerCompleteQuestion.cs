using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class TextAnswerCompleteQuestion : IAnswerStrategy
    {
        private CompleteQuestion document;
        public TextAnswerCompleteQuestion(CompleteQuestion document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || currentAnswer.QuestionPublicKey != this.document.PublicKey)
                throw new CompositeException("answer wasn't found");

            this.document.Answers.Clear();
            this.document.Answers.Add(currentAnswer);

        }
    }
}
