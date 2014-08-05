using System;
using Main.Core.Entities.SubEntities;
using Main.Core.View;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class EditStaticTextView : ICompositeView
    {
        public EditStaticTextView(IStaticText doc, Guid? parentId)
        {
            this.Id = doc.PublicKey;
            this.ParentId = parentId;

            string trimmedText = doc.Text.Replace(Environment.NewLine, " ");

            this.Title = string.Format("[Supported in new designer]. {0}...",
                trimmedText.Length > 30 ? trimmedText.Substring(0, 30) : trimmedText);
        }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid? ParentId { get; set; }
    }
}