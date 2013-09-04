using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.Statistics;
using CAPI.Android.Events;
using Orientation = Android.Widget.Orientation;

namespace CAPI.Android.Controls.Statistics
{
    public class StatisticsDataAdapter : SmartAdapter<StatisticsQuestionViewModel>
    {
        protected readonly IList<Func<StatisticsQuestionViewModel, string>> valueFucntions;
        private readonly Context context;
        private readonly Action<ScreenChangedEventArgs> notifier;

        public StatisticsDataAdapter(IList<StatisticsQuestionViewModel> items, IList<Func<StatisticsQuestionViewModel, string>> valueFucntions, Context context, Action<ScreenChangedEventArgs> notifier) : base(items)
        {
            this.valueFucntions = valueFucntions;
            this.context = context;
            this.notifier = notifier;
        }

        protected override View BuildViewItem(StatisticsQuestionViewModel dataItem, int position)
        {
            LinearLayout view = new LinearLayout(context);
            view.Orientation = Orientation.Horizontal;
            view.LayoutParameters = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent, 1);

            view.Clickable = true;
            view.Click += tr_Click;
            view.SetTag(Resource.Id.ScreenId, dataItem.ParentKey.ToString());
            view.SetBackgroundResource(Resource.Drawable.statistics_row_style);

            for (int i = 0; i < valueFucntions.Count; i++)
            {
                TextView tvQuestion = new TextView(context);
                var text = valueFucntions[i](dataItem);

                tvQuestion.Text = string.IsNullOrEmpty(text) ? "NA" : text;
                tvQuestion.Gravity = GravityFlags.Left;
                tvQuestion.SetPadding(10, 10, 10, 10);
                //TypedValue.ApplyDimension(ComplexUnitType.Dip, 0,context.Resources.DisplayMetrics)
                view.AddView(tvQuestion, new LinearLayout.LayoutParams(0,
                                                                   ViewGroup.LayoutParams.WrapContent, 1));
            }

            return view;
        }

        void tr_Click(object sender, EventArgs e)
        {
            var typedSender = sender as LinearLayout;
            var screenId = ItemPublicKey.Parse(typedSender.GetTag(Resource.Id.ScreenId).ToString());
            notifier(new ScreenChangedEventArgs(screenId));
        }
    }
}
