using System;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class VisitedGroup
    {
        public VisitedGroup(Guid groupKey, Guid? propagationKey)
        {
            GroupKey = groupKey;
            PropagationKey = propagationKey;
        }

        public Guid GroupKey { get; private set; }
        public Guid? PropagationKey { get; private set; }
    }
}
