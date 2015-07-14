using System;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public abstract class BaseBinding<TControl, TValue> : MvxAndroidTargetBinding
        where TControl : class
    {
        protected BaseBinding(TControl androidControl)
            : base(androidControl) {}

        protected new TControl Target
        {
            get { return base.Target as TControl; }
        }

        public override Type TargetType
        {
            get { return typeof(TValue); }
        }

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