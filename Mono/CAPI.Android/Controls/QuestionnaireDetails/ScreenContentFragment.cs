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

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class ScreenContentFragment : AbstractScreenChangingFragment
    {
        public static ScreenContentFragment NewInstance(ItemPublicKey screenId, Guid questionnaireId)
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
        private readonly IQuestionViewFactory questionViewFactory;
        
        protected List<AbstractQuestionView> bindableElements = new List<AbstractQuestionView>();
        protected View top;
        public ScreenContentFragment()
        {
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.bindableElements = new List<AbstractQuestionView>();
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

            previousBtn.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);
            llTop.AddView(previousBtn);
            //  top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);

            breadcrumbs.SetPadding(0, 0, 0, 10);
            llTop.AddView(breadcrumbs);

          



            foreach (var item in Model.Items)
            {
                var question = item as QuestionViewModel;
                View itemView = null;
                if (question != null)
                {
                    var questionView = this.questionViewFactory.CreateQuestionView(inflater.Context, question,
                                                                                   Model.QuestionnaireId);
                    this.bindableElements.Add(questionView);
                    itemView = questionView;
                }
                var group = item as QuestionnaireNavigationPanelItem;
                if (group != null)
                {
                    var groupView = new GroupView(inflater.Context, group);
                    groupView.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);
                    itemView = groupView;
                }
                if (itemView != null)
                    llContent.AddView(itemView);
            }
            llContent.EnableDisableView(!SurveyStatus.IsStatusAllowCapiSync(questionnaire.Status));

            var nextBtn = new GroupView(inflater.Context,
                                        PropagatedModel == null
                                            ? null
                                            : PropagatedModel.Next as QuestionnaireNavigationPanelItem, global::Android.Resource.Drawable.ArrowDownFloat);


            nextBtn.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);

            llButtom.AddView(nextBtn);


            return top;
        }

        public override void OnDetach()
        {

            base.OnDetach();
            foreach (AbstractQuestionView abstractQuestionView in bindableElements)
            {
                abstractQuestionView.Dispose();
            }
            bindableElements=new List<AbstractQuestionView>();
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
                    model = Questionnaire.Screens[ItemPublicKey.Parse(Arguments.GetString(SCREEN_ID))] as QuestionnaireScreenViewModel;
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

        protected QuestionnairePropagatedScreenViewModel PropagatedModel
        {
            get { return Model as QuestionnairePropagatedScreenViewModel; }
        }

        protected LinearLayout llContent
        {
            get { return top.FindViewById<LinearLayout>(Resource.Id.llContent); }
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