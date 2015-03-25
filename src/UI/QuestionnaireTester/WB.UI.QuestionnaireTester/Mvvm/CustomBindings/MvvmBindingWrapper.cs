using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{
    public abstract class MvvmBindingWrapper<T> : MvxAndroidTargetBinding where T : class
    {
        protected MvvmBindingWrapper(T control)
            : base(control) {}

        protected new  T Target
        {
            get { return base.Target as T; }
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (this.Target == null)
                return;
            this.SetValueToView(this.Target, value);
        }

        protected abstract void SetValueToView(T view, object value);
    }
}