using System;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Shared.Adapters;
using WB.UI.Capi.Shared.Controls;
using WB.UI.Capi.Shared.Controls.ScreenItems;
using WB.UI.Capi.Shared.Events;
using WB.UI.Capi.Shared.Extensions;

namespace WB.UI.Capi.Shared.Frames
{
    public abstract class ScreenContentFragment : AbstractScreenChangingFragment
    {
        protected abstract IQuestionViewFactory GetQuestionViewFactory();
        protected abstract InterviewViewModel GetInterviewViewModel(Guid questionnaireId);

        public const string SCREEN_ID = "screenId";
        public const string QUESTIONNAIRE_ID = "questionnaireId";
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

            this.top = inflater.Inflate(Resource.Layout.ScreenContentFragment, null);
            var previousBtn = new GroupView(inflater.Context,
                                          this.PropagatedModel == null
                                              ? null
                                              : this.PropagatedModel.Previous as QuestionnaireNavigationPanelItem, global::Android.Resource.Drawable.ArrowUpFloat);

            previousBtn.ScreenChanged += this.groupView_ScreenChanged;
            this.llTop.AddView(previousBtn);
            //  top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  this.questionnaire.RestoreBreadCrumbs(this.Model.Breadcrumbs).ToList(),
                                                  this.OnScreenChanged);

            breadcrumbs.SetPadding(0, 0, 0, 10);
            this.llTop.AddView(breadcrumbs);

            this.llContent.Adapter = new ScreenContentAdapter(this.Model, this.Activity, this.Model.QuestionnaireId,
                this.questionnaire.Status, this.groupView_ScreenChanged, this.GetQuestionViewFactory());
            this.llContent.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            this.llContent.ItemsCanFocus = true;
            this.llContent.ScrollingCacheEnabled = false;
            this.llContent.ChildViewRemoved += this.llContent_ChildViewRemoved;
            var nextBtn = new GroupView(inflater.Context,
                                        this.PropagatedModel == null
                                            ? null
                                            : this.PropagatedModel.Next as QuestionnaireNavigationPanelItem, global::Android.Resource.Drawable.ArrowDownFloat);


            nextBtn.ScreenChanged += this.groupView_ScreenChanged;

            this.llButtom.AddView(nextBtn);

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
                    this.model = this.Questionnaire.Screens[InterviewItemId.Parse(this.Arguments.GetString(SCREEN_ID))] as QuestionnaireScreenViewModel;
                }
                return this.model;
            }
        }

        private InterviewViewModel questionnaire;

        protected InterviewViewModel Questionnaire
        {
            get
            {
                if (this.questionnaire == null)
                {
                    this.questionnaire = GetInterviewViewModel(Guid.Parse(this.Arguments.GetString(QUESTIONNAIRE_ID)));
                }
                return this.questionnaire;
            }
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