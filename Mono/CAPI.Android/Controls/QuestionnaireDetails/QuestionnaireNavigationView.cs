using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationView : ScrollView
    {
        #region public fields

        private readonly LinearLayout linearLayout;

        public QuestionnaireNavigationView(Context context, InterviewViewModel model)
            : base(context)
        {
            this.model = model;
            linearLayout = new LinearLayout(context);
            linearLayout.Orientation = Orientation.Vertical;
            this.BuildChildren();
            this.AddView(linearLayout);
        }

        private void BuildChildren()
        {
            LayoutInflater layoutInflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);

            foreach (var chapter in model.Chapters)
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
                linearLayout.AddView(view);
            }

            var lastView = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);
            var tvLastItem = lastView.FindViewById<TextView>(Resource.Id.tvITem);
            var tvLastCount = lastView.FindViewById<TextView>(Resource.Id.tvCount);
            tvLastItem.Text = model.Status == InterviewStatus.Completed ? "Summary" : "Complete";
            tvLastCount.Visibility = ViewStates.Gone;
            lastView.Click += view_Click;
            linearLayout.AddView(lastView);

            linearLayout.GetChildAt(0).SetBackgroundColor(Color.LightBlue);
        }

        private void view_Click(object sender, EventArgs e)
        {
            var view = sender as View;
            if (view == null)
                return;
            SelectItem(view);
            var tag = view.GetTag(Resource.Id.ScreenId);
            InterviewItemId? screenId = null;
            if (tag != null)
            {
                screenId = InterviewItemId.Parse(view.GetTag(Resource.Id.ScreenId).ToString());
            }
            OnItemClick(screenId);
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
            var handler = ScreenChanged;
            if (handler != null)
                handler(this, new ScreenChangedEventArgs(groupKey));
        }

        public void SelectItem(View view)
        {
            if (selectedView == view)
                return;
            selectedView = view;

            for (int i = 0; i < linearLayout.ChildCount; i++)
            {
                var element = linearLayout.GetChildAt(i);
                if (element == null)
                    continue;
                element.SetBackgroundColor(element == selectedView ? Color.LightBlue : Color.Transparent);
            }
        }
        public void SelectItem(int position)
        {
            var view =linearLayout.GetChildAt(position);
            SelectItem(view);
        }

    }
}