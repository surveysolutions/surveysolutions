using Main.Core.Entities.Composite;
using Main.Core.Entities.Observers;

namespace Main.Core.Entities.SubEntities
{
    public interface IGroup : IComposite, ITriggerable, IConditional
    {
        Propagate Propagated { get; set; }

        string Title { get; set; }

        string Description { get; set; }
    }
}