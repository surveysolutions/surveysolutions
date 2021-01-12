namespace WB.UI.Headquarters.Models.ComponentModels
{
    public class ComboboxModel : ComboboxModel<ComboboxOptionModel>
    {
        public ComboboxModel(ComboboxOptionModel[] options, int? total = null) 
            : base(options, total)
        {
        }
    }

    public class ComboboxModel<T>
    {
        public ComboboxModel(T[] options, int? total = null)
        {
            this.Options = options ?? new T[0];
            this.Total = total ?? Options.Length;
        }
        public T[] Options { get; private set; }
        public int Total { get; private set; }
    }

    public class ComboboxOptionModel
    {
        public ComboboxOptionModel(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
    
}
