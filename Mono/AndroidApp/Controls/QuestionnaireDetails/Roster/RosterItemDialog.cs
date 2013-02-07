using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails.Roster
{
    public class RosterItemDialog:LinearLayout
    {


        public RosterItemDialog(Context context, QuestionViewModel source, string headerName, Guid questionnairePublicKey, IQuestionViewFactory questionViewFactory)
            : base(context)
        {
            this.Orientation = Orientation.Vertical;
            TextView tv = new TextView(context);
            tv.Gravity=GravityFlags.Center;
            tv.TextSize = 22;
            tv.SetPadding(10,10,10,10);
            tv.Text = headerName;
            tv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            this.AddView(tv);
            this.AddView(questionViewFactory.CreateQuestionView(context, source, questionnairePublicKey));
        }

    }
}