using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.Statistics;
using CAPI.Android.Events;
namespace CAPI.Android.Controls.Statistics
{
    public class StatisticsTableQuestionsView  : LinearLayout
    {
        private readonly Action<ScreenChangedEventArgs> notifier;
        protected readonly IEnumerable<StatisticsQuestionViewModel> questions;
        protected readonly IEnumerable<string> headerNames;
        protected readonly IEnumerable<Func<StatisticsQuestionViewModel, string>> valueFucntions;
        protected TableLayout tl;
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

            tl = new TableLayout(this.Context);
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

            BuildRows();
            sv.AddView(tl);
            this.AddView(sv);
        }
      
        private void BuildRows()
        {
            foreach (var statisticsQuestionViewModel in questions)
            {
                TableRow tr = new TableRow(this.Context);
                tr.Clickable = true;
                tr.Click += tr_Click;
                tr.SetTag(Resource.Id.ScreenId, statisticsQuestionViewModel.ParentKey.ToString());
                tr.SetBackgroundResource(Resource.Drawable.statistics_row_style);
                foreach (Func<StatisticsQuestionViewModel, string> valueFucntion in valueFucntions)
                {
                    TextView tvQuestion = new TextView(this.Context);
                    var text = valueFucntion(statisticsQuestionViewModel);
                    if (!string.IsNullOrEmpty(text))
                    {
                        tvQuestion.SetText(Html.FromHtml(text),
                                           TextView.BufferType.Spannable);
                    }
                    StyleCell(tvQuestion);
                    tr.AddView(tvQuestion);
                }
                tl.AddView(tr);
            }
        }

        void tr_Click(object sender, EventArgs e)
        {
            var typedSender = sender as TableRow;
            var screenId = ItemPublicKey.Parse(typedSender.GetTag(Resource.Id.ScreenId).ToString());
            notifier(new ScreenChangedEventArgs(screenId));
        }

        protected void StyleCell(View cell)
        {
            cell.SetBackgroundResource(Resource.Drawable.cell_shape);
            cell.SetPadding(10,10,10,10);
            var layout = new TableRow.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.FillParent);
            cell.LayoutParameters = layout;
        }

       
    }
}