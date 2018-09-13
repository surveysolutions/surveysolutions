using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    [Obsolete("KP-11815")]
    public class DoExportFileHeader
    {
        public DoExportFileHeader(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public DoExportFileHeader(string title, string description, bool addCapture) : this(title, description)
        {
            AddCapture = addCapture;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool AddCapture { get; set; }
    }
}
