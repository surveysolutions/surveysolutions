using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class ExportedVariableHeaderItem : IExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public VariableType VariableType { get; set; }
        public string VariableName { get; set; }
        public List<HeaderColumn> ColumnHeaders { get; set; }
    }
}