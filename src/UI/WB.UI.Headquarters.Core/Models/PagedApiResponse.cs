using System.Collections.Generic;

namespace WB.UI.Headquarters.API
{
    public class PagedApiResponse<T>
    {
        public List<T> Items { get; set; }
        public long Total { get; set; }
        public long Skip { get; set; }
        public long Count { get; set; }
        public bool IsLastPage { get; set; }
    }
}
