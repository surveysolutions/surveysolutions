using System;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class AutoPropagateQuestionView : NumericIntegerQuestionView
    {
        public AutoPropagateQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source, 
            Guid questionnairePublicKey, 
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override InputTypes KeyboardTypeFlags
        {
            get { return InputTypes.ClassNumber; }
        }
    }
}