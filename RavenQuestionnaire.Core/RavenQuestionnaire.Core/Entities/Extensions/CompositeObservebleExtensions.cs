using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public static class CompositeObservebleExtensions
    {
        public static IObservable<CompositeAddedEventArgs> GetAllAnswerAddedEvents(this IObservable<CompositeEventArgs> observeble)
        {
            return from q in observeble
                   where q.ParentEvent != null &&
                         q is CompositeAddedEventArgs &&
                         ((CompositeAddedEventArgs) q).AddedComposite is ICompleteAnswer
                   select q as CompositeAddedEventArgs;
        }
        public static IObservable<CompositeAddedEventArgs> GetGroupPropagatedEvents(this IObservable<CompositeEventArgs> observeble)
        {
            return from q in observeble
                   where q is CompositeAddedEventArgs &&
                         ((CompositeAddedEventArgs)q).AddedComposite is PropagatableCompleteGroup
                   select q as CompositeAddedEventArgs;
        }
        public static IObservable<CompositeRemovedEventArgs> GetGroupPropagatedRemovedEvents(this IObservable<CompositeEventArgs> observeble)
        {
            return from q in observeble
                   where q is CompositeRemovedEventArgs &&
                         ((CompositeRemovedEventArgs)q).RemovedComposite is PropagatableCompleteGroup
                   select q as CompositeRemovedEventArgs;
        }
    }
}
