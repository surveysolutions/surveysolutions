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
        protected ScreenContentFragment()
        {
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.RetainInstance = true;
            this.questionViewFactory = new DefaultQuestionViewFactory();
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
            LinearLayout top = new LinearLayout(inflater.Context);
            top.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent);
            top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            top.AddView(breadcrumbs);

            ScrollView sv = new ScrollView(inflater.Context);
            sv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            
            LinearLayout ll = new LinearLayout(inflater.Context);
            ll.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            ll.Orientation = Orientation.Vertical;

            sv.AddView(ll);



            foreach (var item in Model.Items)
            {
                var question = item as QuestionViewModel;
                View itemView = null;
                if (question != null)
                {
                    itemView = this.questionViewFactory.CreateQuestionView(inflater.Context, question,
                                                                           Model.QuestionnaireId);
                }
                var group = item as QuestionnaireNavigationPanelItem;
                if (group != null)
                {
                    var groupView = new GroupView(inflater.Context, group);
                    groupView.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);
                    itemView = groupView;
                }
                if (itemView != null)
                    ll.AddView(itemView);
            }
            sv.EnableDisableView(!SurveyStatus.IsStatusAllowCapiSync(questionnaire.Status));
            top.AddView(sv);
            return top;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }

        private void groupView_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            OnScreenChanged(e);
        }


        public QuestionnaireScreenViewModel Model { get; private set; }

    }
}