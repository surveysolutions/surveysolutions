using System;
using System.Reflection;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Target;
using MvvmCross.Platform.Droid.WeakSubscription;
using MvvmCross.Platform.Platform;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewSelectedObjectTargetBinding
        : MvxAndroidPropertyInfoTargetBinding<InstantAutoCompleteTextView>
    {
        private IDisposable _subscription;

        public InstantAutoCompleteTextViewSelectedObjectTargetBinding(object target, PropertyInfo targetPropertyInfo)
            : base(target, targetPropertyInfo)
        {
            var autoComplete = this.View;
            if (autoComplete == null)
            {
                MvxBindingTrace.Trace(MvxTraceLevel.Error,
                    "Error - autoComplete is null in InstantAutoCompleteTextViewSelectedObjectTargetBinding");
            }
        }

        private void AutoCompleteOnSelectedObjectChanged(object sender, EventArgs eventArgs)
        {
            this.FireValueChanged(this.View.SelectedObject);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWayToSource;

        public override void SubscribeToEvents()
        {
            var autoComplete = this.View;

            if (autoComplete == null)
                return;

            this._subscription = autoComplete.WeakSubscribe(
                nameof(autoComplete.SelectedObjectChanged),
                AutoCompleteOnSelectedObjectChanged);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this._subscription?.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}