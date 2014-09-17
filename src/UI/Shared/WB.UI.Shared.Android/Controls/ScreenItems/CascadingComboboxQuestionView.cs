using System;
using Android.Content;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class CascadingComboboxQuestionView : FilteredComboboxQuestionView
    {
        public CascadingComboboxQuestionView(
            Context context,
            IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source,
            Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }
    }
}