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
                var butParam = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                               ViewGroup.LayoutParams.WrapContent);
                butParam.SetMargins(0, 0, 10, 0);
                crumb.LayoutParameters = butParam;
                
                this.AddView(crumb);
            }
        }
    }
}