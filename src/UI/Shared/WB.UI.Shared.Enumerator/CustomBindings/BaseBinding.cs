using System;
using MvvmCross.Platforms.Android.Binding.Target;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public abstract class BaseBinding<TControl, TValue> : MvxAndroidTargetBinding
        where TControl : class
    {
        protected BaseBinding(TControl androidControl)
            : base(androidControl) {}

        protected new TControl Target => base.Target as TControl;

        public override Type TargetType => typeof(TValue);

        protected override void SetValueImpl(object target, object value)
        {
            var control = target as TControl;

            if (control == null)
                return;

            this.SetValueToView(control, (TValue) value);
        }

        protected abstract void SetValueToView(TControl control, TValue value);
    }
}
