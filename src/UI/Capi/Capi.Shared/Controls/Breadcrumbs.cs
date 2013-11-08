using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Shared.Events;

namespace WB.UI.Capi.Shared.Controls
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
            this.Initialize();
        }

        private void Initialize()
        {
            this.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                               ViewGroup.LayoutParams.WrapContent);
            this.Orientation = Orientation.Vertical;

            var firstBreadcrumbLine = this.CreateOneLineOfBreadcrumbs();
            this.AddView(firstBreadcrumbLine);

            foreach (var questionnaireNavigationPanelItem in this.breadcrumbs)
            {
                var crumbButton = this.CreateBreadCrumbButton(questionnaireNavigationPanelItem);

                this.DisableBreadcrumbIfLast(questionnaireNavigationPanelItem, crumbButton);

                var crumbWrapper = this.CreateBreadCrumbWrapper();
                crumbWrapper.AddView(crumbButton);
                firstBreadcrumbLine.AddView(crumbWrapper);
            }
            this.PopulateBreadcrumbs(this);
        }

        private void DisableBreadcrumbIfLast(IQuestionnaireViewModel questionnaireNavigationPanelItem, Button crumbButton)
        {
            if (this.breadcrumbs.Last() == questionnaireNavigationPanelItem)
                crumbButton.Enabled = false;
        }

        private Button CreateBreadCrumbButton(IQuestionnaireViewModel questionnaireNavigationPanelItem)
        {
            Button crumb = new Button(this.Context);
            crumb.Text = questionnaireNavigationPanelItem.ScreenName;
            crumb.SetTag(Resource.Id.ScreenId, questionnaireNavigationPanelItem.ScreenId.ToString());
            crumb.Click += this.crumb_Click;
            var butParam = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                                                         ViewGroup.LayoutParams.WrapContent);

            var img = this.Context.Resources.GetDrawable(global::Android.Resource.Drawable.IcMediaPlay);
            //img.SetBounds(0, 0, 45, 45);
            crumb.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);

            butParam.SetMargins(0, 0, 10, 0);
            crumb.LayoutParameters = butParam;
            return crumb;
        }

        private LinearLayout CreateBreadCrumbWrapper()
        {
            LinearLayout LL = new LinearLayout(this.Context);
            LL.Orientation = Orientation.Horizontal;
            LL.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.Bottom);
            LL.LayoutParameters = new ListView.LayoutParams(
                LayoutParams.WrapContent, LayoutParams.WrapContent);

            return LL;
        }

        void crumb_Click(object sender, EventArgs e)
        {
            var crumb = sender as Button;
            if (crumb == null)
                return;
            var screenId = InterviewItemId.Parse(crumb.GetTag(Resource.Id.ScreenId).ToString());
            this.notifier(new ScreenChangedEventArgs(screenId));
        }

        private void PopulateBreadcrumbs(LinearLayout ll)
        {
            var breadcrumbsViews = this.GetPresentBreaCrumbs(ll);

            var screenWidth = this.ScreenWidth();

            int widthSoFar = 0;

            var currentLineView = ll.GetChildAt(0) as LinearLayout;

            for (int i = 0; i < breadcrumbsViews.Count; i++)
            {
                LinearLayout breadcrumbView = breadcrumbsViews[i];

                var breadcrumbWith = this.AssignMesuredWidthToBreadcrumbAndReturnIt(breadcrumbView);

                widthSoFar += breadcrumbWith;

                if (widthSoFar >= screenWidth)
                {
                    currentLineView = this.PutBreadCrumbAtNextLineAndReturnNextLine(breadcrumbView);
                    widthSoFar = breadcrumbWith;
                }

                this.CheckIsBreadCrumbIsAtCurrentLineIfNotMoveIt(currentLineView, breadcrumbView);
            }
            this.DeleteAllEmptyLines(ll);
        }

        private List<LinearLayout> GetPresentBreaCrumbs(LinearLayout ll)
        {
            var result = new List<LinearLayout>();

            for (int i = 0; i < ll.ChildCount; i++)
            {
                var lineView = ll.GetChildAt(i) as LinearLayout;
                for (int j = 0; j < lineView.ChildCount; j++)
                {
                    result.Add(lineView.GetChildAt(j) as LinearLayout);
                }
            }

            return result;
        }

        private void DeleteAllEmptyLines(LinearLayout ll)
        {
            for (int i = 0; i < ll.ChildCount; i++)
            {
                var line = ll.GetChildAt(i) as LinearLayout;
                if (line.ChildCount == 0)
                    ll.RemoveView(line);
            }
        }

        private void CheckIsBreadCrumbIsAtCurrentLineIfNotMoveIt(LinearLayout currentLineView,
                                                                 LinearLayout breadcrumbView)
        {
            if (breadcrumbView.Parent == currentLineView)
                return;
            var breadcrumbLine = breadcrumbView.Parent as LinearLayout;
            breadcrumbLine.RemoveView(breadcrumbView);
            currentLineView.AddView(breadcrumbView);
        }

        private LinearLayout PutBreadCrumbAtNextLineAndReturnNextLine(LinearLayout breadcrumbView)
        {
            var currentLine = breadcrumbView.Parent as LinearLayout;
            var wholeBreadCrumbContainer = currentLine.Parent as LinearLayout;
            var currentLinePosition = this.GetCurrentLinePositionInContainer(wholeBreadCrumbContainer, currentLine);
            LinearLayout nextLine = null;
            if (currentLinePosition < wholeBreadCrumbContainer.ChildCount - 1)
            {
                nextLine = wholeBreadCrumbContainer.GetChildAt(currentLinePosition + 1) as LinearLayout;
            }
            else
            {
                nextLine = this.CreateOneLineOfBreadcrumbs();
                wholeBreadCrumbContainer.AddView(nextLine);
            }
            currentLine.RemoveView(breadcrumbView);
            nextLine.AddView(breadcrumbView);
            return nextLine;
        }

        private int GetCurrentLinePositionInContainer(LinearLayout wholeBreadCrumbContainer, LinearLayout currentLine)
        {
            for (int i = 0; i < wholeBreadCrumbContainer.ChildCount; i++)
            {
                if (wholeBreadCrumbContainer.GetChildAt(i) == currentLine) return i;
            }
            return -1;
        }

        private int AssignMesuredWidthToBreadcrumbAndReturnIt(LinearLayout breadcrumbView)
        {
            breadcrumbView.Measure(0, 0);
            breadcrumbView.LayoutParameters = new LinearLayout.LayoutParams(breadcrumbView.MeasuredWidth,
                                                                            LayoutParams.WrapContent);
            return breadcrumbView.MeasuredWidth;
        }

        private LinearLayout CreateOneLineOfBreadcrumbs()
        {
            LinearLayout newLL = new LinearLayout(this.Context);
            newLL.LayoutParameters = new LayoutParams(LayoutParams.FillParent,
                                                      LayoutParams.WrapContent);
            newLL.SetGravity(GravityFlags.Left);
            newLL.Orientation = Orientation.Horizontal;
            return newLL;
        }

        private int ScreenWidth()
        {
            Display display = ((Activity)this.Context).WindowManager.DefaultDisplay;
            Point size = new Point();
            display.GetSize(size);
            return size.X - 20;
         /*   var parent = this.Parent as View ?? this;
            parent.Measure(0, 0);
            int maxWidth = parent.MeasuredWidth - 20;
            return maxWidth;*/
        }
    }
}