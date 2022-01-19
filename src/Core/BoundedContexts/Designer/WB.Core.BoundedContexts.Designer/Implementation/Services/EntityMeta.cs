using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services;

    public class EntityMeta
    {
        public EntityMeta(RosterScope rosterScope)
        {
            RosterScope = rosterScope;
        }

        public RosterScope RosterScope { get; }
    }
