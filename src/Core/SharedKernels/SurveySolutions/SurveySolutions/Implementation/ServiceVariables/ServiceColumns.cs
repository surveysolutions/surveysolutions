namespace WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables
{
    public static class ServiceColumns
    {
        //Id of the row
        public const string Id = "Id";

        //prefix to identify parent record
        public const string ParentId = "ParentId";

        //column for assignee
        public const string SupervisorName = "_Supervisor";

        //system generated

        public static readonly ServiceVariable[] SystemVariables = new []
        {
            new ServiceVariable(ServiceVariableType.InterviewRandom, "ssSys_IRnd") //random number generated for interview
        };
        
    }
}
