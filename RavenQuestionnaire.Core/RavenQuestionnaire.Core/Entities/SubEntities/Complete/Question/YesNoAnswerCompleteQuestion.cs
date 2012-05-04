using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class YesNoAnswerCompleteQuestion : IAnswerStrategy
    {
        private ICompleteQuestion<ICompleteAnswer> document;
        public YesNoAnswerCompleteQuestion(ICompleteQuestion<ICompleteAnswer> document)
        {
            this.document = document;
        }

        public  void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || currentAnswer.QuestionPublicKey != this.document.PublicKey)
                throw new CompositeException("answer wasn't found");

            foreach (CompleteAnswer completeAnswer in this.document.Answers)
            {
                try
                {
                    completeAnswer.Add(c, parent);
                    foreach (var answer in this.document.Answers.Where(answer => answer.PublicKey != currentAnswer.PublicKey))
                    {
                        answer.Selected = false;
                    }
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException("answer wasn't found");
        }
        public void Remove()
        {
            foreach (CompleteAnswer answer in this.document.Answers)
            {
                answer.Remove(answer);
            }
        }
    }
}
