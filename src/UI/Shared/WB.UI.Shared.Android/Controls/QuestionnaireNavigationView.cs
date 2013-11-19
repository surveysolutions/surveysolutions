using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Controls
{
    public class QuestionnaireNavigationView : ScrollView
    {
        #region public fields

        private readonly LinearLayout linearLayout;

        public QuestionnaireNavigationView(Context context, InterviewViewModel model)
            : base(context)
        {
            this.model = model;
            this.linearLayout = new LinearLayout(context);
            this.linearLayout.Orientation = Orientation.Vertical;
            this.BuildChildren();
            this.AddView(this.linearLayout);
        }

        private void BuildChildren()
        {
            LayoutInflater layoutInflater = (LayoutInflater) this.Context.GetSystemService(Context.LayoutInflaterService);

            foreach (var chapter in this.model.Chapters)
            {
                var view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);
                var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);
                var tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);

                chapter.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                        return;

                    UpdateCounters(chapter, tvCount);
                };
                tvITem.Text = chapter.ScreenName;
                UpdateCounters(chapter, tvCount);
                view.SetTag(Resource.Id.ScreenId, chapter.ScreenId.ToString());
                view.Click += view_Click;
                this.linearLayout.AddView(view);
            }

            var lastView = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);
            var tvLastItem = lastView.FindViewById<TextView>(Resource.Id.tvITem);
            var tvLastCount = lastView.FindViewById<TextView>(Resource.Id.tvCount);
            tvLastItem.Text = this.model.Status == InterviewStatus.Completed ? "Summary" : "Complete";
            tvLastCount.Visibility = ViewStates.Gone;
            lastView.Click += view_Click;
            this.linearLayout.AddView(lastView);

            this.linearLayout.GetChildAt(0).SetBackgroundColor(Color.LightBlue);
        }

        private void view_Click(object sender, EventArgs e)
        {
            var view = sender as View;
            if (view == null)
                return;
            this.SelectItem(view);
            var tag = view.GetTag(Resource.Id.ScreenId);
            InterviewItemId? screenId = null;
            if (tag != null)
            {
                screenId = InterviewItemId.Parse(view.GetTag(Resource.Id.ScreenId).ToString());
            }
            this.OnItemClick(screenId);
        }


        private static void UpdateCounters(QuestionnaireScreenViewModel dataItem, TextView tvCount)
        {
            tvCount.Text = string.Format("{0}/{1}", dataItem.Answered, dataItem.Total);
            if (dataItem.Total == dataItem.Answered)
                tvCount.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                tvCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }
        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
        private InterviewViewModel model;


        private View selectedView = null;

        #endregion

        protected void OnItemClick(InterviewItemId? groupKey)
        {
            var handler = this.ScreenChanged;
            if (handler != null)
                handler(this, new ScreenChangedEventArgs(groupKey));
        }

        public void SelectItem(View view)
        {
            if (this.selectedView == view)
                return;
            this.selectedView = view;

            for (int i = 0; i < this.linearLayout.ChildCount; i++)
            {
                var element = this.linearLayout.GetChildAt(i);
                if (element == null)
                    continue;
                element.SetBackgroundColor(element == this.selectedView ? Color.LightBlue : Color.Transparent);
            }
        }
        public void SelectItem(int position)
        {
            var view =this.linearLayout.GetChildAt(position);
            this.SelectItem(view);
        }

    }
}