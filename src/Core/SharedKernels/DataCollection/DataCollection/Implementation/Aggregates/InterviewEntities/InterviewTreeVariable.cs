namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeVariable : InterviewTreeLeafNode
    {
        public object Value { get; private set; }
        public bool HasValue => this.Value != null;

        public InterviewTreeVariable(Identity identity) : this(identity, false, null)
        {
        }

        public InterviewTreeVariable(Identity identity, bool isDisabled, object value)
            : base(identity, isDisabled)
        {
            this.SetValue(value);
        }


        public override string ToString() => $"Variable ({this.Identity})";

        public void SetValue(object value) => this.Value = value;
    }
}