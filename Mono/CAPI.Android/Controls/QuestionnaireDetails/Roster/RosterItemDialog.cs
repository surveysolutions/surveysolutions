using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Controls.QuestionnaireDetails.Roster
{
    public class RosterItemDialog:IDisposable
    {
        protected AbstractQuestionView questionView;
        protected QuestionViewModel model;
        protected AlertDialog dialog;
        protected TextView tv;

        public RosterItemDialog(Context context, QuestionViewModel source, string headerName,
                                Guid questionnairePublicKey, IQuestionViewFactory questionViewFactory)
        {
            this.model = source;
            var setAnswerPopup = new AlertDialog.Builder(context);

            #region build ui

            LinearLayout ll = new LinearLayout(context);
            ll.Orientation = Orientation.Vertical;

            tv = new TextView(context);

            tv.Gravity = GravityFlags.Center;
            tv.TextSize = 22;
            tv.SetPadding(10, 10, 10, 10);
            tv.Text = headerName;
            tv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            ll.AddView(tv);
            questionView = questionViewFactory.CreateQuestionView(context, source, questionnairePublicKey);
            questionView.AnswerSet += questionView_AnswerSet;
            ll.AddView(questionView);
            setAnswerPopup.SetView(ll);

            #endregion

            dialog = setAnswerPopup.Create();
            dialog.DismissEvent += RosterItemDialog_DismissEvent;
            dialog.Show();
        }

        private void questionView_AnswerSet(object sender, EventArgs e)
        {
            if (!questionView.IsCommentsEditorFocused)
                dialog.Dismiss();
        }

        void RosterItemDialog_DismissEvent(object sender, EventArgs e)
        {
         /*   if (answerHandler == null)
                return;
            answerHandler.Dispose();
            answerHandler = null;*/
        }
        public void Dispose()
        {
            questionView.AnswerSet -= questionView_AnswerSet;
            questionView.Dispose();
        }
    }
}