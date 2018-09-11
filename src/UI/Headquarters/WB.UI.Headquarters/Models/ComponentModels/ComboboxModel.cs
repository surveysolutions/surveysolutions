using System.ComponentModel;

namespace WB.UI.Headquarters.Models.ComponentModels
{
    public class ComboboxModel
    {
        public ComboboxModel(ComboboxOptionModel[] options, int? total = null)
        {
            this.Options = options ?? new ComboboxOptionModel[0];
            this.Total = total ?? Options.Length;
        }
        public ComboboxOptionModel[] Options { get; private set; }
        public int Total { get; private set; }
    }

    public class ComboboxOptionModel
    {
        public ComboboxOptionModel(string key, [Localizable(false)] string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
    
}
