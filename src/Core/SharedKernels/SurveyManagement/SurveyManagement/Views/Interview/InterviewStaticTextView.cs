using System.Diagnostics;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    [DebuggerDisplay("{Text} ({Id})")]
    public class InterviewStaticTextView : InterviewEntityView
    {
        public InterviewStaticTextView(IStaticText staticText)
        {
            this.Id = staticText.PublicKey;
            this.Text = staticText.Text;
        }

        public string Text { get; set; }
    }
}