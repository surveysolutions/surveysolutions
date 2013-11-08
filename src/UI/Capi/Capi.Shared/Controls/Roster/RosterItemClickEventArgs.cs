using System;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Capi.Shared.Controls.Roster
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