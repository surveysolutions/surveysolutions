using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class QuestionnaireEntityExtendedReference
    {
        public QuestionnaireVerificationReferenceType Type { get; set; }
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string ChapterId { get; set; }
        public string Variable { get; set; }
        public string QuestionType { get; set; }
        public int? IndexOfEntityInProperty { get; set; }
    }
}
