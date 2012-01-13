using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public class PropagatableCompleteGroup : CompleteGroup, IPropogate
    {
        #region Implementation of IPropogate

        public Guid PropogationPublicKey { get; set; }

        public void Propogate(Guid childGroupPublicKey)
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
        }

        public object Clone()
        {
            return new PropagatableCompleteGroup()
                       {
                           GroupText = this.GroupText,
                           Propagated = true,
                           PropogationPublicKey = Guid.NewGuid(),
                           PublicKey = this.PublicKey,
                           Questions = this.Questions,
                           Groups = this.Groups
                       };
        }

        #endregion
        public override bool Add(IComposite c, Guid? parent)
        {
            foreach (CompleteGroup child in Groups)
            {
                if (child.Add(c, parent))
                    return true;
            }
            foreach (CompleteQuestion child in Questions)
            {
                if (child.Add(c, parent))
                    return true;
            }
            return false;
        }
    }
}
