using System.Collections.Generic;

namespace WB.UI.Headquarters.Models.UserPreloading
{
    public class UserPreloadingProcessesView
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<UserPreloadingProcessView> Items { get; set; }
    }
}