using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
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
            new ServiceVariable(ServiceVariableType.InterviewRandom, "_IRnd") //random number generated for interview
        };
        
    }
}
