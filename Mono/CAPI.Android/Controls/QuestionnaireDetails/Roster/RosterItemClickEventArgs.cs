using System;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails.Roster
{
    public class RosterItemClickEventArgs:EventArgs
    {
        public RosterItemClickEventArgs(QuestionViewModel model)
        {
            Model = model;
        }

        public QuestionViewModel Model { get; private set; }
    }
}