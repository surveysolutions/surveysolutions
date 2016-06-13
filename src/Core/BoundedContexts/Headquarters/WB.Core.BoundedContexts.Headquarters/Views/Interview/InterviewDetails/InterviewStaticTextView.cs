using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{Text} ({Id})")]
    public class InterviewStaticTextView : InterviewEntityView
    {
        public InterviewStaticTextView()
        {
            this.IsEnabled = true;
            this.IsValid = true;
            this.FailedValidationMessages = new List<ValidationCondition>();
        }

        public string Text { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationCondition> FailedValidationMessages { get; set; }
        public InterviewAttachmentViewModel Attachment { get; set; }
    }
}