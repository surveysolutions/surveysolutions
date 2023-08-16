using System;
using System.Diagnostics;

namespace WB.Core.SharedKernels.Questionnaire.Categories
{
    [DebuggerDisplay("{Id} {Text}")]
    public class CategoriesItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; } = String.Empty;
        public string? AttachmentName { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is CategoriesItem item &&
                   Id == item.Id &&
                   ParentId == item.ParentId &&
                   Text == item.Text &&
                   AttachmentName == item.AttachmentName;
        }

        public override int GetHashCode()
        {
            var hashCode = -1946138586;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + ParentId.GetHashCode();
            return hashCode;
        }
    }
}
