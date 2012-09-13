using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public class PropagationAddedSubscriber : EntitySubscriber<ICompleteGroup>
    {
        #region Overrides of EntitySubscriber<ICompleteGroup>

        protected override IDisposable GetUnsubscriber(ICompleteGroup target)
        
        {
            return target.GetGroupPropagatedEvents().Subscribe(Observer.Create<CompositeAddedEventArgs>((e) =>
               {
                   ICompleteGroup group = e.AddedComposite as ICompleteGroup;
                   if (group == null || !group.PropogationPublicKey.HasValue)
                       return;
                   var triggeres =
                       target.Find<ICompleteGroup>(
                           g => g.Triggers.Count(gp => gp == group.PublicKey) > 0).ToList();
                   foreach (ICompleteGroup triggere in triggeres)
                   {
                       var propagatebleGroup = new CompleteGroup(triggere, group.PropogationPublicKey.Value);
                       target.Add(propagatebleGroup, null);
                   }
               }));
           
            
        }

        #endregion
    }
}
