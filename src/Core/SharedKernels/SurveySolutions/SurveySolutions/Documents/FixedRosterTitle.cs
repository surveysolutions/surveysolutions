namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class FixedRosterTitle
    {
        public decimal Value { set; get; }
        public string Title { set; get; }

        public FixedRosterTitle(decimal titleValue, string title)
        {
            Value = titleValue;
            Title = title;
        }
    }
}
