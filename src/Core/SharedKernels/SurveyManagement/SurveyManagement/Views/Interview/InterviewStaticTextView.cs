using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public InterviewStaticTextView(IStaticText staticText, string titleWithSubstitutedVariables, InterviewStaticText interviewStaticText, InterviewAttachmentViewModel attachment) : this()
        {
            this.Id = staticText.PublicKey;
            this.Text = titleWithSubstitutedVariables;
            this.Attachment = attachment;

            if (interviewStaticText != null)
            {
                this.IsEnabled = interviewStaticText.IsEnabled;
                this.IsValid = !interviewStaticText.IsInvalid;
                this.FailedValidationMessages = interviewStaticText.FailedValidationConditions.Select(x => staticText.ValidationConditions[x.FailedConditionIndex]).ToList();
            }
        }

        public string Text { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationCondition> FailedValidationMessages { get; set; }
        public InterviewAttachmentViewModel Attachment { get; set; }
    }
}