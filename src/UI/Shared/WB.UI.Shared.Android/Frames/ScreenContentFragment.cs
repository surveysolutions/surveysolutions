using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Controls;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Events;
using WB.UI.Shared.Android.Extensions;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.Shared.Android.Frames
{
    public abstract class ScreenContentFragment : AbstractScreenChangingFragment
    {
        protected abstract IQuestionViewFactory GetQuestionViewFactory();

        protected abstract QuestionnaireScreenViewModel GetScreenViewModel();
        protected abstract List<IQuestionnaireViewModel> GetBreadcrumbs();
        protected abstract InterviewStatus GetInterviewStatus();

        public const string SCREEN_ID = "screenId";
        public const string INTERVIEW_ID = "interviewId";
        protected View top;

        public ScreenContentFragment()
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }

            if (!this.Arguments.ContainsKey(INTERVIEW_ID))
            {
                throw new ArgumentException("Interview id is missing");
            }

            if (!this.Arguments.ContainsKey(SCREEN_ID))
            {
                throw new ArgumentException("Screen id is missing");
            }

            this.top = inflater.Inflate(Resource.Layout.ScreenContentFragment, null);
           
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  GetBreadcrumbs(),
                                                  this.OnScreenChanged);

            breadcrumbs.SetPadding(0, 0, 0, 10);
            this.llTop.AddView(breadcrumbs);

            this.llContent.Adapter = new ScreenContentAdapter(this.Model, this.Activity, this.Model.QuestionnaireId,
               this.GetInterviewStatus(), this.groupView_ScreenChanged, this.GetQuestionViewFactory());
            this.llContent.ItemsCanFocus = true;
            this.llContent.ScrollingCacheEnabled = false;
            this.llContent.AttachCheckAndClearFocusForPanel(this.Activity);

            this.llContent.ChildViewRemoved += this.llContent_ChildViewRemoved;
           
            return this.top;
        }

        void llContent_ChildViewRemoved(object sender, ViewGroup.ChildViewRemovedEventArgs e)
        {
            e.Child.Dispose();
        }

        public override void OnViewStateRestored(Bundle savedInstanceState)
        {
            base.OnViewStateRestored(savedInstanceState);
            this.llContent.SetSelection(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.llContent != null)
                {
                    this.llContent.TryClearBindingsIfPossibleForChildren();

                    if (this.llContent.Adapter != null)
                    {
                        this.llContent.Adapter.Dispose();
                        this.llContent.Adapter = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        private void groupView_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            this.OnScreenChanged(e);
        }

        private QuestionnaireScreenViewModel model;
        public QuestionnaireScreenViewModel Model
        {
            get {
                if (this.model == null)
                {
                    this.model = GetScreenViewModel();
                }
                return this.model;
            }
        }

        protected Guid QuestionnaireId {
            get { return Guid.Parse(this.Arguments.GetString(INTERVIEW_ID)); }
        }

        protected InterviewItemId ScreenId
        {
            get { return InterviewItemId.Parse(this.Arguments.GetString(SCREEN_ID)); }
        }

        protected QuestionnairePropagatedScreenViewModel PropagatedModel
        {
            get { return this.Model as QuestionnairePropagatedScreenViewModel; }
        }

        protected ListView llContent
        {
            get { return this.top.FindViewById<ListView>(Resource.Id.llContent); }
        }
        protected LinearLayout llTop
        {
            get { return this.top.FindViewById<LinearLayout>(Resource.Id.llTop); }
        }
        protected LinearLayout llButtom
        {
            get { return this.top.FindViewById<LinearLayout>(Resource.Id.llButtom); }
        }
    }
}