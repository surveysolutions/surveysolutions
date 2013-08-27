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
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class GridContentFragment : AbstractScreenChangingFragment
    {
        protected TextView tvEmptyLabelDescription;
        protected ListView llTablesContainer;
        protected LinearLayout top;
        private const string SCREEN_ID = "screenId";
        private const string QUESTIONNAIRE_ID = "questionnaireId";

        public static GridContentFragment NewInstance(ItemPublicKey screenId, Guid questionnaireId)
        {
            GridContentFragment myFragment = new GridContentFragment();

            Bundle args = new Bundle();
            args.PutString(SCREEN_ID, screenId.ToString());
            args.PutString(QUESTIONNAIRE_ID, questionnaireId.ToString());
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

            top = new LinearLayout(inflater.Context);
            top.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent);
            top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  Questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            top.AddView(breadcrumbs);
            BuildEmptyLabelDescription(inflater.Context);
            BuildTabels(inflater.Context);
            return top;
        }

        protected void BuildEmptyLabelDescription(Context context)
        {
            tvEmptyLabelDescription = new TextView(context);
            tvEmptyLabelDescription.Gravity = GravityFlags.Center;
            tvEmptyLabelDescription.TextSize = 22;
            tvEmptyLabelDescription.SetPadding(10, 10, 10, 10);
            tvEmptyLabelDescription.Text = "Questions are absent";
            tvEmptyLabelDescription.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            if (Model.Rows.Any(r => r.Enabled))
                tvEmptyLabelDescription.Visibility=ViewStates.Gone;
            top.AddView(tvEmptyLabelDescription);
        }

        protected void BuildTabels(Context context)
        {
            llTablesContainer = new ListView(context);
            llTablesContainer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            const int columnCount = 2;
            llTablesContainer.Adapter = new GridContentAdapter(Model, columnCount, this.Activity, OnScreenChanged,
                                                               tvEmptyLabelDescription);
            if (!Model.Rows.Any(r => r.Enabled))
                llTablesContainer.Visibility = ViewStates.Gone;
            top.AddView(llTablesContainer);
        }

        private QuestionnaireGridViewModel model;

        public QuestionnaireGridViewModel Model
        {
            get
            {
                if (model == null)
                {
                    model = Questionnaire.Screens[ItemPublicKey.Parse(Arguments.GetString(SCREEN_ID))] as QuestionnaireGridViewModel;
                }
                return model;
            }
        }

        private CompleteQuestionnaireView questionnaire;

        protected CompleteQuestionnaireView Questionnaire
        {
            get
            {
                if (questionnaire == null)
                {
                    questionnaire = CapiApplication.LoadView<QuestionnaireScreenInput, CompleteQuestionnaireView>(
                        new QuestionnaireScreenInput(Guid.Parse(Arguments.GetString(QUESTIONNAIRE_ID))));
                }
                return questionnaire;
            }
        }
    }
}
