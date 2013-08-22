using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class ScreenContentAdapter : SmartAdapter<IQuestionnaireItemViewModel>
    {
        private readonly Context context;
        private readonly Guid questionnaireId;
        private readonly SurveyStatus status;
        private readonly IQuestionViewFactory questionViewFactory;
        private readonly EventHandler<ScreenChangedEventArgs> screenChangeEventHandler;

        public ScreenContentAdapter(IList<IQuestionnaireItemViewModel> items, Context context, Guid questionnaireId,
                                    SurveyStatus status, 
                                    EventHandler<ScreenChangedEventArgs> screenChangeEventHandler)
            : base(items)
        {
            this.context = context;
            this.questionnaireId = questionnaireId;
            this.status = status;
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.screenChangeEventHandler = screenChangeEventHandler;
        }

        protected override View BuildViewItem(IQuestionnaireItemViewModel questionnaireItemViewModel, int position)
        {
            View result = null;
            var question = questionnaireItemViewModel as QuestionViewModel;
            if (question != null)
            {
                var questionView = this.questionViewFactory.CreateQuestionView(context, question, questionnaireId);
                //   this.bindableElements.Add(questionView);
                if (SurveyStatus.IsStatusAllowCapiSync(status))
                {
                    questionView.EnableDisableView(false);
                }
                result = questionView;
            }
            var group = questionnaireItemViewModel as QuestionnaireNavigationPanelItem;
            if (group != null)
            {
                var groupView = new GroupView(context, group);

                var layoutParams = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
                //layoutParams.SetMargins(0, 0, 0, 10);
                groupView.LayoutParameters = layoutParams;
                groupView.ScreenChanged += screenChangeEventHandler;
                result = groupView;
            }
            return result;
        }

        protected override object GetElementFunction(IQuestionnaireItemViewModel dataItem)
        {
            return dataItem.PublicKey;
        }
    }
}