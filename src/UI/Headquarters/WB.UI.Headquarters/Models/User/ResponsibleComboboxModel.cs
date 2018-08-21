namespace WB.UI.Headquarters.Models.User
{
    public class ResponsibleComboboxModel
    {
        public ResponsibleComboboxModel(ResponsibleComboboxOptionModel[] options, int? total = null)
        {
            this.Options = options ?? new ResponsibleComboboxOptionModel[0];
            this.Total = total ?? options.Length;
        }
        public ResponsibleComboboxOptionModel[] Options { get; private set; }
        public int Total { get; private set; }
    }
}
