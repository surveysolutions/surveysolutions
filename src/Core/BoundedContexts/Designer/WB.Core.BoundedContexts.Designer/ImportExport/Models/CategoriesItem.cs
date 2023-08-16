using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public class CategoriesItem
    {
        public int Value { get; set; }
        public int? ParentValue { get; set; }
        public string Text { get; set; } = String.Empty;
        public virtual string? AttachmentName { get; set; }
    }
}
