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
    public class InvalidQuestionsView  : LinearLayout
    {

        public InvalidQuestionsView(Context context, IEnumerable<StatisticsQuestionViewModel> questions) :
            base(context)
        {
            this.questions = questions;
            Initialize();
        }

        private void Initialize()
        {
            ScrollView sv=new ScrollView(this.Context);
            
            TableLayout tl = new TableLayout(this.Context);
            tl.StretchAllColumns = true;
            TableRow th=new TableRow(this.Context);

            TextView tvQuestionHeader=new TextView(this.Context);
            tvQuestionHeader.Text = "Question";
            StyleCell(tvQuestionHeader);
            th.AddView(tvQuestionHeader);

            TextView tvAnswerHeader = new TextView(this.Context);
            tvAnswerHeader.Text = "Answer";
            StyleCell(tvAnswerHeader);
            th.AddView(tvAnswerHeader);


            TextView tvMessageHeader = new TextView(this.Context);
            tvMessageHeader.Text = "Error message";
            StyleCell(tvMessageHeader);
            th.AddView(tvMessageHeader);

            tl.AddView(th);

            foreach (var statisticsQuestionViewModel in questions)
            {
                TableRow tr = new TableRow(this.Context);

                TextView tvQuestion = new TextView(this.Context);
                tvQuestion.Text = statisticsQuestionViewModel.Text;
                StyleCell(tvQuestion);
                tr.AddView(tvQuestion);

                TextView tvAnswer = new TextView(this.Context);
                tvAnswer.Text = statisticsQuestionViewModel.AnswerString;
                StyleCell(tvAnswer);
                tr.AddView(tvAnswer);

                TextView tvMessage = new TextView(this.Context);
                tvMessage.Text = statisticsQuestionViewModel.ErrorMessage;
                StyleCell(tvMessage);
                tr.AddView(tvMessage);

                tl.AddView(tr);
            }
            sv.AddView(tl);
            this.AddView(sv);
        }
        protected void StyleCell(View cell)
        {
            cell.SetBackgroundResource(Resource.Drawable.cell_shape);
            cell.SetPadding(10,10,10,10);
        }

        protected readonly IEnumerable<StatisticsQuestionViewModel> questions;
    }
}