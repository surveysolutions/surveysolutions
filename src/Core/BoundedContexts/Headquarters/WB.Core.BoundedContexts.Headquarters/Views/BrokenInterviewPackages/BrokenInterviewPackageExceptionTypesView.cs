using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages
{
    public class BrokenInterviewPackageExceptionTypesView
    {
        public IEnumerable<string> ExceptionTypes { get; set; }
        public long TotalCountByQuery { get; set; }
    }
}