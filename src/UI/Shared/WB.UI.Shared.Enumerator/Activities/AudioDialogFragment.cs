using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.FullFragging.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.Activities
{
    public class AudioDialogFragment : MvxDialogFragment<AudioDialogViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            this.EnsureBindingContextIsSet(savedInstanceState);
            return this.BindingInflate(Resource.Layout.interview_question_audio_dialog, container, false);
        }
        
    }
}