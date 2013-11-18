using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Events;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Adapters
{
    public class ScreenContentAdapter : SmartAdapter<IQuestionnaireItemViewModel>
    {
        private readonly Context context;
        private readonly Guid questionnaireId;
        private readonly InterviewStatus status;
        
        private readonly IQuestionViewFactory questionViewFactory;
        private readonly EventHandler<ScreenChangedEventArgs> screenChangeEventHandler;

        public ScreenContentAdapter(QuestionnaireScreenViewModel screen, 
                                    Context context, 
                                    Guid questionnaireId,
                                    InterviewStatus status,
                                    EventHandler<ScreenChangedEventArgs> screenChangeEventHandler, IQuestionViewFactory questionViewFactory)
            : base(screen.Items)
        {
            this.context = context;
            this.questionnaireId = questionnaireId;
            this.status = status;

            this.questionViewFactory = questionViewFactory;
            this.screenChangeEventHandler = screenChangeEventHandler;
        }

        protected override View BuildViewItem(IQuestionnaireItemViewModel questionnaireItemViewModel, int position)
        {
            View result = null;
            var question = questionnaireItemViewModel as QuestionViewModel;
            if (question != null)
            {
                var questionView = this.questionViewFactory.CreateQuestionView(this.context, question, this.questionnaireId);
                if (this.status == InterviewStatus.Completed)
                {
                    questionView.EnableDisableView(false);
                }
                questionView.Clickable = true;
                questionView.Focusable = true;
                result = questionView;
            }
            var group = questionnaireItemViewModel as QuestionnaireNavigationPanelItem;
            if (group != null)
            {
                var groupView = new GroupView(this.context, group);

                var layoutParams = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                    ViewGroup.LayoutParams.WrapContent);
                //layoutParams.SetMargins(0, 0, 0, 10);
                groupView.LayoutParameters = layoutParams;
                groupView.ScreenChanged += this.screenChangeEventHandler;
                result = groupView;
            }
            return result;
        }
    }
}