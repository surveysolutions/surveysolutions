namespace WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables
{
    public static class ServiceColumns
    {
        //Id of the row
        public const string Id = "Id";

        //prefix to identify parent record
        public const string ParentId = "ParentId";

        public const string ResponsibleColumnName = "responsible";

        //system generated

        public static readonly ServiceVariable[] SystemVariables = new []
        {
            new ServiceVariable(ServiceVariableType.InterviewRandom, "ssSys_IRnd") //random number generated for interview
        };
        
    }
}
