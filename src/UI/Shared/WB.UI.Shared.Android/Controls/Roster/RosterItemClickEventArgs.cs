using System;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Shared.Android.Controls.Roster
{
    public class RosterItemClickEventArgs:EventArgs
    {
        public RosterItemClickEventArgs(QuestionViewModel model)
        {
            this.Model = model;
        }

        public QuestionViewModel Model { get; private set; }
    }
}