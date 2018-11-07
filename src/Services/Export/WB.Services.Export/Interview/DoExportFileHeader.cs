namespace WB.Services.Export.Interview
{
    public class DoExportFileHeader
    {
        public DoExportFileHeader(string title, string description, ExportValueType valueType)
        {
            Title = title;
            Description = description;
            this.ValueType = valueType;
        }

        public DoExportFileHeader(string title, string description, ExportValueType valueType, bool addCaption) 
            : this(title, description, valueType)
        {
            AddCaption = addCaption;
        }

        public string Title { get; }
        public string Description { get; }
        public bool AddCaption { get; }

        public ExportValueType ValueType { get;}
    }
}
