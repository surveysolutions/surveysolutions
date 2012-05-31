using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteGroup : CompleteGroup, IPropogate
    {
        public PropagatableCompleteGroup()
        {
        }

        public PropagatableCompleteGroup(ICompleteGroup group, Guid propogationPublicKey):this()
        {
            this.Title = group.Title;
            this.Propagated = group.Propagated;
            this.AutoPropagate = group.Propagated == Propagate.AutoPropagated;
            this.PublicKey = group.PublicKey;

            for (int i = 0; i < group.Children.Count; i++)
            {
                var question = group.Children[i] as ICompleteQuestion;
                if (question != null)
                {
                    if (!(question is IBinded))
                        this.Children.Add(question);
                    else
                        this.Children.Add((BindedCompleteQuestion)question);
                    continue;
                    
                }
                var groupChild = group.Children[i] as ICompleteGroup;
                if(groupChild!=null)
                {
                    this.Children.Add(new PropagatableCompleteGroup(groupChild, propogationPublicKey));
                    continue;
                }
                throw new InvalidOperationException("uncnown children type");
            }

            /* for (int i = 0; i < groupWithQuestion.Groups.Count; i++)
                {
                    this.Groups.Add(new PropagatableCompleteGroup(groupWithQuestion.Groups[i], propogationPublicKey));
                   
                }*/
          
            this.PropogationPublicKey = propogationPublicKey;
        }

        #region Implementation of IPropogate

        public Guid PropogationPublicKey { get; set; }

        public bool AutoPropagate { get; set; }


        /*    public void Propogate(Guid childGroupPublicKey)
        {
            var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(childGroupPublicKey));
            if (group == null)
                throw new ArgumentException("Propogated group can't be founded");
            if (group as IPropogate != null)
                throw new InvalidOperationException("Group can't be propogated");
            Groups.Add(((IPropogate) group).Clone() as CompleteGroup);
        }

        public void RemovePropogated(Guid childGroupPublicKey)
        {
            var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(childGroupPublicKey));
            if (group == null)
                throw new ArgumentException("Removed group can't be founded");
            if (group as IPropogate != null)
                throw new InvalidOperationException("Group can't be removed. The reason is group wasn't propogated");
            Groups.Remove(group);
        }*/

        public object Clone()
        {
            var result = new PropagatableCompleteGroup()
                       {
                           Title = this.Title,
                           Propagated = this.Propagated,
                           AutoPropagate = this.Propagated == Propagate.AutoPropagated,
                           PropogationPublicKey = Guid.NewGuid(),
                           PublicKey = this.PublicKey
                       };
           /* for (int i = 0; i < this.Questions.Count; i++)
            {
                result.Questions.Add(new PropagatableCompleteQuestion(this.Questions[i], result.PublicKey));
            }
            for (int i = 0; i < result.Groups.Count; i++)
            {
                result.Groups[i] = new PropagatableCompleteGroup(this.Groups[i], result.PublicKey);
            }*/
            throw new NotImplementedException();
            return result;
        }

        #endregion
        public override void Add(IComposite c, Guid? parent)
        {
            IPropogate propagate = c as IPropogate;
            if (propagate == null)
                throw new CompositeException();
            if (propagate.PropogationPublicKey != this.PropogationPublicKey)
                throw new CompositeException();
            base.Add(c, parent);
        }
        public override void Remove(IComposite c)
        {
            IPropogate propagate = c as IPropogate;
            if (propagate == null)
                throw new CompositeException();
            if (propagate.PropogationPublicKey != this.PropogationPublicKey)
                throw new CompositeException();
            base.Remove(c);
        }

        public override void Remove(Guid publicKey){
            //  IPropogate propagate = c as IPropogate;
          /*  if (!typeof (T).IsAssignableFrom(typeof (IPropogate)))
                return false;
            var propagate = Find<T>(publicKey);
            if (propagate == null)
                return false;
            return base.Remove(propagate);*/
            throw new InvalidOperationException("remove operation can't be executed at propogated group.");
            //   return false;
        }

        public override T Find<T>(Guid publicKey)
        {
            /*  if (!typeof (IPropogate).IsAssignableFrom(typeof (T)))
                  return null;*/

            if (this.PublicKey.Equals(publicKey))
                return this as T;

            var resultInsideGroups =
                Children.Select(group => group.Find<T>(publicKey)).
                    FirstOrDefault(result => result != null);

            return resultInsideGroups;

        }
    }
}
