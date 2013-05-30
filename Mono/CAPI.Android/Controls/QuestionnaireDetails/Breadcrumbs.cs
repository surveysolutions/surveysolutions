using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class BreadcrumbsView : LinearLayout
    {
        private readonly Action<ScreenChangedEventArgs> notifier;
        protected readonly IEnumerable<IQuestionnaireViewModel> breadcrumbs;

        public BreadcrumbsView(
            Context context,
            IEnumerable<IQuestionnaireViewModel> breadcrumbs,
            Action<ScreenChangedEventArgs> notifier) :
                base(context)
        {
            this.breadcrumbs = breadcrumbs;
            this.notifier = notifier;
            Initialize();
        }

        private void Initialize()
        {
            this.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                               ViewGroup.LayoutParams.WrapContent);
            this.Orientation = Orientation.Vertical;
            var buttons = new List<Button>();
            foreach (var questionnaireNavigationPanelItem in breadcrumbs)
            {
                Button crumb = new Button(this.Context);
                crumb.Text = questionnaireNavigationPanelItem.ScreenName;
                crumb.SetTag(Resource.Id.ScreenId, questionnaireNavigationPanelItem.ScreenId.ToString());
                crumb.Click += crumb_Click;
                var butParam = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                             ViewGroup.LayoutParams.WrapContent);

                var img = Context.Resources.GetDrawable(global::Android.Resource.Drawable.IcMediaPlay);
                //img.SetBounds(0, 0, 45, 45);
                crumb.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);

                butParam.SetMargins(0, 0, 10, 0);
                crumb.LayoutParameters = butParam;

                buttons.Add(crumb);
            }

            PopulateBreadcrumbs(this, buttons, this.Context);
            buttons.Last().Enabled = false;
        }

        void crumb_Click(object sender, EventArgs e)
        {
            var crumb = sender as Button;
            if (crumb == null)
                return;
            var screenId = ItemPublicKey.Parse(crumb.GetTag(Resource.Id.ScreenId).ToString());
            notifier(new ScreenChangedEventArgs(screenId));
        }

        private void PopulateBreadcrumbs(LinearLayout ll, List<Button> views, Context mContext)
        {
            //  Display display = ((Activity)mContext).WindowManager.DefaultDisplay;
            ll.RemoveAllViews();
            this.Measure(0, 0);
            int maxWidth = this.MeasuredWidth - 20;

            LinearLayout.LayoutParams lparams;
            LinearLayout newLL = new LinearLayout(mContext);
            newLL.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                      LayoutParams.WrapContent);
            newLL.SetGravity(GravityFlags.Left);
            newLL.Orientation = Orientation.Horizontal;

            int widthSoFar = 0;

            for (int i = 0; i < views.Count; i++)
            {
                LinearLayout LL = new LinearLayout(mContext);
                LL.Orientation = Orientation.Horizontal;
                LL.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.Bottom);
                LL.LayoutParameters = new ListView.LayoutParams(
                    LayoutParams.WrapContent, LayoutParams.WrapContent);
                views[i].Measure(0, 0);
                lparams = new LinearLayout.LayoutParams(views[i].MeasuredWidth,
                                                        LayoutParams.WrapContent);
                LL.AddView(views[i], lparams);
                LL.Measure(0, 0);
                widthSoFar += views[i].MeasuredWidth; // YOU MAY NEED TO ADD THE MARGINS
                if (widthSoFar >= maxWidth)
                {
                    ll.AddView(newLL);

                    newLL = new LinearLayout(mContext);
                    newLL.LayoutParameters = new LayoutParams(
                        LayoutParams.FillParent,
                        LayoutParams.WrapContent);
                    newLL.Orientation = Orientation.Horizontal;
                    newLL.SetGravity(GravityFlags.Left);
                    lparams = new LinearLayout.LayoutParams(LL.MeasuredWidth, LL.MeasuredHeight);
                    newLL.AddView(LL, lparams);
                    widthSoFar = LL.MeasuredWidth;
                }
                else
                {
                    newLL.AddView(LL);
                }
            }
            ll.AddView(newLL);
        }
    }
}