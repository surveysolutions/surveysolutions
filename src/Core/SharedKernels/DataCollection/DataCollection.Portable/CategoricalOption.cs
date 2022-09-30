namespace WB.Core.SharedKernels.DataCollection
{
    public class CategoricalOption
    {
        public int Value { get; set; }
        public int? ParentValue { get; set; }
        public string Title { get; set; }
        public string AttachmentName { get; set; }

        public override bool Equals(object obj) => obj is CategoricalOption option && Value == option.Value;

        public override int GetHashCode() => -1937169414 + Value.GetHashCode();
    }
}
