namespace WB.UI.Headquarters.Models.Admin
{
    public class ApplicationSetting
    {
        public ApplicationSetting() { }
        public ApplicationSetting(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}
