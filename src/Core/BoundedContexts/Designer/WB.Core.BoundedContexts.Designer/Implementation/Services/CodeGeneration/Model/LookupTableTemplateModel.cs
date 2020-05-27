using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class LookupTableTemplateModel
    {
        public LookupTableTemplateModel(string typeName, string tableName, string tableNameField, string[] variableNames, LookupTableRow[] rows)
        {
            TypeName = typeName;
            TableName = tableName;
            TableNameField = tableNameField;
            VariableNames = variableNames;
            Rows = rows;
        }

        public string TypeName { set; get; }
        public string TableName { set; get; }
        public string TableNameField { set; get; }
        public string[] VariableNames { get; set; }
        public LookupTableRow[] Rows { get; set; }
    }
}
