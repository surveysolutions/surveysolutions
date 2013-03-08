using System;
using System.Collections.Generic;
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
            this.Orientation = Orientation.Horizontal;
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
                
                this.AddView(crumb);
            }
            this.GetChildAt(this.ChildCount - 1).Enabled = false;
        }

        void crumb_Click(object sender, EventArgs e)
        {
            var crumb = sender as Button;
            if (crumb == null)
                return;
            var screenId = ItemPublicKey.Parse(crumb.GetTag(Resource.Id.ScreenId).ToString());
            notifier(new ScreenChangedEventArgs(screenId));
        }
    }
}