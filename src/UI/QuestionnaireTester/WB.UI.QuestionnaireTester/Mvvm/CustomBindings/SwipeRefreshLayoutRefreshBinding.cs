using System;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.ViewModels;
using WB.UI.QuestionnaireTester.Controls;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SwipeRefreshLayoutRefreshBinding : MvvmBindingWrapper<MvxSwipeRefreshLayout>
    {
        public SwipeRefreshLayoutRefreshBinding(MvxSwipeRefreshLayout target)
            : base(target)
        {
        }

        protected override void SetValueToView(MvxSwipeRefreshLayout view, object value)
        {
            Command = (IMvxCommand)value;
        }

        public override void SubscribeToEvents()
        {
            Target.Refresh += TargetRefreshChanged;
        }

        private void TargetRefreshChanged(object sender, EventArgs eventArgs)
        {
            if (this.Target == null)
                return;

            if (Command == null)
                return;

            if (!Command.CanExecute())
                return;

            Command.Execute();
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
