using System;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
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