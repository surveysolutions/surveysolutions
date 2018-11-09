using WB.Services.Export.Interview;

namespace WB.Services.Export.Services
{
    public class HeaderItemDescription
    {
        public HeaderItemDescription(string label, ExportValueType valueType)
        {
            this.ValueType = valueType;
            this.Label = label;
        }

        public string Label { get; set; }
        public ExportValueType ValueType { set; get; }
    }
}
