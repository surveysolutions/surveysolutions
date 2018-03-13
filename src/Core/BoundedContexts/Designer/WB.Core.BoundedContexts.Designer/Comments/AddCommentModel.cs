using System;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public class AddCommentModel
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string Comment { get; set; }
    }
}
