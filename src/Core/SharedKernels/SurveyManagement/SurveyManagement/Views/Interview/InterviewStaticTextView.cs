using System.Collections.Generic;
using System.Diagnostics;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    [DebuggerDisplay("{Text} ({Id})")]
    public class InterviewStaticTextView : InterviewEntityView
    {
        private InterviewStaticTextView()
        {
            this.IsEnabled = true;
            this.IsValid = true;
            this.FailedValidationMessages = new List<ValidationCondition>();
        }

        public InterviewStaticTextView(IStaticText staticText, InterviewAttachmentViewModel attachment) : this()
        {
            this.Id = staticText.PublicKey;
            this.Text = staticText.Text;
            this.Attachment = attachment;
        }

        public string Text { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationCondition> FailedValidationMessages { get; set; }
        public InterviewAttachmentViewModel Attachment { get; set; }
    }
}