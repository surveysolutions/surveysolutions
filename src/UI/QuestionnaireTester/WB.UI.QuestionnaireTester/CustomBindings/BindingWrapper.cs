using System;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public abstract class BindingWrapper<TAndroidControlType, TBindingType> : MvxAndroidTargetBinding where TAndroidControlType : class
    {
        protected BindingWrapper(TAndroidControlType androidControl)
            : base(androidControl) {}

        protected new  TAndroidControlType Target
        {
            get { return base.Target as TAndroidControlType; }
        }

        public override Type TargetType
        {
            get { return typeof(TBindingType); }
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (this.Target == null)
                return;
            this.SetValueToView(this.Target, (TBindingType)value);
        }

        protected abstract void SetValueToView(TAndroidControlType androidControl, TBindingType value);
    }
}