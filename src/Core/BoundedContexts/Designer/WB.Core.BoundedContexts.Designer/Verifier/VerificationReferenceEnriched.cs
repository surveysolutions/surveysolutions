using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class QuestionnaireEntityExtendedReference
    {
        public QuestionnaireEntityExtendedReference(QuestionnaireVerificationReferenceType type, string itemId, 
            string title, string? chapterId = null, string? variable = null, string? questionType = null, int? indexOfEntityInProperty = null,
            string? property = null)
        {
            Type = type;
            ItemId = itemId;
            Title = title;
            ChapterId = chapterId;
            Variable = variable;
            QuestionType = questionType;
            IndexOfEntityInProperty = indexOfEntityInProperty;
            Property = property;
        }

        public QuestionnaireVerificationReferenceType Type { get; set; }
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string? ChapterId { get; set; }
        public string? Variable { get; set; }
        public string? QuestionType { get; set; }
        public string? Property { get; set; }
        public int? IndexOfEntityInProperty { get; set; }
    }
}
