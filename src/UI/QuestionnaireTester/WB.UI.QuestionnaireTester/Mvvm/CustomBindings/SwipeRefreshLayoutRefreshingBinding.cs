using System;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using WB.UI.QuestionnaireTester.Controls;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SwipeRefreshLayoutRefreshingBinding : MvxAndroidTargetBinding
    {
        protected new MvxSwipeRefreshLayout Target
        {
            get { return (MvxSwipeRefreshLayout)base.Target; }
        }

        public SwipeRefreshLayoutRefreshingBinding(MvxSwipeRefreshLayout target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Target.Refresh += TargetRefreshChanged;
        }

        private void TargetRefreshChanged(object sender, EventArgs eventArgs)
        {
            if (Target == null)
                return;

            FireValueChanged(Target.Refreshing);
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (Target == null)
                return;

            Target.Refreshing = (bool)value;
        }

        public override Type TargetType
        {
            get { return typeof (bool); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (Target != null)
                {
                    Target.Refresh -= TargetRefreshChanged;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
