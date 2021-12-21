using System;
using Android.Widget;
using MvvmCross.Commands;
using MvvmCross.Platforms.Android.Binding.Target;


namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextChangedBinding : MvxAndroidTargetBinding
    {
        private WeakReference<IMvxCommand> Command;

        protected new EditText Target => (EditText)base.Target;

        public EditTextChangedBinding(EditText androidControl) : base(androidControl)
        {
            this.Target.TextChanged += Target_TextChanged;
        }

        private void Target_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (this.Target != null && this.Command != null && this.Command.TryGetTarget(out var command))
            {
                command.Execute(string.Concat(e.Text));
            }
        }

        public override Type TargetType => typeof(IMvxCommand);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                if (this.Target != null)
                {
                    this.Target.TextChanged -= this.Target_TextChanged;
                }

                Command = null;
            }
        }

        protected override void SetValueImpl(object target, object value)
        {
            this.Command = new WeakReference<IMvxCommand>((IMvxCommand)value);
        }
    }
}
