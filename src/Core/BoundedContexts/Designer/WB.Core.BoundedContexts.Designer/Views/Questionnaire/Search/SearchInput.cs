using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class SearchInput
    {
        public string? Query { get; set; }
        public Guid? FolderId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; } = 20;
    }
}
