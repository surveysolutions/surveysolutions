using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public class CategoriesItem
    {
        public int Value { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; } = String.Empty;
    }
}