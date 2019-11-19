using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories
{
    public class CategoriesInstance
    {
        public virtual Guid QuestionnaireId { get; set; }

        public virtual Guid CategoriesId { get; set; }

        public virtual int Id { get; set; }
        public virtual int? ParentId { get; set; }
        public virtual string Text { get; set; }

        public virtual CategoriesInstance Clone() => new CategoriesInstance
        {
            QuestionnaireId = this.QuestionnaireId,
            CategoriesId = this.CategoriesId,
            Id = this.Id,
            ParentId = this.ParentId,
            Text = this.Text
        };
    }
}
