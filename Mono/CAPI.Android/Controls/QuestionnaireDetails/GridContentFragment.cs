using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.Roster;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class GridContentFragment : AbstractScreenChangingFragment
    {
        protected TextView TvEmptyLabelDescription;
        protected ListView LlTablesContainer;
        protected LinearLayout Top;
        private const string ScreenId = "screenId";
        private const string QuestionnaireId = "questionnaireId";

        public static GridContentFragment NewInstance(InterviewItemId screenId, Guid questionnaireId)
        {
            GridContentFragment myFragment = new GridContentFragment();

            Bundle args = new Bundle();
            args.PutString(ScreenId, screenId.ToString());
            args.PutString(QuestionnaireId, questionnaireId.ToString());
            myFragment.Arguments = args;

            return myFragment;
        }

        public GridContentFragment()
            : base()
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                return null;
            }

            this.Top = new LinearLayout(inflater.Context);
            this.Top.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent);
            this.Top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  Questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            this.Top.AddView(breadcrumbs);
            
            BuildEmptyLabelDescription(inflater.Context);
            BuildTabels(inflater.Context);

            AjustControlVisibility();

            this.SubscribeModelOnRowDisable();

            return this.Top;
        }

        private void SubscribeModelOnRowDisable()
        {
            foreach (var row in this.Model.Rows)
            {
                row.PropertyChanged += this.Model_PropertyChanged;
            }
        }

        private void UnSubscribeModelOnRowDisable()
        {
            if (Model == null || Model.Rows == null)
                return;

            foreach (var row in this.Model.Rows)
            {
                row.PropertyChanged -= this.Model_PropertyChanged;
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                var tableVisible = Model.Rows.Any(r => r.Enabled);
                this.LlTablesContainer.Visibility = tableVisible ? ViewStates.Visible : ViewStates.Invisible;
                this.TvEmptyLabelDescription.Visibility = tableVisible ? ViewStates.Gone : ViewStates.Visible;
            }
        }

        private void AjustControlVisibility()
        {
            if (IsRostersAreVisible())
                this.TvEmptyLabelDescription.Visibility = ViewStates.Gone;
            else
                this.LlTablesContainer.Visibility = ViewStates.Invisible;
        }

        protected void BuildEmptyLabelDescription(Context context)
        {
            this.TvEmptyLabelDescription = new TextView(context);
            this.TvEmptyLabelDescription.Gravity = GravityFlags.Center;
            this.TvEmptyLabelDescription.TextSize = 22;
            this.TvEmptyLabelDescription.SetPadding(10, 10, 10, 10);
            this.TvEmptyLabelDescription.Text = "Questions are absent";
            this.TvEmptyLabelDescription.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            this.Top.AddView(this.TvEmptyLabelDescription);
        }

        protected void BuildTabels(Context context)
        {
            this.LlTablesContainer = new ListView(context);
            this.LlTablesContainer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            const int columnCount = 2;
            this.LlTablesContainer.Adapter = new GridContentAdapter(Model, columnCount, this.Activity, OnScreenChanged);
            this.LlTablesContainer.ScrollingCacheEnabled = false;
            this.LlTablesContainer.ChildViewRemoved += LlTablesContainer_ChildViewRemoved;
            this.Top.AddView(this.LlTablesContainer);
        }

        void LlTablesContainer_ChildViewRemoved(object sender, ViewGroup.ChildViewRemovedEventArgs e)
        {
            e.Child.Dispose();
        }

        private bool IsRostersAreVisible()
        {
            return Model.Rows.Any(r => r.Enabled);
        }

        private QuestionnaireGridViewModel model;

        public QuestionnaireGridViewModel Model
        {
            get
            {
                if (model == null)
                {
                    model = Questionnaire.Screens[InterviewItemId.Parse(Arguments.GetString(ScreenId))] as QuestionnaireGridViewModel;
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
                        new QuestionnaireScreenInput(Guid.Parse(Arguments.GetString(QuestionnaireId))));
                }
                return questionnaire;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Console.WriteLine("disposing roster");
                UnSubscribeModelOnRowDisable();

                if (this.LlTablesContainer != null)
                {
                    LlTablesContainer.TryClearBindingsIfPossibleForChildren();

                    if (this.LlTablesContainer.Adapter != null)
                    {
                        this.LlTablesContainer.Adapter.Dispose();
                        this.LlTablesContainer.Adapter = null;
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}
