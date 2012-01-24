using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteGroup : CompleteGroup, IPropogate
    {
        public PropagatableCompleteGroup()
        {
        }

        public PropagatableCompleteGroup(CompleteGroup group, Guid propogationPublicKey)
        {
            this.GroupText = group.GroupText;
            this.Propagated = group.Propagated;
            this.PublicKey = group.PublicKey;

            for (int i = 0; i < group.Questions.Count; i++)
            {
                this.Questions.Add(new PropagatableCompleteQuestion(group.Questions[i], propogationPublicKey));
            }
            for (int i = 0; i < group.Groups.Count; i++)
            {
                this.Groups.Add(new PropagatableCompleteGroup(group.Groups[i], propogationPublicKey));
            //    this.Groups[i] = new PropagatableCompleteGroup(group.Groups[i], propogationPublicKey);
            }

            this.PropogationPublicKey = propogationPublicKey;
        }

        #region Implementation of IPropogate

        public Guid PropogationPublicKey { get; set; }

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
                           GroupText = this.GroupText,
                           Propagated = true,
                           PropogationPublicKey = Guid.NewGuid(),
                           PublicKey = this.PublicKey
                       };
            for (int i = 0; i < this.Questions.Count; i++)
            {
                result.Questions.Add(new PropagatableCompleteQuestion(this.Questions[i], result.PublicKey));
            }
            for (int i = 0; i < result.Groups.Count; i++)
            {
                result.Groups[i] = new PropagatableCompleteGroup(this.Groups[i], result.PublicKey);
            }
            return result;
        }

        #endregion
        public override bool Add(IComposite c, Guid? parent)
        {
            IPropogate propagate = c as IPropogate;
            if (propagate == null)
                return false;
            if (propagate.PropogationPublicKey != this.PropogationPublicKey)
                return false;
            return base.Add(c, parent);
        }
        public override bool Remove(IComposite c)
        {
            IPropogate propagate = c as IPropogate;
            if (propagate == null)
                return false;
            if (propagate.PropogationPublicKey != this.PropogationPublicKey)
                return false;
            return base.Remove(c);
        }

        public override bool Remove<T>(Guid publicKey){
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
            if (!typeof (T).IsAssignableFrom(typeof (IPropogate)))
                return null;
            if (typeof (T) == GetType())
            {
                if (this.PublicKey.Equals(publicKey))
                    return this as T;
            }
            var resultInsideGroups =
                Groups.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideGroups != null)
                return resultInsideGroups;
            var resultInsideQuestions =
                Questions.Select(answer => answer.Find<T>(publicKey)).FirstOrDefault(result => result != null);
            if (resultInsideQuestions != null)
                return resultInsideQuestions;
            return null;
        }
    }
}
