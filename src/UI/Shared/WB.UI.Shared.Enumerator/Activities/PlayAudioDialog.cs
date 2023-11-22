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
    [Register(nameof(PlayAudioDialog))]
    public class PlayAudioDialog : BaseFragmentDialog<PlayAudioViewModel>
    {
        public PlayAudioDialog()
        {
        }

        protected PlayAudioDialog(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override string? Title => ViewModel?.Title;
        protected override int LayoutFragmentId => Resource.Layout.play_audio_dialog;
    }
}
