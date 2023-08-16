using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories
{
    public class CategoriesInstance
    {
        public virtual int Id { get; set; }
        public virtual Guid QuestionnaireId { get; set; }

        public virtual Guid CategoriesId { get; set; }

        public virtual int Value { get; set; }
        public virtual int? ParentId { get; set; }
        public virtual string Text { get; set; } = String.Empty;
        public virtual int SortIndex { get; set; }

        public virtual string? AttachmentName { get; set; }
        
        public virtual CategoriesInstance Clone() => new CategoriesInstance
        {
            QuestionnaireId = this.QuestionnaireId,
            CategoriesId = this.CategoriesId,
            Value = this.Value,
            ParentId = this.ParentId,
            Text = this.Text,
            SortIndex = this.SortIndex,
            AttachmentName = this.AttachmentName
        };
    }
}
