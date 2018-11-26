using System.Collections.Generic;
using System.Diagnostics;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class SearchResult
    {
        public int Id { get; set; }
        public string SectionId { get; set; }
        public List<Link> Sections { get; set; } = new List<Link>();
        public List<Link> Questions { get; set; } = new List<Link>();
    }

    [DebuggerDisplay("{Title} : {Target}")]
    public class Link
    {
        public string Target { get; set; }
        public string Title { get; set; }
    }

    public class SearchResults
    {
        public List<SearchResult> Results { get; set; } = new List<SearchResult>();
        public long TotalCount { get; set; }
        public Dictionary<FilterOption, int> Stats { get; set; }
    }
}
