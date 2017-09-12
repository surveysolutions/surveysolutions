using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class ExportedVariableHeaderItem : IExportedHeaderItem
    {
        public Guid PublicKey { get; set; }
        public VariableType VariableType { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] Titles { get; set; }
        public string VariableName { get; set; }
    }
}