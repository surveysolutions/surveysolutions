using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.ViewModels;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SearchViewQueryTextChangeBinding : MvvmBindingWrapper<SearchView>
    {

        public SearchViewQueryTextChangeBinding(SearchView target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Target.QueryTextChange += TargetChanged;
        }

        private void TargetChanged(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            if (this.Target == null)
                return;

            if (Command == null)
                return;

            if (!Command.CanExecute())
                return;

            e.Handled = true;
            Command.Execute(e.NewText);
        }
        
        protected override void SetValueToView(SearchView view, object value)
        {
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
                if (this.Target != null)
                {
                    Target.QueryTextChange -= TargetChanged;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
