using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.Core;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Events;
using AndroidApp.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Java.Interop;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class ScreenContentFragment : AbstractScreenChangingFragment
    {
        private readonly IQuestionViewFactory questionViewFactory;
        private readonly CompleteQuestionnaireView questionnaire;
        protected List<AbstractQuestionView> bindableElements = new List<AbstractQuestionView>();
        protected View top;
        public ScreenContentFragment()
        {
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.bindableElements = new List<AbstractQuestionView>();
            this.RetainInstance = true;
        }

        public ScreenContentFragment(QuestionnaireScreenViewModel model, CompleteQuestionnaireView questionnaire)
            : this()
        {
           
            this.Model = model;
            this.questionnaire = questionnaire;
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
            //  top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);

            breadcrumbs.SetPadding(0, 0, 0, 10);
            llTop.AddView(breadcrumbs);

            var previousBtn = new GroupView(inflater.Context,
                                            PropagatedModel == null
                                                ? null
                                                : PropagatedModel.Previous as QuestionnaireNavigationPanelItem, Android.Resource.Drawable.ArrowUpFloat);

            previousBtn.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);
            llTop.AddView(previousBtn);



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
                                            : PropagatedModel.Next as QuestionnaireNavigationPanelItem, Android.Resource.Drawable.ArrowDownFloat);


            nextBtn.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);

            llButtom.AddView(nextBtn);


            return top;
        }

        public override void OnDestroy()
        {
           
            base.OnDestroy();
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


        public QuestionnaireScreenViewModel Model { get; private set; }
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