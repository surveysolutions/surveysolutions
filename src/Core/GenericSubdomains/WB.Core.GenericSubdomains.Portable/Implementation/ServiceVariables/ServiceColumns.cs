namespace WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables
{
    public static class ServiceColumns
    {
        //Id of the row
        public const string Id = "Id";
        public const string Key = "interview__key";

        //prefix to identify parent record
        public const string ParentId = "ParentId";

        public const string ResponsibleColumnName = "_responsible";
        public const string AssignmentsCountColumnName = "_quantity";

        //system generated

        public static readonly ServiceVariable[] SystemVariables = new []
        {
            new ServiceVariable(ServiceVariableType.InterviewRandom, "ssSys_IRnd"), //random number generated for interview
            new ServiceVariable(ServiceVariableType.InterviewKey, ServiceColumns.Key), 
        };
    }
}
