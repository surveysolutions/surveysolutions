#nullable enable

using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxDialogFragmentPresentation]
    [Register(nameof(CalendarEventDialog))]
    public class CalendarEventDialog : MvxDialogFragment<CalendarEventDialogViewModel>
    {
        public CalendarEventDialog()
        {
        }

        protected CalendarEventDialog(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.calendar_event_dialog, null);

            return view;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            HideKeyboard();

            base.OnDismiss(dialog);
        }
        
        public override void OnStop()
        {
            HideKeyboard();

            base.OnStop();
        }

        private void HideKeyboard()
        {
            IMvxAndroidCurrentTopActivity topActivity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = topActivity.Activity;

            activity.RemoveFocusFromEditText();
            activity.HideKeyboard(View.WindowToken);
        }
    }
}
