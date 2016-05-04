namespace WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables
{
    public class ServiceVariable
    {
        public ServiceVariable(ServiceVariableType variableType, string variableExportColumnName)
        {
            this.VariableType = variableType;
            this.VariableExportColumnName = variableExportColumnName;
        }

        public ServiceVariableType VariableType { private set; get; }
        public string VariableExportColumnName { private set; get; }
    }
}
