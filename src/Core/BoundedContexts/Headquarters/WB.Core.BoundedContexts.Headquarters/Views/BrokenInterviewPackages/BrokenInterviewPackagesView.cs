using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages
{
    public class BrokenInterviewPackagesView
    {
        public IEnumerable<BrokenInterviewPackageView> Items { get; set; }
        public long TotalCount { get; set; }
    }
}