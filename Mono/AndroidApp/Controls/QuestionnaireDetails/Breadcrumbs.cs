using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Events;
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class BreadcrumbsView : LinearLayout
    {
        private readonly Action<ScreenChangedEventArgs> notifier;
        protected readonly IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs;

        public BreadcrumbsView(
            Context context,
            IEnumerable<QuestionnaireNavigationPanelItem> breadcrumbs,
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
            foreach (QuestionnaireNavigationPanelItem questionnaireNavigationPanelItem in breadcrumbs)
            {
                Button crumb = new Button(this.Context);
                crumb.Text = questionnaireNavigationPanelItem.Title;
                crumb.SetTag(Resource.Id.ScreenId, questionnaireNavigationPanelItem.ScreenPublicKey.ToString());
                crumb.Click += crumb_Click;
                var butParam = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                               ViewGroup.LayoutParams.WrapContent);

                var img = Context.Resources.GetDrawable(Android.Resource.Drawable.IcMediaPlay);
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