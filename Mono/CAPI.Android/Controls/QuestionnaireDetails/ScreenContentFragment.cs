using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Main.Core.Entities.SubEntities;
using Ninject;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class ScreenContentFragment : AbstractScreenChangingFragment
    {
        public static ScreenContentFragment NewInstance(InterviewItemId screenId, Guid questionnaireId)
        {
            ScreenContentFragment myFragment = new ScreenContentFragment();

            Bundle args = new Bundle();
            args.PutString(SCREEN_ID, screenId.ToString());
            args.PutString(QUESTIONNAIRE_ID, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }

        private const string SCREEN_ID = "screenId";
        private const string QUESTIONNAIRE_ID = "questionnaireId";
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

            top = inflater.Inflate(Resource.Layout.ScreenContentFragment, null);
            var previousBtn = new GroupView(inflater.Context,
                                          PropagatedModel == null
                                              ? null
                                              : PropagatedModel.Previous as QuestionnaireNavigationPanelItem, global::Android.Resource.Drawable.ArrowUpFloat);

            previousBtn.ScreenChanged += groupView_ScreenChanged;
            llTop.AddView(previousBtn);
            //  top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);

            breadcrumbs.SetPadding(0, 0, 0, 10);
            llTop.AddView(breadcrumbs);

            llContent.Adapter = new ScreenContentAdapter(Model, this.Activity, Model.QuestionnaireId,questionnaire.Status, groupView_ScreenChanged);
            llContent.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            llContent.ItemsCanFocus = true;
            llContent.ScrollingCacheEnabled = false;
            llContent.ChildViewRemoved += llContent_ChildViewRemoved;
            var nextBtn = new GroupView(inflater.Context,
                                        PropagatedModel == null
                                            ? null
                                            : PropagatedModel.Next as QuestionnaireNavigationPanelItem, global::Android.Resource.Drawable.ArrowDownFloat);


            nextBtn.ScreenChanged += groupView_ScreenChanged;

            llButtom.AddView(nextBtn);

            return top;
        }

        void llContent_ChildViewRemoved(object sender, ViewGroup.ChildViewRemovedEventArgs e)
        {
            e.Child.Dispose();
        }

        public override void OnViewStateRestored(Bundle savedInstanceState)
        {
            base.OnViewStateRestored(savedInstanceState);
            llContent.SetSelection(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (llContent != null)
                {
                    llContent.TryClearBindingsIfPossibleForChildren();

                    if (llContent.Adapter != null)
                    {
                        llContent.Adapter.Dispose();
                        llContent.Adapter = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        private void groupView_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            OnScreenChanged(e);
        }

        private QuestionnaireScreenViewModel model;
        public QuestionnaireScreenViewModel Model
        {
            get {
                if (model == null)
                {
                    model = Questionnaire.Screens[InterviewItemId.Parse(Arguments.GetString(SCREEN_ID))] as QuestionnaireScreenViewModel;
                }
                return model;
            }
        }

        private InterviewViewModel questionnaire;

        protected InterviewViewModel Questionnaire
        {
            get
            {
                if (questionnaire == null)
                {
                    questionnaire = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                        new QuestionnaireScreenInput(Guid.Parse(Arguments.GetString(QUESTIONNAIRE_ID))));
                }
                return questionnaire;
            }
        }

        protected QuestionnairePropagatedScreenViewModel PropagatedModel
        {
            get { return Model as QuestionnairePropagatedScreenViewModel; }
        }

        protected ListView llContent
        {
            get { return top.FindViewById<ListView>(Resource.Id.llContent); }
        }
        protected LinearLayout llTop
        {
            get { return top.FindViewById<LinearLayout>(Resource.Id.llTop); }
        }
        protected LinearLayout llButtom
        {
            get { return top.FindViewById<LinearLayout>(Resource.Id.llButtom); }
        }
    }
}