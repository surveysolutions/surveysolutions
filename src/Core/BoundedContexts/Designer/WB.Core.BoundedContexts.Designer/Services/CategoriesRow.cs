using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class CategoriesRow
    {
        public string? Id { get; set; }
        public string Text { get; set; } = String.Empty;
        public string? ParentId { get; set; }

        public string? AttachmentName { set; get; }

        public int RowId { get; set; }
    }
}
