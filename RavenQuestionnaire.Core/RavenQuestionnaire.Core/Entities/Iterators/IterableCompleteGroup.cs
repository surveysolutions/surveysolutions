using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class IterableCompleteGroup
    {
        public Guid PublicKey { get; set; }
        public Propagate Propagated { get; set; }
        public List<ICompleteGroup> PropagatedGroups { get; set; }
        public ICompleteGroup PropagatableGroup { get; set; }
        public ICompleteGroup SimpleGroup { get; set; }

        public IterableCompleteGroup(Guid key, ICompleteGroup completeGroup)
        {
            PublicKey = key;
            SimpleGroup = completeGroup;
            Propagated = Propagate.None;
        }

        public IterableCompleteGroup(Guid key, List<ICompleteGroup> groups)
        {
            PropagatableGroup = groups.Single(g => (g as PropagatableCompleteGroup) == null);
            PropagatedGroups = groups.Where(g => g != PropagatableGroup).Select(g => g).ToList();
            PublicKey = key;
            Propagated = Propagate.Propagated;
        }
    }
}
