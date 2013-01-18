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
using AndroidApp.Events;
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.Statistics;

namespace AndroidApp.Controls.Statistics
{
    public class StatisticsTableQuestionsView  : LinearLayout
    {
        private readonly Action<ScreenChangedEventArgs> notifier;
        protected readonly IEnumerable<StatisticsQuestionViewModel> questions;
        protected readonly IEnumerable<string> headerNames;
        protected readonly IEnumerable<Func<StatisticsQuestionViewModel, string>> valueFucntions;
        public StatisticsTableQuestionsView(
            Context context, 
            IEnumerable<StatisticsQuestionViewModel> questions, 
            Action<ScreenChangedEventArgs> notifier,
            IEnumerable<string> headerNames,
            IEnumerable<Func<StatisticsQuestionViewModel, string>> valueFucntions) :
            base(context)
        {
            if (valueFucntions.Count() != headerNames.Count())
                throw new ArgumentException();
            this.questions = questions;
            this.notifier = notifier;
            this.headerNames = headerNames;
            this.valueFucntions = valueFucntions;
            Initialize();
        }

        private void Initialize()
        {
            ScrollView sv = new ScrollView(this.Context);

            TableLayout tl = new TableLayout(this.Context);
            tl.StretchAllColumns = true;
            TableRow th = new TableRow(this.Context);
            foreach (string headerName in headerNames)
            {
                TextView tvQuestionHeader = new TextView(this.Context);
                tvQuestionHeader.Text = headerName;
                StyleCell(tvQuestionHeader);
                th.AddView(tvQuestionHeader);
            }

            tl.AddView(th);

            foreach (var statisticsQuestionViewModel in questions)
            {
                TableRow tr = new TableRow(this.Context);
                tr.Clickable = true;
                tr.Click += tr_Click;
                tr.SetBackgroundResource(Resource.Drawable.statistics_row_style);
                foreach (Func<StatisticsQuestionViewModel, string> valueFucntion in valueFucntions)
                {
                    TextView tvQuestion = new TextView(this.Context);
                    tvQuestion.Text = valueFucntion(statisticsQuestionViewModel);
                    StyleCell(tvQuestion);
                    tr.AddView(tvQuestion);
                }
                tl.AddView(tr);
            }
            sv.AddView(tl);
            this.AddView(sv);
        }

        void tr_Click(object sender, EventArgs e)
        {
            notifier(
                new ScreenChangedEventArgs(new ItemPublicKey(Guid.Parse("e2aaeffe-4b4c-4db1-a773-83bebc394543"), null)));
        }
        protected void StyleCell(View cell)
        {
            cell.SetBackgroundResource(Resource.Drawable.cell_shape);
            cell.SetPadding(10,10,10,10);
        }

       
    }
}