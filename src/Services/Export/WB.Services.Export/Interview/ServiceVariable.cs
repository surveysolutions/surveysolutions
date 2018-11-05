namespace WB.Services.Export.Interview
{
    public class ServiceVariable
    {
        public ServiceVariable(ServiceVariableType variableType, string variableExportColumnName, int index)
        {
            this.VariableType = variableType;
            this.VariableExportColumnName = variableExportColumnName;
            this.Index = index;
        }

        public ServiceVariableType VariableType { get; }
        public string VariableExportColumnName { get; }

        public int Index { get; }
    }
}