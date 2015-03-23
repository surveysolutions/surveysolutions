using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Cirrious.MvvmCross.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SearchViewQueryTextSubmitBinding : MvxAndroidTargetBinding
    {
        protected new SearchView Target
        {
            get { return (SearchView)base.Target; }
        }

        public SearchViewQueryTextSubmitBinding(SearchView target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Target.QueryTextSubmit += TargetChanged;
        }

        private void TargetChanged(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            if (Target == null)
                return;

            if (Command == null)
                return;

            if (!Command.CanExecute())
                return;

            e.Handled = true;
            Command.Execute(e.Query);
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
                    Target.QueryTextSubmit -= TargetChanged;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
