using System;

namespace WB.Services.Export.Interview
{
    public class HeaderColumn
    {
        public string Name { set; get; } = String.Empty;
        public string Title { set; get; } = String.Empty;
        public ExportValueType ExportType { set;  get;  }
    }
}
