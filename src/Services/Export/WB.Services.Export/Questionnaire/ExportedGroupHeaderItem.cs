using System;
using System.Collections.Generic;
using WB.Services.Export.Interview;

namespace WB.Services.Export.Questionnaire
{
    public class ExportedGroupHeaderItem : IExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public string VariableName { get; set; }
        public List<HeaderColumn> ColumnHeaders { get; set; }
    }
}
