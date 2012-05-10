using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public class AnswerSubscriber : EntitySubscriber<ICompleteGroup>
    {
        #region Overrides of EntitySubscriber<ICompleteQuestion>

        public override void Subscribe(ICompleteGroup target)
        {
            var questions = target.GetAllQuestions<ICompleteQuestion>();
            foreach (ICompleteQuestion completeQuestion in questions)
            {
                completeQuestion.GetAllAnswerAddedEvents().Subscribe(
                    Observer.Create<CompositeAddedEventArgs>(
                        (e) =>
                            {
                                ((ICompleteAnswer) e.AddedComposite).QuestionPublicKey = target.PublicKey;
                            }));
            }
          
        }

        #endregion
    }
}
