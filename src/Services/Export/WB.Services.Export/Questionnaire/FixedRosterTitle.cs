namespace WB.Services.Export.Questionnaire
{
    public class FixedRosterTitle
    {
        public FixedRosterTitle(decimal titleValue, string title)
        {
            Value = titleValue;
            Title = title;
        }

        public decimal Value { set; get; }
        public string Title { set; get; }
    }
}