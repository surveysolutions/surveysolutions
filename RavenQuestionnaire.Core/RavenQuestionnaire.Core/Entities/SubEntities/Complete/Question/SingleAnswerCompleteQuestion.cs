using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class SingleAnswerCompleteQuestion : IAnswerStrategy
    {
        private ICompleteQuestion document;
        public SingleAnswerCompleteQuestion(ICompleteQuestion document)
        {
            this.document = document;
        }

        public  void Add(IComposite c, Guid? parent)
        {
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || !this.document.Children.Any(a => a.PublicKey == currentAnswer.PublicKey))
                throw new CompositeException("answer wasn't found");

            foreach (CompleteAnswer completeAnswer in this.document.Children)
            {
                try
                {
                    completeAnswer.Add(c, parent);
                    foreach (CompleteAnswer answer in this.document.Children.Where(answer => answer.PublicKey != currentAnswer.PublicKey))
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
            foreach (CompleteAnswer answer in this.document.Children)
            {
                answer.Remove(answer);
            }
        }
        public void Create(IEnumerable<IComposite> answers)
        {
            foreach (IComposite composite in answers)
            {
                document.Children.Add(new CompleteAnswerFactory().ConvertToCompleteAnswer(composite as IAnswer));
            }
          
            //  document.OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(result), newanswer));
        }
    }
}
