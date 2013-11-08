

using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.Capi.Shared.Bindings
{
    public abstract class MvvmBindingWrapper<T> : MvxAndroidTargetBinding where T : class
    {
        protected MvvmBindingWrapper(T control)
            : base(control) {}

        private T View
        {
            get { return this.Target as T; }
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (this.View == null)
                return;
            this.SetValueToView(this.View, value);
        }

        protected abstract void SetValueToView(T view, object value);
    }
}