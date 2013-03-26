using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

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