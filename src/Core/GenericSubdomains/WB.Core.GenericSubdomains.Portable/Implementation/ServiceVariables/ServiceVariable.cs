namespace WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables
{
    public class ServiceVariable
    {
        public ServiceVariable(ServiceVariableType variableType, string variableExportColumnName, int index)
        {
            this.VariableType = variableType;
            this.VariableExportColumnName = variableExportColumnName;
            this.Index = index;
        }

        public ServiceVariableType VariableType { private set; get; }
        public string VariableExportColumnName { private set; get; }

        public int Index { get; }
    }
}
