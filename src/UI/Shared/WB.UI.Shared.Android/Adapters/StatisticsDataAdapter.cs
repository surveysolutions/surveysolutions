using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Adapters
{
    public class StatisticsDataAdapter : SmartAdapter<QuestionViewModel>
    {
        protected readonly IList<Func<QuestionViewModel, string>> valueFucntions;
        private readonly Context context;
        private readonly Action<InterviewItemId> notifier;

        public StatisticsDataAdapter(IList<QuestionViewModel> items, IList<Func<QuestionViewModel, string>> valueFucntions, Context context, Action<InterviewItemId> notifier)
            : base(items)
        {
            this.valueFucntions = valueFucntions;
            this.context = context;
            this.notifier = notifier;
        }

        protected override View BuildViewItem(QuestionViewModel dataItem, int position)
        {
            LinearLayout view = new LinearLayout(this.context);
            view.Orientation = Orientation.Horizontal;
            view.LayoutParameters = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent,1);

            view.Clickable = true;
            view.Click += this.tr_Click;
            view.SetTag(Resource.Id.ScreenId, dataItem.PublicKey.ToString());
            view.SetBackgroundResource(Resource.Drawable.statistics_row_style);

            for (int i = 0; i < this.valueFucntions.Count; i++)
            {
                TextView tvQuestion = new TextView(this.context);
                var text = this.valueFucntions[i](dataItem);

                tvQuestion.Text = string.IsNullOrEmpty(text) ? "NA" : text;
                tvQuestion.Gravity = GravityFlags.Left;
                tvQuestion.SetPadding(10, 10, 10, 10);
                view.AddView(tvQuestion, new LinearLayout.LayoutParams(0,
                                                                   ViewGroup.LayoutParams.WrapContent, 1));
            }

            return view;
        }

        void tr_Click(object sender, EventArgs e)
        {
            var typedSender = sender as LinearLayout;
            this.notifier(InterviewItemId.Parse(typedSender.GetTag(Resource.Id.ScreenId).ToString()));
        }
    }
}
