using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class RosterTable
    {
        public RosterTable(List<HeaderItem> header, IEnumerable<QuestionnairePropagatedScreenViewModel> rows)
        {
            Header = header;
            Rows = rows;
        }

        public List<HeaderItem> Header { get; private set; }
        public IEnumerable<QuestionnairePropagatedScreenViewModel> Rows { get; private set; }
    }
}