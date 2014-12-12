using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Events;
using Orientation = Android.Widget.Orientation;

namespace WB.UI.Shared.Android.Controls
{
    public class QuestionnaireNavigationView : ScrollView
    {
        #region public fields

        private readonly LinearLayout linearLayout;
        private readonly Color selectedItemColor;

        public QuestionnaireNavigationView(Context context, InterviewViewModel model)
            : base(context)
        {
            TypedValue typedValue = new TypedValue();
            int[] colorAttr = new int[] { global::Android.Resource.Attribute.TextColorHighlightInverse };
            using (var a = context.ObtainStyledAttributes(typedValue.Data, colorAttr))
            {
                selectedItemColor = a.GetColor(0, -1);
                a.Recycle();
            }

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
                view.Touch += view_Touch;
                this.linearLayout.AddView(view);
            }

            var lastView = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);
            var tvLastItem = lastView.FindViewById<TextView>(Resource.Id.tvITem);
            var tvLastCount = lastView.FindViewById<TextView>(Resource.Id.tvCount);
            tvLastItem.Text = this.model.Status == InterviewStatus.Completed ? "Summary" : "Complete";
            tvLastCount.Visibility = ViewStates.Gone;
            lastView.Touch += view_Touch;
            this.linearLayout.AddView(lastView);

            this.linearLayout.GetChildAt(0).SetBackgroundColor(selectedItemColor);
        }

        private void view_Touch(object sender, View.TouchEventArgs e)
        {
            var view = sender as View;
            if (view == null)
                return;
            
            var tag = view.GetTag(Resource.Id.ScreenId);
            InterviewItemId? screenId = null;
            if (tag != null)
            {
                screenId = InterviewItemId.Parse(view.GetTag(Resource.Id.ScreenId).ToString());
            }

            if (e.Event.Action == MotionEventActions.Down)
            {
                view.SetBackgroundColor(selectedItemColor);
                return;
            }
            if (e.Event.Action == MotionEventActions.Up)
            {
                this.SelectItem(view);
                this.OnItemClick(screenId);
            }
        }

        private static void UpdateCounters(QuestionnaireScreenViewModel dataItem, TextView tvCount)
        {
            tvCount.Text = string.Format("{0}/{1}", dataItem.Answered, dataItem.Total);
            tvCount.SetBackgroundResource(dataItem.Total == dataItem.Answered
                ? Resource.Drawable.donecountershape
                : Resource.Drawable.CounterRoundShape);
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
                element.SetBackgroundColor(element == this.selectedView
                    ? selectedItemColor
                    : Color.Transparent);
            }
        }
        public void SelectItem(int position)
        {
            var view =this.linearLayout.GetChildAt(position);
            this.SelectItem(view);
        }

    }
}