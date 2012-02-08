using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    public class GroupObservable : IObserver<CompositeInfo>
    {
        public Guid PublicKey { get; set; }
        public Guid TargetPublicKey { get; set; }
        private IDisposable cancellation;

        public virtual void Subscribe(CompositeHandler provider)
        {
            cancellation = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            cancellation.Dispose();
            //   flightInfos.Clear();
        }
        public GroupObservable(Guid group,Guid targetKey)
        {
            this.TargetPublicKey = targetKey;
            this.PublicKey = group;
        }

        #region Implementation of IObserver<in CompositeInfo>

        public virtual void OnNext(CompositeInfo value)
        {
            var target = value.Target as PropagatableCompleteGroup;
            if (target == null)
                return;
            var group = value.Document.Find<CompleteGroup>(this.PublicKey);
            if (group == null)
                return;
            if (target.PublicKey != this.TargetPublicKey)
                return;
            switch (value.Action)
            {
                case Actions.Add:
                    var propagatebleGroup = new PropagatableCompleteGroup(group, target.PropogationPublicKey);
                    value.Document.Add(propagatebleGroup, null);
                    break;
                case Actions.Remove:
                  
                    value.Document.Remove(new PropagatableCompleteGroup(group,
                                                                        target.PropogationPublicKey));

                    break;
            }
        }

        public virtual void OnError(Exception error)
        {

        }

        public virtual void OnCompleted()
        {

        }

        #endregion
    }
}
