using System;

namespace WB.Core.SharedKernels.DataCollection
{
    //could be replaces with bool?

    public enum State
    {
        Unknown = 0,
        Enabled = 1,
        Disabled = 2
    }

    public enum ItemType
    {
        Question = 1,
        Group = 10
    }

    public class ConditionalState
    {
        public ConditionalState(Guid itemId, ItemType type = ItemType.Question, State state = State.Enabled, State previousState = State.Enabled)
        {
            this.Type = type;
            this.ItemId = itemId;
            this.State = state;
            this.PreviousState = previousState;
        }

        public Guid ItemId { get; set; }
        public ItemType Type { get; set; }
        public State State { get; set; }
        public State PreviousState { get; set; }
    }
}
