using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class MvxAutoCompleteTextViewHidePopupOnDoneBinding : BaseBinding<MvxAutoCompleteTextView, object>
    {
        public MvxAutoCompleteTextViewHidePopupOnDoneBinding(MvxAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(MvxAutoCompleteTextView view, object value) { }

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.EditorAction += HandleEditorAction;
            }

            base.SubscribeToEvents();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && this.Target != null)
            {
                this.Target.EditorAction -= HandleEditorAction;
            }

            base.Dispose(isDisposing);
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done)
                return;

            Target.DismissDropDown();
        }
    }
}