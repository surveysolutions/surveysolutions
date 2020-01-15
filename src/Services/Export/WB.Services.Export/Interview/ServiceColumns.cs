using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.Interview
{
    public static class ServiceColumns
    {
        public const string ColumnDelimiter = "__";
        //Id of the row
        public static readonly string InterviewRandom = "sssys_irnd";
        public static readonly string HasAnyError = $"has{ColumnDelimiter}errors";
        public static readonly string Key = $"interview{ColumnDelimiter}key";
        public static readonly string InterviewId = $"interview{ColumnDelimiter}id";
        public static readonly string InterviewStatus = $"interview{ColumnDelimiter}status";
        public static readonly string ProtectedVariableNameColumn = $"variable{ColumnDelimiter}name";
        public static readonly string AssignmentId = $"assignment{ColumnDelimiter}id";

        public static readonly string IdSuffixFormat = $"{{0}}{ColumnDelimiter}id";

        //prefix to identify parent record
        public const string ParentId = "parentId";
        
        //system generated
        public static readonly SortedDictionary<ServiceVariableType, ServiceVariable> SystemVariables = new SortedDictionary<ServiceVariableType, ServiceVariable>
        {
            { ServiceVariableType.InterviewRandom,  new ServiceVariable(ServiceVariableType.InterviewRandom, InterviewRandom, 0)},
            { ServiceVariableType.HasAnyError,  new ServiceVariable(ServiceVariableType.HasAnyError, HasAnyError, 1)},
            { ServiceVariableType.InterviewStatus,  new ServiceVariable(ServiceVariableType.InterviewStatus, InterviewStatus, 2)},
            { ServiceVariableType.AssignmentId,  new ServiceVariable(ServiceVariableType.AssignmentId, AssignmentId, 3)},
        };

        public static readonly ServiceVariable InterviewKey = new ServiceVariable(ServiceVariableType.InterviewKey, Key, 0);
        
    }
}
