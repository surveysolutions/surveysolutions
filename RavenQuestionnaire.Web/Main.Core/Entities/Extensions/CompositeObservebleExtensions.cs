using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public static class CompositeObservebleExtensions
    {
        public static IObservable<CompositeAddedEventArgs> GetAllQuestionAnsweredEvents(this IObservable<CompositeEventArgs> observeble)
        {
            return from q in observeble
                   where 
                         q is CompositeAddedEventArgs &&
                         ((CompositeAddedEventArgs)q).AddedComposite is ICompleteQuestion
                   select q as CompositeAddedEventArgs;
        }
        public static IObservable<CompositeAddedEventArgs> GetGroupPropagatedEvents(this IComposite observeble)
        {
            return from q in observeble
                   where
                       q is CompositeAddedEventArgs &&
                       ((CompositeAddedEventArgs)q).AddedComposite is ICompleteGroup && ((ICompleteGroup)((CompositeAddedEventArgs)q).AddedComposite).PropogationPublicKey.HasValue
                   let propagatedGroup = ((CompositeAddedEventArgs) q).AddedComposite as ICompleteGroup
                   let triggeres =
                       observeble.Find<ICompleteGroup>(
                           g => g.Triggers.Count(gp => gp.Equals(propagatedGroup.PublicKey)) > 0)
                   where triggeres.Any()
                   select q as CompositeAddedEventArgs;
        }


        public static IObservable<CompositeRemovedEventArgs> GetGroupPropagatedRemovedEvents(this IComposite observeble)
        {
            return from q in observeble
                   where q is CompositeRemovedEventArgs &&
                         ((CompositeRemovedEventArgs)q).RemovedComposite is ICompleteGroup && ((ICompleteGroup)((CompositeRemovedEventArgs)q).RemovedComposite).PropogationPublicKey.HasValue
                   let propagatedGroup = ((CompositeRemovedEventArgs)q).RemovedComposite as ICompleteGroup
                   let triggeres =
                       observeble.Find<ICompleteGroup>(
                           g => g.Triggers.Count(gp => gp.Equals(propagatedGroup.PublicKey)) > 0)
                   where triggeres.Any()
                   select q as CompositeRemovedEventArgs;
        }
    }
}
