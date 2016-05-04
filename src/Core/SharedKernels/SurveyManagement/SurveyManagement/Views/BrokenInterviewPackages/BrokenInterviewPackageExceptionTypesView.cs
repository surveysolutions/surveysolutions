using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.BrokenInterviewPackages
{
    public class BrokenInterviewPackageExceptionTypesView
    {
        public IEnumerable<string> ExceptionTypes { get; set; }
        public long TotalCountByQuery { get; set; }
    }
}