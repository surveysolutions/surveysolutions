using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables
{
    public static class ServiceColumns
    {
        //Id of the row
        
        public static readonly string HasAnyError = "errors__count";
        public static readonly string Key = "interview__key";
        public static readonly string InterviewId = "interview__id";
        public static readonly string InterviewStatus = "interview__status";

        public static readonly string IdSuffixFormat = "{0}__id";

        //prefix to identify parent record
        public const string ParentId = "ParentId";

        public const string ResponsibleColumnName = "_responsible";
        public const string AssignmentsCountColumnName = "_quantity";

        //system generated
        public static readonly SortedDictionary<ServiceVariableType, ServiceVariable> SystemVariables = new SortedDictionary<ServiceVariableType, ServiceVariable>
        {
            { ServiceVariableType.InterviewRandom,  new ServiceVariable(ServiceVariableType.InterviewRandom, "ssSys_IRnd", 0)},
            { ServiceVariableType.InterviewKey,  new ServiceVariable(ServiceVariableType.InterviewKey, ServiceColumns.Key, 1)},
            { ServiceVariableType.HasAnyError,  new ServiceVariable(ServiceVariableType.HasAnyError, ServiceColumns.HasAnyError, 2)},
            { ServiceVariableType.InterviewStatus,  new ServiceVariable(ServiceVariableType.InterviewStatus, ServiceColumns.InterviewStatus, 3)},
        };
    }
}
