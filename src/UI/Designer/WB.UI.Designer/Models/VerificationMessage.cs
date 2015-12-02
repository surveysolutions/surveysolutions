using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.UI.Designer.Models
{
    public class VerificationMessage
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool IsGroupOfErrors { get; set; }
        public List<VerificationReference> References { get; set; }
    }

    public class VerificationReference
    {
        public QuestionnaireVerificationReferenceType Type { get; set; }
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string ChapterId { get; set; }
        public string Variable { get; set; }
        public string QuestionType { get; set; }
    }
}