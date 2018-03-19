using MvvmCross.Binding;
using WB.Core.SharedKernels.Enumerator.Properties;
using Xamarin.Controls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class SignaturePadSettingsBinding : BaseBinding<SignaturePadView, object>
    {
        public SignaturePadSettingsBinding(SignaturePadView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(SignaturePadView control, object value)
        {
            control.SignaturePromptText = string.Empty;
            control.CaptionText = UIResources.Interview_SignatureSignHere;
            control.ClearLabelText = UIResources.Interview_SignatureClear;
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;
    }
}
