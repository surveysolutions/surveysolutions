using Main.Core.Entities.Observers;

namespace Main.Core.Entities.SubEntities.Question
{
    public interface IAutoPropagate : ITriggerable
    {
        int MaxValue { get; set; }
    }
}