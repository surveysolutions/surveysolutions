using System;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Controls;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Extensions;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.Shared.Android.Frames
{
    public abstract class GridContentFragment : AbstractScreenChangingFragment
    {
        protected TextView TvEmptyLabelDescription;
        protected ListView LlTablesContainer;
        protected LinearLayout Top;
        public const string ScreenId = "screenId";
        public const string QuestionnaireId = "questionnaireId";

        public GridContentFragment(): base(){}

        protected abstract IQuestionViewFactory GetQuestionViewFactory();
        protected abstract InterviewViewModel GetInterviewViewModel(Guid questionnaireId);

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
                                                  this.Questionnaire.RestoreBreadCrumbs(this.Model.Breadcrumbs).ToList(),
                                                  this.OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            this.Top.AddView(breadcrumbs);
            
            this.BuildEmptyLabelDescription(inflater.Context);
            this.BuildTables(inflater.Context);

            this.AjustControlVisibility();

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
            if (this.Model == null || this.Model.Rows == null)
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
                var tableVisible = this.Model.Rows.Any(r => r.Enabled);
                this.LlTablesContainer.Visibility = tableVisible ? ViewStates.Visible : ViewStates.Invisible;
                this.TvEmptyLabelDescription.Visibility = tableVisible ? ViewStates.Gone : ViewStates.Visible;
            }
        }

        private void AjustControlVisibility()
        {
            if (this.IsRostersAreVisible())
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

        protected void BuildTables(Context context)
        {
            this.LlTablesContainer = new ListView(context);
            this.LlTablesContainer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            const int columnCount = 2;
            this.LlTablesContainer.Adapter = new GridContentAdapter(this.Model, columnCount, this.Activity, this.OnScreenChanged, this.GetQuestionViewFactory());
            this.LlTablesContainer.ScrollingCacheEnabled = false;

            this.LlTablesContainer.AttachCheckAndClearFocusForPanel(this.Activity);
            this.LlTablesContainer.ChildViewRemoved += this.LlTablesContainer_ChildViewRemoved;
            this.Top.AddView(this.LlTablesContainer);
        }

        void LlTablesContainer_ChildViewRemoved(object sender, ViewGroup.ChildViewRemovedEventArgs e)
        {
            e.Child.Dispose();
        }

        private bool IsRostersAreVisible()
        {
            return this.Model.Rows.Any(r => r.Enabled);
        }

        private QuestionnaireGridViewModel model;

        public QuestionnaireGridViewModel Model
        {
            get 
            {
                return this.model ?? (this.model = this.Questionnaire.Screens[this.Arguments.GetString(ScreenId)] as QuestionnaireGridViewModel);
            }
        }

        private InterviewViewModel questionnaire;

        protected InterviewViewModel Questionnaire
        {
            get 
            {
                return this.questionnaire ?? (this.questionnaire = this.GetInterviewViewModel(Guid.Parse(this.Arguments.GetString(QuestionnaireId))));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if DEBUG
                Console.WriteLine("disposing roster");
#endif
                this.UnSubscribeModelOnRowDisable();

                if (this.LlTablesContainer != null)
                {
                    this.LlTablesContainer.TryClearBindingsIfPossibleForChildren();

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
