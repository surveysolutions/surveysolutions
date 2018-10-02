using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class HeaderColumn
    {
        public string Name { set; get; }
        public string Title { set; get; }
        public ExportValueType ExportType { set;  get;  }
    }

    public enum ExportValueType
    {
        Unknown = 0,
        String = 1,
        Numeric = 2,
        NumericInt = 3,
        Date = 4,
        DateTime = 5,
        Boolean = 6
    }
}
