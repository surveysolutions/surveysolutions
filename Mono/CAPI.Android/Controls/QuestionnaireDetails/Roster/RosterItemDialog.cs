using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails.Roster
{
    public class RosterItemDialog:LinearLayout
    {
        protected AbstractQuestionView questionView;
        protected TextView tv;
        public RosterItemDialog(Context context, QuestionViewModel source, string headerName, Guid questionnairePublicKey, IQuestionViewFactory questionViewFactory)
            : base(context)
        {
            this.Orientation = Orientation.Vertical;
            tv = new TextView(context);
            
            tv.Gravity=GravityFlags.Center;
            tv.TextSize = 22;
            tv.SetPadding(10,10,10,10);
            tv.Text = headerName;
            tv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            this.AddView(tv);
            questionView = questionViewFactory.CreateQuestionView(context, source, questionnairePublicKey);
            this.AddView(questionView);
        }
       
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            questionView.Dispose();
        }
    }
}