using System;

namespace WB.Core.BoundedContexts.Designer.Services
{
    internal class CategoriesRow
    {
        public string? Id { get; set; }
        public string Text { get; set; } = String.Empty;
        public string? ParentId { get; set; }

        public int RowId { get; set; }
    }
}
