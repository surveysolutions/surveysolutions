using System;
using Android.Text;
using Android.Widget;
using MvvmCross.Commands;
using MvvmCross.Platforms.Android.Binding.Target;
using MvvmCross.WeakSubscription;


namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextChangedBinding : MvxAndroidTargetBinding
    {
        private IMvxCommand command;
        private IDisposable subscription;

        protected new EditText Target => (EditText)base.Target;

        public EditTextChangedBinding(EditText androidControl) : base(androidControl)
        {
            var target = Target;
            if (target == null)
                return;
            
            subscription = target.WeakSubscribe<EditText, TextChangedEventArgs>(
                nameof(target.TextChanged),
                Target_TextChanged);
        }
        
        private void Target_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Target != null)
            {
                this.command?.Execute(string.Concat(e.Text));
            }
        }

        public override Type TargetType => typeof(IMvxCommand);

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.subscription?.Dispose();
                this.subscription = null;
            }
            
            base.Dispose(isDisposing);
        }

        protected override void SetValueImpl(object target, object value)
        {
            this.command = (IMvxCommand)value;
        }
    }
}
