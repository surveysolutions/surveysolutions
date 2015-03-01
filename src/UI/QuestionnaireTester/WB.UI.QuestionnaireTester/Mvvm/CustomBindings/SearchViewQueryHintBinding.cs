using System;
using Android.Runtime;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Java.Lang;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{

    public class SearchViewQueryHintBinding : MvxAndroidTargetBinding
    {
        protected new SearchView Target
        {
            get { return (SearchView)base.Target; }
        }

        public SearchViewQueryHintBinding(SearchView target)
            : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (Target == null)
                return;

            Target.SetQueryHint((string)value);
        }

        public override Type TargetType
        {
            get { return typeof (string); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }
    }
}
