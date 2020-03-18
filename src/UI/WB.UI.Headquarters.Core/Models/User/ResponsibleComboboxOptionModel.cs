namespace WB.UI.Headquarters.Models.User
{
    public class ResponsibleComboboxOptionModel
    {
        public ResponsibleComboboxOptionModel(string key, string value, string iconClass)
        {
            this.Key = key;
            this.Value = value;
            this.IconClass = iconClass;
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public string IconClass { get; set; }
    }
}
