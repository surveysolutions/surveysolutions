using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SearchViewQueryHintBinding : MvvmBindingWrapper<SearchView>
    {

        public SearchViewQueryHintBinding(SearchView target)
            : base(target)
        {
        }

        protected override void SetValueToView(SearchView view, object value)
        {
            view.SetQueryHint((string)value);
        }

        public override Type TargetType
        {
            get { return typeof (string); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}
