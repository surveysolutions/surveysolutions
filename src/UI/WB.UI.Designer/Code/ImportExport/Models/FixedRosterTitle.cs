namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class FixedRosterTitle
    {
        public decimal Value { set; get; }
        public string? Title { set; get; }

        public FixedRosterTitle(decimal titleValue, string? title)
        {
            Value = titleValue;
            Title = title;
        }
    }
}
