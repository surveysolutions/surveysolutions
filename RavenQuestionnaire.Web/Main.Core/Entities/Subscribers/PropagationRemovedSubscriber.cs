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
    public class PropagationRemovedSubscriber : EntitySubscriber<ICompleteGroup>
    {
        #region Overrides of EntitySubscriber<ICompleteGroup>

        protected override IDisposable GetUnsubscriber(ICompleteGroup target)
        {
           return target.GetGroupPropagatedRemovedEvents().Subscribe(Observer.Create<CompositeRemovedEventArgs>((e) =>
           {
               ICompleteGroup group = e.RemovedComposite as ICompleteGroup;
               if (group == null || !group.PropogationPublicKey.HasValue)
                   return;
               var triggeres =
                 target.Find<ICompleteGroup>(
                     g => g.Triggers.Count(gp => gp.Equals(group.PublicKey)) > 0).ToList();
               foreach (ICompleteGroup triggere in triggeres)
               {
                   var propagatebleGroup = new CompleteGroup(triggere, group.PropogationPublicKey.Value);
                   target.Remove(propagatebleGroup);
               }
           }));

        }

        #endregion
    }
}
