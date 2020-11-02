#nullable enable

using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
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


        // protected override void OnCreate(Bundle? savedInstanceState)
        // {
        //     base.OnCreate(savedInstanceState);
        //     //this.RequestWindowFeature(Window.FEATURE_NO_TITLE);
        //
        //     SetContentView(Resource.Layout.calendar_event_dialog);
        // }

        // public IMvxCommand CancelClick => new MvxCommand(() => Dismiss()); 
        //
        // public IMvxCommand OkClick => new MvxCommand(() => (this.Context as Activity)?.Finish());
    }
}
