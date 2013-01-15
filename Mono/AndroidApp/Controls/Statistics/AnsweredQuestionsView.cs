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
using AndroidApp.ViewModel.Statistics;

namespace AndroidApp.Controls.Statistics
{
    public class AnsweredQuestionsView : LinearLayout
    {

        public AnsweredQuestionsView(Context context,IEnumerable<StatisticsQuestionViewModel> questions) :
            base(context)
        {
            this.questions = questions;
            Initialize();
        }

        private void Initialize()
        {
            TableLayout tl = new TableLayout(this.Context);
            tl.StretchAllColumns = true;
            TableRow th=new TableRow(this.Context);

            TextView tvQuestionHeader=new TextView(this.Context);
            tvQuestionHeader.Text = "Question";
            th.AddView(tvQuestionHeader);

            TextView tvAnswerHeader = new TextView(this.Context);
            tvAnswerHeader.Text = "Answer";
            th.AddView(tvAnswerHeader);
            
            tl.AddView(th);

            foreach (var statisticsQuestionViewModel in questions)
            {
                TableRow tr = new TableRow(this.Context);

                TextView tvQuestion = new TextView(this.Context);
                tvQuestion.Text = statisticsQuestionViewModel.Text;
                tr.AddView(tvQuestion);

                TextView tvAnswer = new TextView(this.Context);
                tvAnswer.Text = statisticsQuestionViewModel.AnswerString;
                tr.AddView(tvAnswer);

                tl.AddView(tr);
            }
            this.AddView(tl);
        }

        protected readonly IEnumerable<StatisticsQuestionViewModel> questions;
    }
}