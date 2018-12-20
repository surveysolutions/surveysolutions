using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class SearchInput
    {
        public string Query { get; set; }
        public Guid? FolderId { get; set; }
        public string PageIndex { get; set; }
        public string PageSize { get; set; }
        public string OrderBy { get; set; }
    }
}
