using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.BrokenInterviewPackages
{
    public class BrokenInterviewPackagesView
    {
        public IEnumerable<BrokenInterviewPackageView> Items { get; set; }
        public long TotalCount { get; set; }
    }
}