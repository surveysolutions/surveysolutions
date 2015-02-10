using System;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.ViewModels;
using WB.UI.QuestionnaireTester.Controls;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SwipeRefreshLayoutRefreshBinding : MvxAndroidTargetBinding
    {
        protected new MvxSwipeRefreshLayout Target
        {
            get { return (MvxSwipeRefreshLayout)base.Target; }
        }

        public SwipeRefreshLayoutRefreshBinding(MvxSwipeRefreshLayout target)
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

            if (Command == null)
                return;

            if (!Command.CanExecute())
                return;

            Command.Execute();
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (Target == null)
                return;

            Command = (IMvxCommand)value;
        }

        private IMvxCommand Command;

        public override Type TargetType
        {
            get { return typeof (IMvxCommand); }
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
