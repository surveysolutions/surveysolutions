using System;
using Android.Graphics;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class BoldWhenCheckedBinding
     : MvxAndroidTargetBinding
    {
        private readonly CheckBox checkBox;
        private bool currentValue;

        public BoldWhenCheckedBinding(CheckBox checkBox) : base(checkBox)
        {
            this.checkBox = checkBox;
        }

        private void CheckBoxOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {
            currentValue = !currentValue;
            SetFondWeight();
            FireValueChanged(currentValue);
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (target != null)
            {
                var boolValue = (bool) value;
                currentValue = boolValue;
                SetFondWeight();
            }
        }

        private void SetFondWeight()
        {
            if (currentValue)
            {
                checkBox.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
            }
            else
            {
                checkBox.SetTypeface(Typeface.Default, TypefaceStyle.Normal);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                checkBox.CheckedChange -= CheckBoxOnCheckedChange;
            }
            base.Dispose(isDisposing);
        }

        public override Type TargetType
        {
            get { return typeof(bool); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }
    }
}