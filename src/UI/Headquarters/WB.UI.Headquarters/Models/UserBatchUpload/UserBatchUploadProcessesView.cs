using System.Collections.Generic;

namespace WB.UI.Headquarters.Models.UserBatchUpload
{
    public class UserBatchUploadProcessesView
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<UserBatchUploadProcessView> Items { get; set; }
    }
}